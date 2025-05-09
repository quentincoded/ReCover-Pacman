using UnityEngine;

public class BackgroundSpawner : MonoBehaviour
{
    public GameObject[] backgroundPrefabs;
    public GameObject[] borderPrefabs;
    public float baseSpawnInterval = 10f; // base spacing in world units
    private float timer = 0f;
    public float spawnPosY = 20f; // Fixed Y position for spawning

    private float nextSpawnY = 0;

    void Start()
    {
        SpawnTile(0);
        SpawnTile(10);
        SpawnTile(17);
    }

    void Update()
{
    if (GameManager.Instance == null) return;

    float currentGameSpeed = GameManager.Instance.gameSpeed;
    float timeToNextTile = baseSpawnInterval / currentGameSpeed;
    timer += Time.deltaTime;
    if (timer >= timeToNextTile)
    {
        SpawnTile(spawnPosY);
        
        timer = 0f;
        // Debug.Log("Spawned tile at Y: " + nextSpawnY); // Optional debug log to check spawn position
    }
}

    void SpawnTile(float spawnY)
{
    float fixedY = spawnY; // Always spawn here

    foreach (var prefab in backgroundPrefabs)
    {
        Instantiate(prefab, new Vector3(0, fixedY, 0), Quaternion.identity, GameObject.Find("Layer_MainBackground").transform);
    }

    foreach (var prefab in borderPrefabs)
    {
        Instantiate(prefab, new Vector3(0, fixedY, 0), Quaternion.identity, GameObject.Find("Layer_Border").transform);
    }
}

}
