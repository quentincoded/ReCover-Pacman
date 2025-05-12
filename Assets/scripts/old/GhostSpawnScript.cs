using UnityEngine;

public class GhostSpawnScript : MonoBehaviour
{
    public GameObject ghost; // Reference to the ghost prefab
    public float spawnInterval = 1f; // Time interval between spawns
    private float timer = 0; // Time when the next ghost will be spawned

    public float width_offset_to_side=10;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnGost();
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < spawnInterval){
            timer += Time.deltaTime; // Increment the timer by the time since the last frame
        }
        else
        {
            spawnGost(); 
            timer = 0; // Reset the timer
        }
        
    }

    void spawnGost()
    {
        float mostleftpoint = transform.position.x - width_offset_to_side;
        float mostrightpoint = transform.position.x + width_offset_to_side;
        float randomX = Random.Range(mostleftpoint, mostrightpoint); // Generate a random x position within the specified range
        Instantiate(ghost, new Vector3(randomX, transform.position.y, 0), transform.rotation); // Instantiate the ghost prefab

        
        
            
    }
}
