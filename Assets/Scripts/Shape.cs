using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Shape : IEquatable<Shape>
{

    Tile[] members;
    int numMembers;

    int hashCode;

    public Tetromino tetromino;

    public Vector2Int[] cells;
    public TileColour colour;


    public void Initialize()
    {
        cells = PieceData.Cells[tetromino];
        members = new Tile[cells.Length];
        numMembers = 0;
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
        if (numMembers < members.Length)
        {
            members[numMembers] = newMember;
            numMembers++;
        }
    }

    public void RemoveMember(Tile member)
    {
        for (int i = 0; i < members.Length; i++)
        {
            if (members[i] == member)
            {
                members[i] = null;
                numMembers--;
            }
        }
    }

    public Tile[] GetMembers()
    {
        return members;
    }

    public void Match()
    {
        string memberString = "";
        for (int i = 0; i < members.Length; i++)
        {
            Vector2Int pos = members[i].GetPosition();
            memberString += $"({pos.x},{pos.y}) ";
        }
        Debug.Log($"Match shape: {tetromino} Members: {memberString}");
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
