using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileColour { Empty, Red, Orange, Green, Blue, Purple, Rainbow, Black, White };
public class ColourData
{

    public static readonly Dictionary<TileColour, Color> Colours = new Dictionary<TileColour, Color>()
    {
        { TileColour.Empty, new Color(0, 0, 0, 0) },
        { TileColour.Red, Color.red},
        { TileColour.Orange, new Color(1, 1, 0, 1) },
        { TileColour.Green, Color.green },
        { TileColour.Blue, Color.blue },
        { TileColour.Purple, new Color(1,0,1,1) },
        { TileColour.Rainbow, new Color(0.5f, 1, 0.5f, 0.5f) },
        { TileColour.Black, Color.black },
        { TileColour.White, Color.white },
    };
}
