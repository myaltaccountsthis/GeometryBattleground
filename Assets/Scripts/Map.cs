using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    public BoundsInt Bounds {
        get => bounds;
    }

    [Tooltip("How many units off the camera bounds should tiles still generate")]
    public int tileExtent;
    [Tooltip ("List of all mobs to use")]
    public Mob[] mobs;
    [Tooltip("The maximum number of experience drops that can exist in the tile extent range")]
    public int maxExpDrops;
    [Tooltip("In updates")]
    public int minExpDelay;
    [Tooltip("In updates")]
    public int maxExpDelay;
    public Transform experienceFolder;
    public Transform mobFolder;

    public int CurrentMobCount {
        get => mobFolder.childCount;
    }

    [Tooltip("The tile to use for the ground")]
    [SerializeField]
    private TileBase groundTile;
    [Tooltip("Minimum experience value for each new experience orb sprite")]
    [SerializeField]
    private int[] experienceOrbStages;
    [Tooltip("Sprites to use for each experience orb stage")]
    [SerializeField]
    private Sprite[] experienceOrbSprites;
    [Tooltip("Random values to use for map spawned experience orbs")]
    [SerializeField]
    private int[] randomExperienceValues;
    [Tooltip("Experience component of experience orb prefab")]
    [SerializeField]
    private Experience experienceOrb;
    [SerializeField]
    private Tilemap background;

    private Tilemap tileMap;
    private List<Mob> activeMobs;
    private BoundsInt bounds;
    private BoundsInt mobBounds;
    private BoundsInt experienceBounds;
    private int nextExpSpawn;

    void Start()
    {
        tileMap = GetComponent<Tilemap>();
        activeMobs = new List<Mob>();
        bounds = tileMap.cellBounds;
        mobBounds = background.cellBounds;
        mobBounds.SetMinMax(new Vector3Int(mobBounds.xMin + 1, mobBounds.yMin + 1), new Vector3Int(mobBounds.xMax - 1, mobBounds.yMax - 1));
        experienceBounds = bounds;
        experienceBounds.SetMinMax(new Vector3Int(experienceBounds.xMin + 1, experienceBounds.yMin + 1), new Vector3Int(experienceBounds.xMax - 1, experienceBounds.yMax - 1));
        nextExpSpawn = 0;
        Debug.Assert(minExpDelay <= maxExpDelay, "Error: Max Experience Delay cannot be less than Min Experience Delay");
        for (int i = 0; i < maxExpDrops / 2; i++) {
            AttemptExpSpawn();
        }
    }

    void Update()
    {
        if (GameTime.isPaused)
            return;

        /*
        UpdateBounds();

        // generate tiles in view of the camera + tileExtent
        TileBase[] tiles = tileMap.GetTilesBlock(bounds);
        for (int x = 0; x < bounds.size.x; x++) {
            for (int y = 0; y < bounds.size.y; y++) {
                tileMap.SetTile(new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0), groundTile);
            }
        }
        */

        // clear dead mobs
        for (int i = 0; i < activeMobs.Count; i++) {
            if (activeMobs[i] == null) {
                activeMobs.RemoveAt(i);
                i--;
            }
        }
        
        /*
        // do mob spawning
        int desiredCount = GetDesiredMobCount();
        for (int i = activeMobs.Count; i < desiredCount; i++) {
            SpawnMob();
        }
        */

        // check exp drops
        if (nextExpSpawn == 0) {
            AttemptExpSpawn();
        }
        else if (nextExpSpawn > 0)
            nextExpSpawn--;
    }

    public Experience InstantiateExperience(int experience, Vector2 position) {
        Experience exp = Instantiate<Experience>(experienceOrb, position, Quaternion.identity, experienceFolder);
        exp.SetExperience(experience);
        SetExperienceSprite(exp);
        return exp;
    }

    private void SetExperienceSprite(Experience experience) {
        int index;
        for (index = experienceOrbStages.Length - 1; experienceOrbStages[index] > experience.Value; index--);
        experience.GetComponent<SpriteRenderer>().sprite = experienceOrbSprites[index];
    }

    private void UpdateBounds() {
        float xExtent = Camera.main.orthographicSize * Screen.width / Screen.height + tileExtent;
        float yExtent = Camera.main.orthographicSize + tileExtent;
        Vector3Int combinedExtent = new Vector3Int((int) xExtent, (int) yExtent, 0);
        bounds = new BoundsInt(new Vector3Int((int) Camera.main.transform.position.x, (int) Camera.main.transform.position.y) - combinedExtent, 2 * combinedExtent);
    }

    public void SpawnWave(int wave) {
        int count = Mathf.FloorToInt(Mathf.Pow(wave, 1.3f)) + 6;
        for (int i = 0; i < count; i++) {
            SpawnMob(wave);
        }
    }

    // Returns a random location from the tileExtent perimeter
    private Vector3 GetMobSpawnLocation() {
        if (Random.Range(0, 2) == 0) {
            return new Vector3(Random.Range((float) mobBounds.xMin, mobBounds.xMax), Random.Range(0, 2) * mobBounds.size.y + mobBounds.yMin);
        }
        return new Vector3(Random.Range(0, 2) * mobBounds.size.x + mobBounds.xMin, Random.Range((float) mobBounds.yMin, mobBounds.yMax));
        /*
        float xExtent = Camera.main.orthographicSize * Screen.width / Screen.height;
        float yExtent = Camera.main.orthographicSize;
        Vector3Int cameraExtent = new Vector3Int((int) xExtent, (int) yExtent, 0);
        BoundsInt cameraBounds = new BoundsInt(new Vector3Int((int) Camera.main.transform.position.x, (int) Camera.main.transform.position.y) - cameraExtent, 2 * cameraExtent);
        int left = cameraBounds.xMin - bounds.xMin, right = bounds.xMax - cameraBounds.xMax;
        int bottom = cameraBounds.yMin - bounds.yMin, top = bounds.yMax - cameraBounds.yMax;
        // in order to get spawns along the perimeter
        bool randomY = Random.value > .5f;
        int x = Random.Range(0, left + right), y = Random.Range(0, bottom + top);
        if (randomY) {
            x = Random.value > .5f ? 0 : left + right;
        }
        else {
            y = Random.value > .5f ? 0 : bottom + top;
        }
        // shift the points into the extent range depending on which half it was on
        if (x < left)
            x += bounds.xMin;
        else
            x += cameraBounds.xMax - left;
        if (y < bottom)
            y += bounds.yMin;
        else
            y += cameraBounds.yMax - bottom;
        
        return new Vector3(x, y);
        */
    }

    // Instantiate a random mob in some position in the tileExtent region (outside of camera view)
    private void SpawnMob(int wave) {
        activeMobs.Add(Instantiate<Mob>(GetRandomMob(wave), GetMobSpawnLocation(), Quaternion.identity, mobFolder));
    }

    // Return a random mob with weight
    private Mob GetRandomMob(int wave) {
        // assume mobs are in ascending order based on their startingTime
        int i;
        for (i = mobs.Length - 1; i > 0; i--) {
            // change to wave
            if (mobs[i].startingWave <= wave)
                break;
        }
        int length = i + 1;
        int totalWeight = 0;
        for (i = 0; i < length; i++) {
            totalWeight += mobs[i].spawnWeight;
        }
        int weight = Random.Range(0, totalWeight);
        for (i = 0; i < length; i++) {
            int mobWeight = mobs[i].spawnWeight;
            if (weight < mobWeight) {
                return mobs[i];
            }
            weight -= mobWeight;
        }
        return mobs[0];
    }

    private Vector3 GetExperienceSpawnLocation() {
        return new Vector3(Random.Range((float) experienceBounds.xMin, experienceBounds.xMax), Random.Range((float) experienceBounds.yMin, experienceBounds.yMax));
    }

    private void GenerateNextExpSpawn() {
        InstantiateExperience(randomExperienceValues[Random.Range(0, randomExperienceValues.Length)], GetExperienceSpawnLocation());
        nextExpSpawn = Random.Range(minExpDelay, maxExpDelay);
    }

    // Returns the number of exp drops that are in the tile extent range
    private int CountExpDrops() {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Experience Orb");
        int num = 0;
        foreach (GameObject gameObject in gameObjects) {
            Vector3 position = gameObject.transform.position;
            if (position.x > bounds.xMin && position.x < bounds.xMax && position.y > bounds.yMin && position.y < bounds.yMax) {
                num++;
            }
        }
        return num;
    }

    private void AttemptExpSpawn() {
        if (CountExpDrops() < maxExpDrops)
            GenerateNextExpSpawn();
    }
}
