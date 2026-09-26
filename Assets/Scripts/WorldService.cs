using UnityEngine;

public class WorldService : MonoBehaviour
{
	public LevelLoader levelLoader;
    void Start()
	{
		levelLoader = new LevelLoader(System.IO.Path.Combine(Application.dataPath, "Levels/easy.json"));
		levelLoader.LoadLevel();
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
