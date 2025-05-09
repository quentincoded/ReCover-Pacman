using UnityEngine;

public class AutoDestroyOffscreen : MonoBehaviour
{
    private Camera cam;
    private float buffer = 2f;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (transform.position.y + GetComponent<SpriteRenderer>().bounds.size.y / 2f < cam.transform.position.y - cam.orthographicSize - buffer)
        {
            Destroy(gameObject);
        }
    }
}

