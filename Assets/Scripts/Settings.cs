using UnityEngine;

public class GameSettings : MonoBehaviour
{
    private static GameSettings _instance;
    public static GameSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("GameSettings");
                _instance = obj.AddComponent<GameSettings>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    public float ballVelocity = 5f;
    public float pathRadius = 3f;
    public float mouseSensitivity = 1f;
    public float volume = 1f;
}