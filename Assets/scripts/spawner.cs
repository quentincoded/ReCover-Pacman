using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject ghostPrefab;
    public GameObject coinPrefab;

    public float ghostSpawnInterval = 2f;
    public float coinSpawnInterval = 1f;
    public float width_offset_to_side=2;

    private float ghostTimer;
    private float coinTimer;

    public Transform spawnPoint;

    void Update()
    {
        ghostTimer += Time.deltaTime;
        coinTimer += Time.deltaTime;

        if (ghostTimer >= ghostSpawnInterval)
        {
            Spawn(ghostPrefab);
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
}

