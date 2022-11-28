using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    public int tileExtent;
    public TileBase groundTile;
    public Mob[] mobs;

    private Tilemap tileMap;
    private List<Mob> activeMobs;
    private BoundsInt bounds;

    void Start()
    {
        tileMap = GetComponent<Tilemap>();
        activeMobs = new List<Mob>();
        bounds = new BoundsInt();
    }

    void Update()
    {
        UpdateBounds();

        // generate tiles in view of the camera + tileExtent
        TileBase[] tiles = tileMap.GetTilesBlock(bounds);
        for (int x = 0; x < bounds.size.x; x++) {
            for (int y = 0; y < bounds.size.y; y++) {
                tileMap.SetTile(new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0), groundTile);
            }
        }

        // clear dead mobs
        for (int i = 0; i < activeMobs.Count; i++) {
            if (activeMobs[i] == null) {
                activeMobs.RemoveAt(i);
                i--;
            }
        }
        
        // do mob spawning
        int desiredCount = GetDesiredMobCount();
        for (int i = activeMobs.Count; i < desiredCount; i++) {
            SpawnMob();
        }
    }

    private void UpdateBounds() {
        float xExtent = Camera.main.orthographicSize * Screen.width / Screen.height + tileExtent;
        float yExtent = Camera.main.orthographicSize + tileExtent;
        Vector3Int combinedExtent = new Vector3Int((int) xExtent, (int) yExtent, 0);
        bounds = new BoundsInt(new Vector3Int((int) Camera.main.transform.position.x, (int) Camera.main.transform.position.y) - combinedExtent, 2 * combinedExtent);
    }

    // Returns a random location from the tileExtent region
    // TODO implement actual random spawning outside of camera view
    private Vector3 GetSpawnLocation() {
        float xExtent = Camera.main.orthographicSize * Screen.width / Screen.height;
        float yExtent = Camera.main.orthographicSize;
        Vector3Int cameraExtent = new Vector3Int((int) xExtent, (int) yExtent, 0);
        BoundsInt cameraBounds = new BoundsInt(new Vector3Int((int) Camera.main.transform.position.x, (int) Camera.main.transform.position.y) - cameraExtent, 2 * cameraExtent);
        int left = cameraBounds.xMin - bounds.xMin, right = bounds.xMax - cameraBounds.xMax;
        int bottom = cameraBounds.yMin - bounds.yMin, top = bounds.yMax - cameraBounds.yMax;
        int x = Random.Range(0, left + right), y = Random.Range(0, bottom + top);
        if (x < left)
            x += bounds.xMin;
        else
            x += cameraBounds.xMax - left;
        if (y < bottom)
            y += bounds.yMin;
        else
            y += cameraBounds.yMax - bottom;
        
        return new Vector3(x, y);
    }

    // Instantiate a random mob in some position in the tileExtent region (outside of camera view)
    private void SpawnMob() {
        activeMobs.Add(Instantiate<Mob>(GetRandomMob(), GetSpawnLocation(), Quaternion.identity, GameObject.FindWithTag("Mob Folder").transform));
    }

    // Return a linearly random mob from those available based on startingTime
    private Mob GetRandomMob() {
        // assume mobs are in ascending order based on their startingTime
        int i;
        for (i = mobs.Length - 1; i > 0; i--) {
            if (mobs[i].startingTime <= Player.Updates)
                break;
        }
        return mobs[Random.Range(0, i + 1)];
    }

    private int GetDesiredMobCount() {
        return Mathf.FloorToInt(Player.Updates / 600f) + 6;
    }
}
