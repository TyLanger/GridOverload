using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewShape : MonoBehaviour
{

    public Tile tilePrefab;

    Tetromino currentShape;
    TileColour currentColour;

    Transform tilesParent;

    public PlayerInput playerInput;

    // Start is called before the first frame update
    void Awake()
    {
        tilesParent = transform;
    }

    // Update is called once per frame
    void Update()
    {
        tilesParent.position = playerInput.mouseWorldPos + Vector3.forward*10; // mouse is at -10z which doesn't show on screen
    }

    public void SetupCurrentShape(Tetromino shape, TileColour colour)
    {
        currentShape = shape;
        currentColour = colour;

        DestroyPreview();
        CreatePreviewShape();
    }

    public void CreatePreviewShape()
    {
        // spawn the objects
        Vector2Int[] cells = PieceData.Cells[currentShape];

        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 pos = tilesParent.position + new Vector3(cells[i].x, cells[i].y, 0);
            Tile t = Instantiate(tilePrefab, pos, Quaternion.identity, tilesParent);
            t.SetColour(currentColour);
        }

    }

    public void DestroyPreview()
    {
        foreach (Transform child in tilesParent)
        {
            Destroy(child.gameObject);
        }
    }
}
