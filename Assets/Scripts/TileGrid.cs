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
            Debug.Log($"Had to clamp. ({x},{y}) ({xInt},{yInt})");
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
                tiles[pos.x, pos.y].UpdateShape(shape);
                tiles[pos.x, pos.y].FillTile();

                
            }
            currentShape = GenerateShape();
        }
        else
        {
            //Debug.Log($"Nope {shape.tetromino}");
        }
        return placeable;
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
