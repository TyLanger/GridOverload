using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// grabbed a bunch of the tetris functionality from
// https://www.youtube.com/watch?v=ODLzYI4d-J8&ab_channel=Zigurous

public class TileGrid : MonoBehaviour
{
    int gridSize = 10;
    float tileSpacing = 1; // if I change this from 1, might break tile selection GetTileFromPosition()

    Vector2 gridBottomLeft;
    Vector2 gridCenter;

    public Tile tilePrefab;
    Tile[,] tiles;
    public PreviewShape previewShape;

    Shape currentShape;

    public Shape[] shapes;

    public static TileGrid instance;

    void Awake()
    {
        if (instance)
            return;
        instance = this;

        float offset = (gridSize-1) / 2f;
        gridCenter = new Vector2(transform.position.x, transform.position.y);
        gridBottomLeft = gridCenter + new Vector2(-offset, -offset);
        // center should be 0,0. BL should be -4.5,-4.5?
        //Debug.Log($"Center: {gridCenter} BL: {gridBottomLeft}");

        tiles = new Tile[gridSize, gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                Vector2 position = gridBottomLeft + new Vector2(i * tileSpacing, j * tileSpacing);
                Tile blankTile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                blankTile.name = $"Tile {i},{j}";
                blankTile.SetPosition(i, j);
                tiles[i, j] = blankTile;
            }
        }

        for (int i = 0; i < shapes.Length; i++)
        {
            shapes[i].Initialize();
        }

