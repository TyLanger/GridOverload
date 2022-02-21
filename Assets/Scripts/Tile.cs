using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tile : MonoBehaviour
{

    // Colour
    TileColour colour;
    Shape myShape;


    bool isHollow = false;
    public bool isEmpty = true; // this or an empty colour?
    bool hasShape = false;

    public bool hasBeenWinChecked = false;

    int gridX = 0;
    int gridY = 0;

    public Tile()
    {

    }

    public void SetPosition(int x, int y)
    {
        gridX = x;
        gridY = y;
    }

    public Vector2Int GetPosition()
    {
        return new Vector2Int(gridX, gridY);
    }

    public void FillTile()
    {
        isEmpty = false;
    }

    public void SetColour(TileColour newColour)
    {
        colour = newColour;
        GetComponent<SpriteRenderer>().color = ColourData.Colours[colour];
    }

    public void UpdateShape(Shape newShape)
    {
        myShape = newShape;
        hasShape = true;
    }

    public Shape GetShape()
    {
        return myShape;
    }

    public bool HasShape()
    {
        return hasShape;
    }

    public void ResetTile()
    {
        // called by the shape when the whole shape dies
        isEmpty = true;
        SetColour(TileColour.Empty);
        hasShape = false;
        myShape = new Shape();
        myShape.Initialize();
    }

    public void DestroyTileInLine()
    {
        // called by the line deletion

        myShape.RemoveMember(this);

        ResetTile();
    }

    public void DebugColour()
    {
        Color currentColour = GetComponent<SpriteRenderer>().color;
        GetComponent<SpriteRenderer>().color = new Color(currentColour.r, currentColour.g, currentColour.b, 0.75f);
    }

    public void ResetWinChecked()
    {
        hasBeenWinChecked = false;
    }
}
