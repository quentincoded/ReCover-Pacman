using UnityEngine;
using System.Collections.Generic;
using System.Collections;
// using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro; // ← required for TextMeshPro
using UnityEngine.SceneManagement; // Required for SceneManager

public class LogicScript : MonoBehaviour
{
    public int playerScore = 0; // Player's score
    public int playerLives = 3; // Player's lives
    public TextMeshProUGUI scoreText; // ← use TMP type here
    public TextMeshProUGUI livesText; // <<< ADDED: Assign your Lives Text UI element in Inspector
    public GameObject gameOverPanel; // <<< ADDED: Assign your Game Over UI Panel in Inspector
    // --- Added for Sound ---
    public AudioClip coinSound; // Assign your coin sound clip in the Inspector
    public AudioClip gameOverSound; // Assign your game over sound clip in the Inspector
    private AudioSource audioSource; // Reference to the AudioSource component
    // -----------------------


    // [ContextMenu("Add Score")]

    void Start()
    {
        // Initialize UI display
        UpdateScoreText();
        UpdateLivesText(); 

        // Ensure Game Over panel is hidden at start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        // Ensure game runs at normal speed at start
        Time.timeScale = 1f;
        // Attempt to find AudioSource if not assigned in Inspector
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    public void addScore(int scoreToAdd)
    {
        // Don't add score if game is over
        if (playerLives <= 0) return;
        playerScore += scoreToAdd; // Add the score to the player's score
        UpdateScoreText(); // Update the score text on the screen
        if (scoreToAdd==1){
            PlayCoinSound(); // --- Added: Play coin sound when score is added
        }
    }
    public void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + playerScore.ToString();
        } //scoreText.text = "Score: " + playerScore.ToString(); // Update the score text
    }
    public void LoseLife()
    {
        // Don't lose life if game is already over
        if (playerLives <= 0) return;

        playerLives--; // Decrement lives
        //could also play losing life/ hurt sound here but its in the Pacman script
        UpdateLivesText(); // Update UI

        if (playerLives <= 0)
        {
            GameOver(); // Trigger Game Over if lives run out
        }
        // Optional: Add visual/audio feedback for losing a life here
        Debug.Log("Life Lost! Lives remaining: " + playerLives);
    }
    public void UpdateLivesText()
    {
        if (livesText != null)
        {
            // You can format this however you like (e.g., use icons)
            livesText.text = "Lives: " + playerLives.ToString();
        }
    }
    public void GameOver()
    {
        Debug.Log("Game Over!");
        // Show Game Over screen
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        // Stop the game (pause time is a simple way)
        Time.timeScale = 0f; // Pauses FixedUpdate, Update calls tied to physics time.
        // NOTE: Input reading in Update might still occur, handle in PacmanScript if needed.
        PlayGameOverSound(); 
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Restart the game by reloading the current scene
    }
    
    void PlayCoinSound()
    {
        if (audioSource != null && coinSound != null)
        {
            audioSource.PlayOneShot(coinSound); // Play the coin sound once
        }
    }

    void PlayGameOverSound()
    {
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound); // Play the game over sound once
        }
    }
}
