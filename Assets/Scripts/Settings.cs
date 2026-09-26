using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    public float ballVelocity = 5f;
    public float pathRadius = 3f;
    public float mouseSensitivity = 1f;
    public float volume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // keeps settings across scene loads
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // These get called by UI elements (sliders and inputbox ui)
    public void SetBallVelocity(float value) => ballVelocity = value;
    public void SetPathRadius(float value) => pathRadius = value;
    public void SetMouseSensitivity(float value) => mouseSensitivity = value;
    public void SetVolume(float value)
    {
        volume = value;
        AudioListener.volume = value; // applies immediately, global game volume
    }
}