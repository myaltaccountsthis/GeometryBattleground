using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileGeneration : MonoBehaviour
{
    private Tilemap tileMap;
    private Vector3Int oldUnitPos;

    public int tileExtent;
    public TileBase groundTile;

    private void UpdatePosition() {
        transform.position = oldUnitPos;
    }

    // Start is called before the first frame update
    void Start()
    {
        tileMap = GetComponent<Tilemap>();
        oldUnitPos = Vector3Int.zero;

        float xExtent = Camera.main.orthographicSize * Screen.width / Screen.height + tileExtent;
        float yExtent = Camera.main.orthographicSize + tileExtent;
        Vector3Int combinedExtent = new Vector3Int((int) xExtent, (int) yExtent, 0);
        BoundsInt bounds = new BoundsInt(-combinedExtent, 2 * combinedExtent);
        TileBase[] tiles = tileMap.GetTilesBlock(bounds);
        for (int x = 0; x < bounds.size.x; x++) {
            for (int y = 0; y < bounds.size.y; y++) {
                tileMap.SetTile(new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0), groundTile);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3Int newUnitPos = new Vector3Int((int) Camera.main.transform.position.x, (int) Camera.main.transform.position.y, 0);
        if (newUnitPos != oldUnitPos) {
            oldUnitPos = newUnitPos;
            UpdatePosition();
        }
    }
}
