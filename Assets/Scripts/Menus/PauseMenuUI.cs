using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Optional: auto-wire buttons instead of using OnClick() in Inspector")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void OnEnable()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    private void OnDisable()
    {
        if (resumeButton != null) resumeButton.onClick.RemoveListener(ResumeGame);
        if (restartButton != null) restartButton.onClick.RemoveListener(RestartLevel);
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(GoToMainMenu);
    }

    public void ResumeGame() => PauseManager.Instance.Resume();
    public void RestartLevel() => PauseManager.Instance.RestartLevel();
    public void GoToMainMenu() => PauseManager.Instance.LoadScene(mainMenuSceneName);
}
