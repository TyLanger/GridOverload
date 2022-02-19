using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Debug.Log($"Mouse: {mousePos}");
            Tile t = TileGrid.instance.GetTileFromPosition(mousePos.x, mousePos.y);
            //Debug.Log($"Tile t: {t.name}");
            TileGrid.instance.Set(t);
        }
    }
}
