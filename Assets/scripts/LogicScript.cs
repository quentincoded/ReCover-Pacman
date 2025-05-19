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
    public GameObject PauseButton; // <<< ADDED: Assign your Pause Button in Inspector
    
    public GameObject[] heartSprites; // The size of this array should match the maximum number of lives  you want to display
    // --- Added for Sound ---
    public AudioClip coinSound; // Assign your coin sound clip in the Inspector
    public AudioClip gameOverSound; // Assign your game over sound clip in the Inspector
    private AudioSource audioSource; // Reference to the AudioSource component
    private const string HighScoreKey = "HighScore";
    // -----------------------


    // [ContextMenu("Add Score")]

    void Start()
    {
        // Initialize UI display
        UpdateScoreText();
        UpdateLivesText(); 
        UpdateLivesVisual();
        PauseButton.SetActive(true); // Hide Pause button at start

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
        UpdateLivesVisual(); // Update visual representation of lives

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
            livesText.text = playerLives.ToString();
        }
    }
    public void UpdateLivesVisual()
    {
        // Ensure the heartSprites array is assigned and has elements
        if (heartSprites == null || heartSprites.Length == 0)
        {
            Debug.LogWarning("Heart Sprites array is not assigned or is empty in LogicScript!");
            return;
        }

        // Iterate through the heart sprites and enable/disable them
        for (int i = 0; i < heartSprites.Length; i++)
        {
            // If the current index is less than the number of lives, the heart should be active
            if (heartSprites[i] != null) // Add null check for safety
            {
                heartSprites[i].SetActive(i < playerLives);
            }
        }
    }
    public void GameOver()
    {
        Debug.Log("Game Over!");
        // Show Game Over screen
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            PauseButton.SetActive(false); // Hide Pause button when game is over
            livesText.gameObject.SetActive(false); // Hide lives text when game is over

        }
        // Stop the game (pause time is a simple way)
        Time.timeScale = 0f; // Pauses FixedUpdate, Update calls tied to physics time.
        // NOTE: Input reading in Update might still occur, handle in PacmanScript if needed.
        PlayGameOverSound();
        CheckAndSaveHighScore(playerScore);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Restart the game by reloading the current scene
    }
    // --- Added: High Score Methods ---
    // Check if the current score is a new high score and save it
    private void CheckAndSaveHighScore(int currentScore)
    {
        Debug.Log($"Started Highscore check: Current Score: {currentScore}");
        int currentHighScore = GetHighScore(); // Get the existing high score
        Debug.Log($"Current High Score: {currentHighScore}");
        if (currentScore > currentHighScore)
        {

            PlayerPrefs.SetInt(HighScoreKey, currentScore); // Save the new high score
            PlayerPrefs.Save(); // Ensure it's written to disk
            Debug.Log($"New High Score: {currentScore}");
        }
    }

    // Get the current high score from PlayerPrefs
    public static int GetHighScore()
    {
        // Returns 0 if the HighScoreKey doesn't exist in PlayerPrefs
        return PlayerPrefs.GetInt(HighScoreKey, 0);
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