        currentShape = GenerateShape();
        
    }

    public Tile GetTileFromPosition(float x, float y)
    {
        // mouse clicks at (-4.6, -4.7) and that maps to tiles[0,0] at (-4.5, -4.5)
        // -5.0 to -4.001 should map to -4.5 which maps to 0
        // -4.0 to -3.001 should map to -3.5 which maps to 0
        // top right is (4.5,4.5)
        // += 4.5 should be right
        // will give -0.5 to +0,5 for 0
        // 8.5 to 9.5 for 9

        float offset = (gridSize - 1) * 0.5f;
        x += offset;
        y += offset;
        int xInt = Mathf.RoundToInt(x);
        int yInt = Mathf.RoundToInt(y);

        // check bounds
        if(IsInBounds(xInt, yInt))
        {
            return tiles[xInt, yInt];
        }
        else
        {
            //clamp?
            //Debug.Log($"Had to clamp. ({x},{y}) ({xInt},{yInt})");
            xInt = Mathf.Clamp(xInt, 0, gridSize - 1);
            yInt = Mathf.Clamp(yInt, 0, gridSize - 1);
            return tiles[xInt, yInt];
        }
    }

    bool IsInBounds(int x, int y)
    {
        if (x < 0 || y < 0)
            return false;
        if (x >= gridSize || y >= gridSize)
            return false;
        return true;
    }

    bool CanPlace(Shape s, Tile t)
    {
        for (int i = 0; i < s.cells.Length; i++)
        {
            Vector2Int pos = t.GetPosition();
            pos += s.cells[i];
            if(!IsInBounds(pos.x, pos.y))
            {
                return false;
            }
            if(!tiles[pos.x, pos.y].isEmpty)
            {
                return false;
            }

        }
        return true;
    }

    public bool TrySet(Vector3 worldPosition)
    {
        return Set(GetTileFromPosition(worldPosition.x, worldPosition.y));
    }

    public bool Set(Tile t)
    {

        return Set(currentShape, t, currentShape.colour);
    }

    public bool Set(Shape shape, Tile tile, TileColour colour)
    {
        bool placeable = CanPlace(shape, tile);

        if (placeable)
        {
            for (int i = 0; i < shape.cells.Length; i++)
            {
                Vector2Int pos = tile.GetPosition() + shape.cells[i];
                tiles[pos.x, pos.y].SetColour(colour);
                shape.AddMember(tiles[pos.x, pos.y]);
                tiles[pos.x, pos.y].UpdateShape(shape);
                tiles[pos.x, pos.y].FillTile();

            }
            Vector2Int centerPos = tile.GetPosition();
            shape.SetHashCode(((centerPos.x+1) ^ (centerPos.y+1))*(int)shape.tetromino);
            CheckMatch3(tile);
            //Evaluate(currentShape, tile);

            currentShape = GenerateShape();
        }
        else
        {
            //Debug.Log($"Nope {shape.tetromino}");
        }
        return placeable;
    }

    void Evaluate(Shape shape, Tile tile)
    {
        // check for match 3
        CheckMatch3(tile);
        
    }

    void CheckMatch3(Tile tile)
    {
        Vector2Int tilePos = tile.GetPosition();
        Shape shape = tile.GetShape();

        //HashSet<Shape> shapesToCheck = new HashSet<Shape>();
        HashSet<Shape> shapesChecked = new HashSet<Shape>();
        Stack<Shape> stackToCheck = new Stack<Shape>();
        stackToCheck.Push(shape);


        int count = 0;
        while (stackToCheck.Count > 0)
        {
            count++;
            if (count > 100)
            {
                Debug.Log("Infinite loop?");
                break;
            }
            Shape currentShape = stackToCheck.Pop();
            shapesChecked.Add(currentShape);

            foreach (Tile t in currentShape.GetMembers())
            {
                Vector2Int currentTilePosition = t.GetPosition();
                Vector2Int northNeighbourPos = currentTilePosition + new Vector2Int(0, 1);
                Vector2Int eastNeighbourPos = currentTilePosition + new Vector2Int(1, 0);
                Vector2Int southNeighbourPos = currentTilePosition + new Vector2Int(0, -1);
                Vector2Int westNeighbourPos = currentTilePosition + new Vector2Int(-1, 0);

                // these could probably all be method(x,y,currentShape)
                if(IsInBounds(northNeighbourPos.x, northNeighbourPos.y))
                {
                    Tile northTile = tiles[northNeighbourPos.x, northNeighbourPos.y];
                    Shape northShape = northTile.GetShape();
                    bool coloursMatch = northShape.colour == currentShape.colour;
                    bool shapesMatch = northShape.Equals(currentShape);
                    bool alreadyChecked = shapesChecked.Contains(northShape);

                    if (!alreadyChecked && coloursMatch && !shapesMatch)
                    {
                        stackToCheck.Push(northTile.GetShape());
                    }
                }
                if (IsInBounds(eastNeighbourPos.x, eastNeighbourPos.y))
                {
                    Tile eastTile = tiles[eastNeighbourPos.x, eastNeighbourPos.y];
                    Shape eastShape = eastTile.GetShape();
                    bool coloursMatch = eastShape.colour == currentShape.colour;
                    bool shapesMatch = eastShape.Equals(currentShape);
                    bool alreadyChecked = shapesChecked.Contains(eastShape);

                    if (!alreadyChecked && coloursMatch && !shapesMatch)
                    {
                        stackToCheck.Push(eastTile.GetShape());
                    }
                }
                if (IsInBounds(southNeighbourPos.x, southNeighbourPos.y))
                {
                    Tile southTile = tiles[southNeighbourPos.x, southNeighbourPos.y];
                    Shape southShape = southTile.GetShape();
                    bool coloursMatch = southShape.colour == currentShape.colour;
                    bool shapesMatch = southShape.Equals(currentShape);
                    bool alreadyChecked = shapesChecked.Contains(southShape);

                    if (!alreadyChecked && coloursMatch && !shapesMatch)
                    {
                        stackToCheck.Push(southTile.GetShape());
                    }
                }
                if (IsInBounds(westNeighbourPos.x, westNeighbourPos.y))
                {
                    Tile westTile = tiles[westNeighbourPos.x, westNeighbourPos.y];
                    Shape westShape = westTile.GetShape();
                    bool coloursMatch = westShape.colour == currentShape.colour;
                    bool shapesMatch = westShape.Equals(currentShape);
                    bool alreadyChecked = shapesChecked.Contains(westShape);

                    if (!alreadyChecked && coloursMatch && !shapesMatch)
                    {
                        stackToCheck.Push(westTile.GetShape());
                    }
                }

            }
        }
        Debug.Log($"Matches (match3): {shapesChecked.Count}");
        foreach (Shape s in shapesChecked)
        {
            s.Match();
        }

    }

    Shape GenerateShape()
    {
        int randColour = Random.Range(1, 6);
        int randShape = Random.Range(0, shapes.Length);

        Shape s = new Shape
        {
            tetromino = (Tetromino)randShape,
            colour = (TileColour)randColour
        };
        s.Initialize();

        //Debug.Log($"Generated a {s.colour} {s.tetromino}");

        previewShape.SetupCurrentShape(s.tetromino, s.colour);

        return s;
    }

}
