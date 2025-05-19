using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Menu_and_pause_script : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject pauseMenu;
    public GameObject mainMenu;
    public bool isPaused = false;
    public TextMeshProUGUI highScoreText;

    void start()
    {
        // Note: Unity's Start method should be lowercase 's'
        // Ensure pause menu is hidden and main menu is shown at the start of the scene
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (mainMenu != null) mainMenu.SetActive(true);

        // --- Added: Display High Score on Main Menu ---
        DisplayHighScore();
        // --------------------------------------------
    }

    void update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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
}
