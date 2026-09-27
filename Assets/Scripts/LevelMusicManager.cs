using UnityEngine;

[System.Serializable]
public class LevelMusicEntry
{
    public string levelName; // must match GameSession.SelectedLevelName exactly ("Easy", "Medium", "Hard")
    public AudioClip musicClip;
}

[RequireComponent(typeof(AudioSource))]
public class LevelMusicManager : MonoBehaviour
{
    [SerializeField] private LevelMusicEntry[] levelMusic;

    private AudioSource musicSource;

    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        AudioClip clip = GetClipForLevel(GameSession.SelectedLevelName);

        if (clip == null)
        {
            Debug.LogWarning($"No music assigned for level '{GameSession.SelectedLevelName}'.");
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    private AudioClip GetClipForLevel(string levelName)
    {
        foreach (var entry in levelMusic)
        {
            if (entry.levelName == levelName)
                return entry.musicClip;
        }
        return null;
    }
}
