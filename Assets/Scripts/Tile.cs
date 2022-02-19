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
    }

}
