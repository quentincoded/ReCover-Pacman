using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class coin_logic_script : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public LogicScript logic;

    void Start()
    {
        // logic = GameObject.FindGameObjectwithTag("Logic").GetComponent<LogicScript>();
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger entered with: " + collision.gameObject.name);

        if (collision.gameObject.layer == 3)
        {
            Destroy(gameObject); // Destroy the coin object
            // Destroy(gameObject); // Destroy the coin
            Debug.Log("Coin collected!"); // Log a message to the console
            logic.addScore(1); // Call the addScore method from LogicScript

        }
        
    }
}
