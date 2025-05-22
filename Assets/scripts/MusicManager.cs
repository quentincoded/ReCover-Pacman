using UnityEngine;
using UnityEngine.SceneManagement; // Required for SceneManager to detect scene changes
using System.Collections; // Required for Coroutines

public class MusicManager : MonoBehaviour
{
    // Singleton instance
    public static MusicManager Instance;

    // Public AudioClips to assign in the Inspector
    public AudioClip mainMenuCalibrationMusic;
    public AudioClip gameMusic;

    // Reference to the AudioSource component that will play the music
    private AudioSource audioSource;

    // State variable for music on/off, saved to PlayerPrefs
    private const string MusicOnKey = "IsMusicOn";
    private bool isMusicOn = true; // Default to music being on

    void Awake()
    {
        // Implement the Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make this GameObject persist across scenes
            Debug.Log("MusicManager: Awake - Instance created and set to DontDestroyOnLoad.");

            // Get or add the AudioSource component
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.loop = true; // Music should loop by default
            audioSource.playOnAwake = false; // We'll control playback via script

            // Load music preference from PlayerPrefs
            isMusicOn = PlayerPrefs.GetInt(MusicOnKey, 1) == 1; // 1 for true, 0 for false

            // Subscribe to scene loaded event to change music
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (Instance != this)
        {
            // If a duplicate instance exists, destroy this one
            Debug.LogWarning("MusicManager: Awake - Duplicate instance found, destroying this one.");
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from the scene loaded event to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("MusicManager: OnDestroy - Unsubscribed from SceneManager.sceneLoaded.");
    }

    // This method is called automatically when a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"MusicManager: OnSceneLoaded - Scene loaded: {scene.name}. Current Music On state: {isMusicOn}.");

        // Determine which music to play based on the loaded scene
        if (scene.buildIndex == 0 || scene.buildIndex == 2) // Main Menu (0) or Calibration (2)
        {
            Debug.Log("MusicManager: Detected Main Menu or Calibration scene. Attempting to play Main Menu/Calibration music.");
            StartCoroutine(PlayMusicTransition(mainMenuCalibrationMusic)); // Start coroutine for music transition
        }
        else if (scene.buildIndex == 1) // Main Game (1)
        {
            Debug.Log("MusicManager: Detected Main Game scene. Attempting to play Game music.");
            StartCoroutine(PlayMusicTransition(gameMusic)); // Start coroutine for music transition
        }
        else
        {
            // For other scenes, stop music or handle as needed
            Debug.Log("MusicManager: Detected unknown scene. Stopping music.");
            StopMusic(); // Direct stop, as no new music is intended
        }
    }

    /// <summary>
    /// Coroutine to handle music playback with a small delay for cleaner transitions.
    /// </summary>
    /// <param name="clipToPlay">The AudioClip to play.</param>
    private IEnumerator PlayMusicTransition(AudioClip clipToPlay)
    {
        if (audioSource == null)
        {
            Debug.LogError("MusicManager: AudioSource is null. Cannot play music.");
            yield break; // Exit coroutine
        }

        // If music is globally off, ensure it's stopped and don't proceed to play
        if (!isMusicOn)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("MusicManager: Music is OFF, stopping current playback.");
            }
            yield break; // Exit coroutine
        }

        // If the new clip is different from the current one, or if nothing is playing
        if (audioSource.clip != clipToPlay || !audioSource.isPlaying)
        {
            // Always stop current music before setting a new clip to ensure a clean start
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log($"MusicManager: Explicitly stopping current music: {audioSource.clip?.name ?? "None"}");
            }

            // Explicitly clear the clip reference to ensure no lingering audio from the previous clip
            audioSource.clip = null;

            // <<< ADDED: Small delay after stopping and clearing the clip >>>
            yield return null; // Wait for one frame to ensure audio system processes the stop
            // yield return new WaitForSeconds(0.01f); // Alternative: wait for a fixed small time (e.g., 10ms)

            audioSource.clip = clipToPlay; // Set the new clip

            if (audioSource.clip != null) // Only play if a valid clip is assigned
            {
                audioSource.Play();
                Debug.Log($"MusicManager: Playing new music: {clipToPlay.name}");
            }
            else
            {
                Debug.Log("MusicManager: No valid AudioClip provided to PlayMusicTransition, stopping.");
                audioSource.Stop();
            }
        }
        else
        {
            // If the same clip is already playing, do nothing
            Debug.Log($"MusicManager: Already playing {clipToPlay.name}. No change needed.");
        }
    }

    /// <summary>
    /// Public method to request music playback. It starts a coroutine for the transition.
    /// </summary>
    /// <param name="clipToPlay">The AudioClip to play.</param>
    public void PlayMusic(AudioClip clipToPlay)
    {
        StartCoroutine(PlayMusicTransition(clipToPlay));
    }


    /// <summary>
    /// Stops the currently playing music.
    /// </summary>
    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("MusicManager: Music stopped.");
        }
    }

    /// <summary>
    /// Toggles the music on or off and saves the preference.
    /// </summary>
    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn; // Invert the state
        PlayerPrefs.SetInt(MusicOnKey, isMusicOn ? 1 : 0); // Save preference
        PlayerPrefs.Save(); // Ensure it's written to disk

        if (isMusicOn)
        {
            // If turning music on, play the current scene's music
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.buildIndex == 0 || currentScene.buildIndex == 2) // Main Menu or Calibration
            {
                PlayMusic(mainMenuCalibrationMusic); // Calls the public PlayMusic, which starts the coroutine
            }
            else if (currentScene.buildIndex == 1) // Main Game
            {
                PlayMusic(gameMusic); // Calls the public PlayMusic, which starts the coroutine
            }
        }
        else
        {
            StopMusic(); // If turning music off, stop it
        }
        Debug.Log($"MusicManager: Music is now {(isMusicOn ? "ON" : "OFF")}.");
    }

    /// <summary>
    /// Returns true if music is currently enabled, false otherwise.
    /// </summary>
    public bool IsMusicOn()
    {
        return isMusicOn;
    }
}