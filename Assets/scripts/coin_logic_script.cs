using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class coin_logic_script : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public LogicScript logic;
    // PacmanScript pacman = collision.gameObject.GetComponent<PacmanScript>();

    void Start()
    {
        // logic = GameObject.FindGameObjectwithTag("Logic").GetComponent<LogicScript>();
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // private void OnTriggerEnter2D(Collider2D collision)
    // {
    //     Debug.Log("Trigger entered with: " + collision.gameObject.name);

    //     if (collision.gameObject.layer == 3 )
    //     {
    //         Destroy(gameObject); // Destroy the coin object
    //         // Destroy(gameObject); // Destroy the coin
    //         Debug.Log("Coin collected!"); // Log a message to the console
    //         logic.addScore(1); // Call the addScore method from LogicScript

    //     }

    // }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Debug.Log("Trigger entered with: " + collision.gameObject.name); // Keep for debugging if needed

        // Check if the colliding object is Pacman (assuming Layer 3 is Pacman's layer)
        if (collision.gameObject.layer == 3)
        {   
            if (collision.gameObject.CompareTag("Ghost")) // Check if it's a Ghost by tag
            {
            Debug.Log("Coin collided with a Ghost. Destroying coin: " + gameObject.name);
            Destroy(gameObject); // Destroy the coin if it touches a ghost
            }
            // --- Added: Get the PacmanScript component from the colliding object ---
            PacmanScript pacman = collision.gameObject.GetComponent<PacmanScript>();
            // ---------------------------------------------------------------------

            // Check if we successfully got the PacmanScript and if Pacman's mouth is open
            if (pacman != null && pacman.IsMouthOpen)
            {
                Destroy(gameObject); // Destroy the coin object
                // Removed: Destroy(gameObject); // Duplicate line
                Debug.Log("Coin collected!"); // Log a message to the console

                // Call the addScore method from LogicScript (which now plays the coin sound)
                logic.addScore(1);
            }
            // If Pacman's mouth is not open, the coin is not collected and remains
        }
        // else if (collision.gameObject.CompareTag("Ghost")) // Check if it's a Ghost by tag
        // {
        //     Debug.Log("Coin collided with a Ghost. Destroying coin: " + gameObject.name);
        //     Destroy(gameObject); // Destroy the coin if it touches a ghost
        // }
    }
}
