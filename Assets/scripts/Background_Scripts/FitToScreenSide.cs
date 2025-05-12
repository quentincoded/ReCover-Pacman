using UnityEngine;

public class FitToScreenSide : MonoBehaviour
{
    public enum Side { Left, Right }
    public Side borderSide;

    void Start()
    {
        Camera cam = Camera.main;
        float z = transform.position.z;
        Vector3 edge = cam.ViewportToWorldPoint(new Vector3(borderSide == Side.Left ? 0 : 1, 0.5f, Mathf.Abs(cam.transform.position.z - z)));
        transform.position = new Vector3(edge.x, transform.position.y, z);
    }
}

// using UnityEngine;

// public class FitToScreenSide : MonoBehaviour
// {
//     public enum Side { Left, Right }
//     public Side borderSide;

//     [Range(0f, 1f)]
//     public float visiblePercent = 0.25f; // 0.25 = only 25% of the object visible

//     void Start()
//     {
//         Camera cam = Camera.main;
//         float z = transform.position.z;

//         Vector3 edge = cam.ViewportToWorldPoint(new Vector3(borderSide == Side.Left ? 0 : 1, 0.5f, Mathf.Abs(cam.transform.position.z - z)));

//         float halfWidth = GetComponent<SpriteRenderer>().bounds.size.x / 2f;
//         float offset = halfWidth * (1f - visiblePercent);

//         float newX = borderSide == Side.Left ? edge.x + offset : edge.x - offset;

//         transform.position = new Vector3(newX, transform.position.y, z);
//     }
// }