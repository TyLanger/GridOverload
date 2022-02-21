using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileColour { Empty, Red, Orange, Green, Blue, Purple, Rainbow, Black, White };
public class ColourData
{
    // Colours from
    // https://lospec.com/palette-list/endesga-32
    public static readonly Dictionary<TileColour, Color> Colours = new Dictionary<TileColour, Color>()
    {
        { TileColour.Empty, new Color(0, 0, 0, 0) },
        { TileColour.Red, new Color(162/255f, 38/255f, 51/255f)},
        { TileColour.Orange, new Color(247/255f, 118/255f, 34/255f, 1) },
        { TileColour.Green, new Color(62/255f, 137/255f, 72/255f) },
        { TileColour.Blue, new Color(18/255f, 78/255f, 137/255f) },
        { TileColour.Purple, new Color(181/255f, 80/255f, 136/255f, 1) },
        { TileColour.Rainbow, new Color(0.5f, 1, 0.5f, 0.5f) },
        { TileColour.Black, Color.black },
        { TileColour.White, Color.white },
    };
}
