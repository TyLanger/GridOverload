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

    int tetrisLength = 6;
    int matchNumber = 3;

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
            //CheckMatch3(tile);
            Evaluate(tile);
            //Evaluate(currentShape, tile);

            currentShape = GenerateShape();
        }
        else
        {
            //Debug.Log($"Nope {shape.tetromino}");
        }
        return placeable;
    }

    void Evaluate(Tile tile)
    {
        // check for match 3
        HashSet<Tile> tilesToDestroy = CheckLine(tile);
        HashSet<Shape> shapesToDestroy = CheckMatch3(tile);

        Debug.Log($"Removal tiles/shapes: {tilesToDestroy.Count} {shapesToDestroy.Count}");

        foreach (Shape s in shapesToDestroy)
        {
            s.DestroyShape();
        }
        RemoveTiles(tilesToDestroy);
        
    }

    HashSet<Shape> CheckMatch3(Tile tile)
    {
        Vector2Int tilePos = tile.GetPosition();
        Shape shape = tile.GetShape();

        //HashSet<Shape> shapesToCheck = new HashSet<Shape>();
        HashSet<Shape> shapesChecked = new HashSet<Shape>();
        Stack<Shape> stackToCheck = new Stack<Shape>();
        stackToCheck.Push(shape);

        // only check full shapes towards the match 3
        // but still delete broken shapes if they're nearby
        int brokenShapes = 0;
        

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

            // if a tile has the same shape as 2 of its neighbours, it can be added twice
            // Like an O block and a S/Z block. The corners of the O can fit into the corner of the S/Z
            // and make the O block have 2 neighbours that are both the same block
            if(shapesChecked.Contains(currentShape))
            {
                if (stackToCheck.Count == 0)
                    break;
                currentShape = stackToCheck.Pop();
            }

            shapesChecked.Add(currentShape);
            Debug.Log($"Adding shape: {currentShape.tetromino}");
            if (currentShape.tetromino == Tetromino.none)
            {
                brokenShapes++;
            }

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
                    if (northTile.HasShape())
                    {
                        Shape northShape = northTile.GetShape();
                        bool coloursMatch = northShape.colour == currentShape.colour;
                        bool shapesMatch = northShape.Equals(currentShape);
                        bool alreadyChecked = shapesChecked.Contains(northShape);

                        if (!alreadyChecked && coloursMatch && !shapesMatch)
                        {
                            stackToCheck.Push(northTile.GetShape());
                        }
                    }
                }
                if (IsInBounds(eastNeighbourPos.x, eastNeighbourPos.y))
                {
                    Tile eastTile = tiles[eastNeighbourPos.x, eastNeighbourPos.y];
                    if (eastTile.HasShape())
                    {
                        Shape eastShape = eastTile.GetShape();
                        bool coloursMatch = eastShape.colour == currentShape.colour;
                        bool shapesMatch = eastShape.Equals(currentShape);
                        bool alreadyChecked = shapesChecked.Contains(eastShape);

                        if (!alreadyChecked && coloursMatch && !shapesMatch)
                        {
                            stackToCheck.Push(eastTile.GetShape());
                        }
                    }
                }
                if (IsInBounds(southNeighbourPos.x, southNeighbourPos.y))
                {
                    Tile southTile = tiles[southNeighbourPos.x, southNeighbourPos.y];
                    if (southTile.HasShape())
                    {
                        Shape southShape = southTile.GetShape();
                        bool coloursMatch = southShape.colour == currentShape.colour;
                        bool shapesMatch = southShape.Equals(currentShape);
                        bool alreadyChecked = shapesChecked.Contains(southShape);

                        if (!alreadyChecked && coloursMatch && !shapesMatch)
                        {
                            stackToCheck.Push(southTile.GetShape());
                        }
                    }
                }
                if (IsInBounds(westNeighbourPos.x, westNeighbourPos.y))
                {
                    Tile westTile = tiles[westNeighbourPos.x, westNeighbourPos.y];
                    if (westTile.HasShape())
                    {
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
        }
        //Debug.Log($"Matches (match3): {shapesChecked.Count}");
        Debug.Log($"Loop count: {count}");
        // shouldn't destroy until done all evaluations
        if((shapesChecked.Count - brokenShapes) >= matchNumber)
        {
            // match 3
            // destroy those shapes
            Debug.Log($"brokenShapes/Count: {brokenShapes}/{shapesChecked.Count}");
            return shapesChecked;
            
        }
        return new HashSet<Shape>();
    }

    HashSet<Tile> CheckLine(Tile tile)
    {
        // for each tile in the shape, check 10 to the left, right, up, down to see if there's a line of 10

        Vector2Int tilePos = tile.GetPosition();
        Shape shape = tile.GetShape();

        HashSet<Tile> tilesToRemove = new HashSet<Tile>();

        foreach (Tile t in shape.GetMembers())
        {
            
            // N/S
            Vector2Int currentTilePos = t.GetPosition();
            Vector2Int northPos = currentTilePos + new Vector2Int(0, 1);

            Tile topTile = tile;
            int northCount = 0;
            Tile bottomTile = tile;
            int southCount = 0;
            if (IsInBounds(northPos.x, northPos.y))
            {
                Tile northTile = tiles[northPos.x, northPos.y];
                int count = 1;
                bool outOfBounds = false;
                while(!northTile.isEmpty)
                {
                    count++;
                    if (count > 100)
                        break;

                    if (IsInBounds(northPos.x, northPos.y + 1))
                    {
                        northTile = tiles[northPos.x, northPos.y + 1];
                        northPos = northTile.GetPosition();
                    }
                    else
                    {
                        outOfBounds = true;
                        break;
                    }
                }
                //Debug.Log($"North line length: {currentTilePos} {count}");
                // only decrement if it ended on an empty tile, not if it reached the edge
                if (!outOfBounds)
                {
                    topTile = tiles[northPos.x, northPos.y - 1];
                }
                else
                {
                    topTile = tiles[northPos.x, northPos.y];
                }
                northCount = count;
            }

            Vector2Int southPos = currentTilePos + new Vector2Int(0, -1);

            if(IsInBounds(southPos.x, southPos.y))
            {
                Tile southTile = tiles[southPos.x, southPos.y];
                int count = 0;
                bool outOfBounds = false;
                while (!southTile.isEmpty)
                {
                    count++;
                    if (count > 100)
                        break;

                    if(IsInBounds(southPos.x, southPos.y-1))
                    {
                        southTile = tiles[southPos.x, southPos.y - 1];
                        southPos = southTile.GetPosition();
                    }
                    else
                    {
                        outOfBounds = true;
                        break;
                    }
                }
                //Debug.Log($"South line length: {currentTilePos} {count}");
                // only decrement if it ended on an empty tile, not if it reached the edge
                if (!outOfBounds)
                {
                    bottomTile = tiles[southPos.x, southPos.y + 1];
                }
                else
                {
                    bottomTile = tiles[southPos.x, southPos.y];

                }
                southCount = count;
            }

            Debug.Log($"Line: {currentTilePos} {northCount} + {southCount} {topTile.GetPosition()}-{bottomTile.GetPosition()}");
            if((northCount+southCount) >= tetrisLength)
            {
                AddTileLineVertical(tilesToRemove, topTile, bottomTile);
                
            }
        }
        return tilesToRemove;
    }

    void AddTileLineVertical(HashSet<Tile> set, Tile top, Tile bottom)
    {
        Debug.Log($"Top, bottom: {top.GetPosition()} {bottom.GetPosition()}");
        Tile current = top;
        int count = 0;
        while(current != bottom)
        {
            //Debug.Log($"Current: {current.GetPosition()}");
            count++;
            if(count > 9)
            {
                Debug.Log($"Out of bounds. Infinite loop");
            }    
            set.Add(current);
            Vector2Int currentPos = current.GetPosition();
            current = tiles[currentPos.x, currentPos.y - 1];
        }
        set.Add(bottom);
    }

    void RemoveTiles(HashSet<Tile> setOfTiles)
    {
        foreach (Tile t in setOfTiles)
        {
            t.DestroyTileInLine();
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
