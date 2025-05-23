using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem; // Make sure this is included for the new Input System

public class Menu_and_pause_script : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject pauseMenu;
    public GameObject mainMenu;
    public bool isPaused = false;
    public TextMeshProUGUI highScoreText;
    // public bool DebugTextFields = false; // Added for debug mode
    public Button musicToggleButton; // Assign your music ON/OFF button (should have an Image component)

    // --- New: Sprites for ON and OFF states of the music button ---
    public Sprite musicOnSprite;  // Assign your photoshopped "Music ON" sprite here
    public Sprite musicOffSprite; // Assign your photoshopped "Music OFF" sprite here
    public bool isDebugModeOn = true; // Added for debug mode
    public GameObject DebugTextFields; // Assign your debug text fields here
    public Button debugToggleButton; // Assign your debug ON/OFF button (must have an Image component)
    public Sprite debugOnSprite;   // Assign your "Debug ON" sprite here
    public Sprite debugOffSprite;  // Assign your "Debug OFF" sprite here
    

    void Start()
    {
        // Note: Unity's Start method should be lowercase 's'
        // Ensure pause menu is hidden and main menu is shown at the start of the scene
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (mainMenu != null) mainMenu.SetActive(true);

        // --- Added: Display High Score on Main Menu ---
        DisplayHighScore();
        if (MusicManager.Instance != null)
        {
            // MusicManager.Instance.PlayMusic(MusicManager.Instance.mainMenuCalibrationMusic);
            UpdateMusicToggleButtonSprite(); // Update button sprite on start

        }
        
        // --------------------------------------------
    }

    void Update()
    {
        // UPDATED: Use the new Input System to check for Escape key press
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }
    public void calibrate()
    {
        SceneManager.LoadSceneAsync(2);
    }
    public void QuitGame()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.StopMusic();
        }
        Application.Quit();
        Debug.Log("Quit");
    }
    public void PauseGame()
    {
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        isPaused = true;
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        isPaused = false;
    }
    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(0);
        isPaused = false;
    }
    void DisplayHighScore()
    {
        if (highScoreText != null)
        {
            // Get the high score using the static method from LogicScript
            int highScore = LogicScript.GetHighScore();
            highScoreText.text = "High Score: " + highScore.ToString();
        }
        else
        {
            Debug.LogWarning("High Score Text UI element is not assigned in the MainMenuScript!");
        }
    }
    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs reset");
        DisplayHighScore();
    }
    public void turnOnDebugMode()
    {
        isDebugModeOn = true;
        updateDebugTextFields(isDebugModeOn);
    }
    public void turnOffDebugMode()
    {
        isDebugModeOn = false;
        updateDebugTextFields(isDebugModeOn);
    }
    void updateDebugTextFields(bool isDebugModeOn)
    {
        if (DebugTextFields != null)
        {
            DebugTextFields.SetActive(isDebugModeOn);
            // UpdateDebugToggleButtonSprite(); // Update button sprite after toggling
        }
        else
        {
            Debug.LogWarning("Debug Text Fields GameObject is not assigned!");
        }
    }
    public void ToggleGameMusic()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.ToggleMusic();
            UpdateMusicToggleButtonSprite(); // Update button sprite after toggling
        }
    }
    private void UpdateMusicToggleButtonSprite()
    {
        if (musicToggleButton != null && musicToggleButton.image != null && MusicManager.Instance != null)
        {
            bool isMusicOn = MusicManager.Instance.IsMusicOn();

            if (isMusicOn && musicOnSprite != null)
            {
                musicToggleButton.image.sprite = musicOnSprite;
            }
            else if (!isMusicOn && musicOffSprite != null)
            {
                musicToggleButton.image.sprite = musicOffSprite;
            }
            else
            {
                Debug.LogWarning("Music ON/OFF sprite is not assigned or music state is unexpected.");
            }
        }
        else
        {
            Debug.LogWarning("Music Toggle Button or its Image component is not assigned or found!");
        }
    }
    
}
