using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Global pause controller. Auto-instantiates itself from Resources/PauseManager.prefab
/// before any scene loads, so it exists in every level without manual setup.
/// Handles the Escape key, freezing time, and showing/hiding the pause menu UI.
/// Also shows the end screen once a level is completed.
/// </summary>
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    public static event Action<bool> OnPauseChanged;

    public bool IsPaused { get; private set; }
    public bool IsEndScreenShown { get; private set; }

    [Header("Assign the Pause Menu Canvas (child of this prefab)")]
    [SerializeField] private GameObject pauseMenuRoot;

    [Header("Assign the End Screen Canvas (child of this prefab)")]
    [SerializeField] private GameObject endScreenRoot;
    [SerializeField] private EndScreenUI endScreen;

    [Header("Options")]
    [SerializeField] private bool pauseAudio = true;

    // Runs automatically before the first scene loads - no manual placement needed.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;

        GameObject prefab = Resources.Load<GameObject>("PauseManager");
        if (prefab == null)
        {
            Debug.LogError("PauseManager prefab not found in a Resources folder. " +
                            "Create it and place it at Assets/Resources/PauseManager.prefab");
            return;
        }

        Instantiate(prefab);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);
        if (endScreenRoot != null)
            endScreenRoot.SetActive(false);
    }

    private void OnEnable()
    {
        WorldService.OnLevelComplete += ShowEndScreen;
    }

    private void OnDisable()
    {
        WorldService.OnLevelComplete -= ShowEndScreen;
    }

    private void Update()
    {
        // the level is over, so there's nothing to pause
        if (IsEndScreenShown) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        if (pauseAudio) AudioListener.pause = true;

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(true);

        OnPauseChanged?.Invoke(true);
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (pauseAudio) AudioListener.pause = false;

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        OnPauseChanged?.Invoke(false);
    }

    public void ShowEndScreen(float score)
    {
        if (IsPaused) Resume();
        IsEndScreenShown = true;

        StreakTracker streak = FindAnyObjectByType<StreakTracker>();
        if (endScreen != null) endScreen.SetResults(streak != null ? streak.CurrentRank : 0, score);
        if (endScreenRoot != null)
            endScreenRoot.SetActive(true);
    }

    private void HideEndScreen()
    {
        IsEndScreenShown = false;
        if (endScreenRoot != null)
            endScreenRoot.SetActive(false);
    }

    // --- Convenience methods for buttons / other scripts ---

    public void RestartLevel()
    {
        Resume(); // un-freeze time before reloading, or the new scene loads paused
        HideEndScreen();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(string sceneName)
    {
        Resume();
        HideEndScreen();
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
