using System.IO;
using UnityEngine;

/// <summary>
/// Attach this to an empty GameObject in your Gameplay scene (e.g. "LevelManager").
/// LevelLoader itself is a plain C# class (not a MonoBehaviour), so it can't be
/// attached directly to a GameObject - this script instantiates it and calls it.
/// </summary>
public class LevelInitializer : MonoBehaviour
{
    private void Start()
    {
        string fileName = GameSession.SelectedLevelName + ".json";

        // Assumes Easy.json / Medium.json / Hard.json live directly inside
        // Assets/StreamingAssets/. Adjust the path below if yours are nested
        // in a subfolder, e.g. Path.Combine(Application.streamingAssetsPath, "Levels", fileName)
        string fullPath = Path.Combine(Application.streamingAssetsPath, fileName);

        LevelLoader loader = new LevelLoader(fullPath);
        loader.LoadLevel();
    }
}
