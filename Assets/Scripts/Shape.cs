using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Shape
{

    Tile[] members;

    Tile centerTile;

    public Tetromino tetromino;

    public Vector2Int[] cells;
    public TileColour colour;


    public void Initialize()
    {
        cells = PieceData.Cells[tetromino];
    }
}
