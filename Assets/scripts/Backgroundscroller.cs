using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    // public float scrollSpeed = 1f;
    public float tileHeight = 10f; // Adjust based on your background sprite height

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);
        transform.Translate(Vector3.down * GameManager.Instance.gameSpeed * Time.deltaTime);

        // Loop logic: if the background has moved down by a full tile, reset position
        if (transform.position.y <= startPosition.y - tileHeight)
        {
            transform.position = startPosition;
        }
    }
}