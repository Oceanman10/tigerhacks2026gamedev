using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndScreenUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Optional: auto-wire buttons instead of using OnClick() in Inspector")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Rank Colors (index 0 = D, 4 = S)")]
    [SerializeField] private Color[] rankColors = new Color[]
    {
        new Color(0.65f, 0.65f, 0.65f), // D - gray
        new Color(0.35f, 0.85f, 0.35f), // C - green
        new Color(0.3f, 0.55f, 1f),     // B - blue
        new Color(1f, 0.6f, 0.1f),      // A - orange
        new Color(1f, 0.15f, 0.15f)     // S - red
    };

    private void OnEnable()
    {
        if (retryButton != null) retryButton.onClick.AddListener(Retry);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    private void OnDisable()
    {
        if (retryButton != null) retryButton.onClick.RemoveListener(Retry);
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(GoToMainMenu);
    }

    public void SetResults(int rank, float score)
    {
        rank = Mathf.Clamp(rank, 0, StreakTracker.RankNames.Length - 1);
        if (rankText != null)
        {
            Color color = rankColors[Mathf.Clamp(rank, 0, rankColors.Length - 1)];
            rankText.text = $"Rank <color=#{ColorUtility.ToHtmlStringRGB(color)}>{StreakTracker.RankNames[rank]}</color>";
        }
        if (scoreText != null) scoreText.text = $"Score {Mathf.FloorToInt(score):N0}";
    }

    public void Retry() => PauseManager.Instance.RestartLevel();
    public void GoToMainMenu() => PauseManager.Instance.LoadScene(mainMenuSceneName);
}
