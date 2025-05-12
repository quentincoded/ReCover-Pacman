// using UnityEngine;

// public class BackgroundSpawner : MonoBehaviour
// {
//     public GameObject[] backgroundPrefabs;
//     public GameObject[] borderPrefabs;
//     public float baseSpawnInterval = 10f; // base spacing in world units
//     private float timer = 0f;
//     public float spawnPosY = 20f; // Fixed Y position for spawning

//     private float nextSpawnY = 0;

//     void Start()
//     {
//         SpawnTile(0);
//         SpawnTile(10);
//         SpawnTile(17);
//     }

//     void Update()
// {
//     if (GameManager.Instance == null) return;

//     float currentGameSpeed = GameManager.Instance.gameSpeed;
//     float timeToNextTile = baseSpawnInterval / currentGameSpeed;
//     timer += Time.deltaTime;
//     if (timer >= timeToNextTile)
//     {
//         SpawnTile(spawnPosY);
        
//         timer = 0f;
//         // Debug.Log("Spawned tile at Y: " + nextSpawnY); // Optional debug log to check spawn position
//     }
// }

//     void SpawnTile(float spawnY)
// {
//     float fixedY = spawnY; // Always spawn here

//     foreach (var prefab in backgroundPrefabs)
//     {
//         Instantiate(prefab, new Vector3(0, fixedY, 0), Quaternion.identity, GameObject.Find("Layer_MainBackground").transform);
//     }

//     foreach (var prefab in borderPrefabs)
//     {
//         Instantiate(prefab, new Vector3(0, fixedY, 0), Quaternion.identity, GameObject.Find("Layer_Border").transform);
//     }
// }

// }

using UnityEngine;

public class BackgroundSpawner : MonoBehaviour
{
    public GameObject[] backgroundPrefabs;
    public GameObject[] borderPrefabs;
    // public float baseSpawnInterval = 10f; // This will be replaced by tileHeight for positioning logic
    
    // ADD THIS: Set this in the Inspector. It's crucial for seamless tiling.
    // This should be the actual height of your background/border tile prefabs in world units.
    public float tileHeight = 10f; 

    public float initialSpawnYOffset = 17f; // The Y pivot of the topmost initially placed tile by Start()

    private float timer = 0f;
    private float nextSpawnPivotY; // Tracks the Y pivot position for the next tile to be spawned

    void Start()
    {
        // These are your initial, manually placed tiles.
        SpawnTile(0);
        SpawnTile(10);
        SpawnTile(17); // This was the Y-value passed to the last SpawnTile in your original Start.

        // Determine the pivot Y for the first dynamically spawned tile.
        // This assumes 'initialSpawnYOffset' (e.g., 17f) was the pivot Y of the topmost tile
        // placed by the Start() method calls above.
        // The next tile should be placed 'tileHeight' units above it, pivot to pivot.
        if (tileHeight <= 0)
        {
            Debug.LogError("tileHeight in BackgroundSpawner is not set to a positive value. Background tiling will be incorrect.");
            // Default to a fallback if not set, though it should be configured correctly.
            tileHeight = 10f; 
        }
        nextSpawnPivotY = initialSpawnYOffset + tileHeight;
        
        timer = 0f; // Ensure timer is reset for dynamic spawning logic in Update.
    }

    void Update()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance is null in BackgroundSpawner.Update");
            return;
        }

        float currentGameSpeed = GameManager.Instance.gameSpeed;
        if (currentGameSpeed <= 0) return; // Avoid division by zero if gameSpeed can be zero or negative

        // Calculate how much time should pass to scroll one tileHeight distance
        float timeToScrollOneTileHeight = tileHeight / currentGameSpeed;

        timer += Time.deltaTime;

        // If enough time has passed (meaning the view has scrolled by one tile height)
        if (timer >= timeToScrollOneTileHeight)
        {
            SpawnTile(nextSpawnPivotY);
            nextSpawnPivotY += tileHeight; // Update the Y position for the next tile
            
            // Reset the timer, subtracting any excess time to maintain accuracy
            timer -= timeToScrollOneTileHeight; 
        }
    }

    void SpawnTile(float spawnY)
    {
        // This 'spawnY' is the Y position for the pivot of the new tiles.
        // Ensure your background and border prefabs have consistent pivot points
        // (e.g., center or bottom edge) for 'tileHeight' to work correctly.
        // If pivot is center, new_center_Y = old_center_Y + tileHeight.
        // If pivot is bottom, new_bottom_Y = old_bottom_Y + tileHeight.
        // The math works out the same for advancing the spawn position.
        Vector3 spawnPosition = new Vector3(0, spawnY, 0);

        if (backgroundPrefabs != null)
        {
            foreach (var prefab in backgroundPrefabs)
            {
                if (prefab != null)
                {
                    Instantiate(prefab, spawnPosition, Quaternion.identity, GameObject.Find("Layer_MainBackground")?.transform);
                }
            }
        }

        if (borderPrefabs != null)
        {
            foreach (var prefab in borderPrefabs)
            {
                if (prefab != null)
                {
                    Instantiate(prefab, spawnPosition, Quaternion.identity, GameObject.Find("Layer_Border")?.transform);
                }
            }
        }
    }
}