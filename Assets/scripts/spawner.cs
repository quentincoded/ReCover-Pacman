using UnityEngine;

public class Spawner : MonoBehaviour
{
    // public GameObject ghostPrefab;
    public GameObject[] ghostPrefabs; // Assign your different colored ghost prefabs here
    public GameObject coinPrefab;

    public float ghostSpawnInterval = 2f;
    public float coinSpawnInterval = 1f;
    public float width_offset_to_side = 2;

    private float ghostTimer;
    private float coinTimer;

    public Transform spawnPoint;

    void Update()
    {
        ghostTimer += Time.deltaTime;
        coinTimer += Time.deltaTime;

        if (ghostTimer >= ghostSpawnInterval)
        {
            SpawnGhost();
            ghostTimer = 0f;
        }

        if (coinTimer >= coinSpawnInterval)
        {
            Spawn(coinPrefab);
            coinTimer = 0f;
        }
    }

    void Spawn(GameObject prefab)
    {
        float mostleftpoint = transform.position.x - width_offset_to_side;
        float mostrightpoint = transform.position.x + width_offset_to_side;
        float randomX = Random.Range(mostleftpoint, mostrightpoint); // Generate a random x position within the specified range

        // Instantiate(ghost, new Vector3(randomX, transform.position.y, 0), transform.rotation); // Instantiate the ghost prefab
        Instantiate(prefab, spawnPoint.position + new Vector3(randomX, transform.position.y, 0), Quaternion.identity);
    }
    void SpawnGhost()
    {
        // Check if the ghostPrefabs array is assigned and not empty
        if (ghostPrefabs == null || ghostPrefabs.Length == 0)
        {
            Debug.LogError("Ghost Prefabs array is not assigned or is empty in Spawner!");
            return;
        }

        // Randomly select a ghost prefab from the array
        int randomIndex = Random.Range(0, ghostPrefabs.Length); // Random.Range for int is exclusive on the max value
        GameObject ghostToSpawn = ghostPrefabs[randomIndex];

        // Check if the selected prefab is not null
        if (ghostToSpawn == null)
        {
            Debug.LogWarning("Selected ghost prefab is null in Spawner array!");
            return;
        }

        // Calculate random X position
        float mostleftpoint = transform.position.x - width_offset_to_side;
        float mostrightpoint = transform.position.x + width_offset_to_side;
        float randomX = Random.Range(mostleftpoint, mostrightpoint); // Generate a random x position within the specified range

        // Determine the spawn position (using spawnPoint if assigned, otherwise Spawner's position)
        Vector3 spawnPosition = (spawnPoint != null) ? spawnPoint.position : transform.position;
        spawnPosition.x += randomX; // Add the random X offset

        // Instantiate the randomly selected ghost prefab
        Instantiate(ghostToSpawn, spawnPosition, Quaternion.identity);
    }
}

