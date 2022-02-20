using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Shape : IEquatable<Shape>
{

    HashSet<Tile> members;

    int hashCode;

    public Tetromino tetromino;

    public Vector2Int[] cells;
    public TileColour colour;


    public void Initialize()
    {
        cells = PieceData.Cells[tetromino];
        members = new HashSet<Tile>();
    }

    public void SetHashCode(int h)
    {
        // (center.x+1 ^ center.y+1) * (int) tetro
        // +1 for no 0s
        // is this unique enough?
        // another shape in the same location would have the same hash
        // but for it to be able to fit, the old one would have to be entirely gone
        hashCode = h;
    }

    public void AddMember(Tile newMember)
    {
        members.Add(newMember);
    }

    public void RemoveMember(Tile member)
    {
        if (members.Contains(member))
        {
            members.Remove(member);
        }
        // check if the shape should be split in 2
        tetromino = Tetromino.none;
        CheckShapeIntegrity();
    }

    void CheckShapeIntegrity()
    {
        foreach (Tile t in members)
        {
            if(!HasNeighbour(t))
            {
                // create your own shape
                Shape newShape = new Shape();
                newShape.Initialize();
                newShape.colour = colour;
                newShape.tetromino = Tetromino.none;
                newShape.SetHashCode(hashCode + 1);
                Vector2Int[] newCells = new Vector2Int[1]; 
                newCells[0] = new Vector2Int(0, 0);
                newShape.cells = newCells;  // cells are only used for placing. Probs don't need this
                newShape.AddMember(t);
                t.UpdateShape(newShape);
            }
        }
    }

    bool HasNeighbour(Tile t)
    {
        Vector2Int tilePos = t.GetPosition();
        foreach (Tile tile in members)
        {
            if (t == tile)
                continue;
            Vector2Int comparePos = tile.GetPosition();
            int xDiff = Mathf.Abs(tilePos.x - comparePos.x);
            int yDiff = Mathf.Abs(tilePos.y - comparePos.y);

            // if one is 1, the other is 0, you are neighbours
            if((xDiff == 1 && yDiff == 0) || (xDiff == 0 && yDiff == 1))
            {
                return true;
            }
        }
        return false;
    }

    public HashSet<Tile> GetMembers()
    {
        return members;
    }

    public void DestroyShape()
    {
        foreach (Tile t in members)
        {
            t.ResetTile();
        }
    }

    public bool Equals(Shape other)
    {
        return members == other.members;
    }

    public override int GetHashCode()
    {
        return hashCode;
    }
}
