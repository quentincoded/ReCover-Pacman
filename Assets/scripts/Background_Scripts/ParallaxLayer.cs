using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public float parallaxFactor = 1.0f;

    void Update()
    {
        float speed = GameManager.Instance.gameSpeed * parallaxFactor;
        transform.Translate(Vector3.down * speed * Time.deltaTime);
    }
}

