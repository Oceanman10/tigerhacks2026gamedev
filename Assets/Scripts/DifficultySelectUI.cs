using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultySelectUI : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "Gameplay";

    public void SelectDifficulty(string levelName)
    {
        GameSession.SelectedLevelName = levelName;
        SceneManager.LoadScene(gameplaySceneName);
    }
}
