using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Vector3 mouseWorldPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetButtonDown("Fire1"))
        {
            TileGrid.instance.TrySet(mouseWorldPos);
        }
        if(Input.GetButtonDown("Fire2"))
        {
            // rotate
            TileGrid.instance.Rotate(1);
        }
        if(Input.GetKeyDown(KeyCode.Q))
        {
            TileGrid.instance.Rotate(-1);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            TileGrid.instance.Rotate(1);
        }
    }
}
