using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScrollingBG : MonoBehaviour
{
    // Removed baseSpeed, now directly using GameManager.Instance.gameSpeed
    [SerializeField]
    private Renderer bgRenderer; // Reference to the background renderer

    // --- Added: Multiplier for background scrolling speed ---
    // This allows you to adjust the visual speed of the background relative to the gameSpeed.
    // A smaller value (e.g., 0.1) will make the background scroll slower than the game objects.
    public float backgroundSpeedMultiplier = 0.1f; // Adjust this value in the Inspector
    // --------------------------------------------------------

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure the renderer is assigned, if not, try to get it from this GameObject
        if (bgRenderer == null)
        {
            bgRenderer = GetComponent<Renderer>();
        }

        if (bgRenderer == null)
        {
            Debug.LogError("ScrollingBG: Renderer not found on this GameObject or assigned in Inspector!");
            enabled = false; // Disable the script if no renderer is found
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Define a fallback speed if GameManager.Instance is not available
        // This speed is used directly if GameManager is not found.
        float fallbackSpeed = 0.5f; // You can adjust this default speed

        // Ensure GameManager instance exists before trying to access its gameSpeed
        if (GameManager.Instance != null)
        {
            // The current scrolling speed is now GameManager's gameSpeed scaled by the multiplier
            float currentScrollingSpeed = GameManager.Instance.gameSpeed * backgroundSpeedMultiplier;

            // Update the texture offset based on the current scrolling speed and Time.deltaTime
            bgRenderer.material.mainTextureOffset += new Vector2(0, currentScrollingSpeed * Time.deltaTime);
        }
        else
        {
            // Log a warning if GameManager.Instance is not found
            Debug.LogWarning("ScrollingBG: GameManager.Instance not found. Background scrolling will use a fallback speed.");
            // Fallback to a default speed (scaled by the multiplier for consistency)
            bgRenderer.material.mainTextureOffset += new Vector2(0, fallbackSpeed * Time.deltaTime);
        }
    }
}
