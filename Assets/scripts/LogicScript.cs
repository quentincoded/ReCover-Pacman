using UnityEngine;
using System.Collections.Generic;
using System.Collections;
// using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro; // ← required for TextMeshPro

public class LogicScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int playerScore = 0; // Player's score
    public TextMeshProUGUI scoreText; // ← use TMP type here
    // public int scoreToAdd = 10; // Score to add when the player collects an item

    // [ContextMenu("Add Score")]

    public void addScore(int scoreToAdd)
    {
        playerScore += scoreToAdd; // Add the score to the player's score
        // playerScore += 1;
        UpdateScoreText(); // Update the score text on the screen
    }
    public void UpdateScoreText()
    {
        scoreText.text = "Score: " + playerScore.ToString(); // Update the score text
    }
    
}
