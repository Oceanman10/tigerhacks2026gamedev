using System;
using System.Security.Cryptography;
using UnityEngine;

[Serializable]
public class Point {
    public Vector3 point;
    // should be null unless you want a point curve between two points
    public Vector3 point2;
}

[Serializable]
public class LevelData {
    public Point[] points;
}

public class LevelLoader
{
    private readonly string file_path;
    private LevelData level_data;

    public LevelLoader(string file_path)
    {
        this.file_path = file_path;
    }

    public LevelData GetLevelData()
    {
        return this.level_data;
    }

	public void LoadLevel()
	{
		if(this.level_data is null)
		{
			Debug.Log("Loading level " + file_path);
			this.level_data = this.LoadLevelData();
		}
		var target_obj = GameObject.FindWithTag("GameTarget");
		if(target_obj is null)
		{
			Debug.LogError("jigga");
			return;
		}

		foreach (var p in this.level_data.points)
		{
			Debug.Log("Loading gameobj at point: " + p.point.x + ", " + p.point.y + ", " + p.point.z);
			GameObject.Instantiate(target_obj, p.point, Quaternion.identity);
		}
	}

    private LevelData LoadLevelData()
    {
        // Check if the file actually exists before reading
        if (System.IO.File.Exists(this.file_path))
        {
            // Read the entire JSON file into a string
            string jsonText = System.IO.File.ReadAllText(this.file_path);

            // Deserialize the string back into a C# object
            LevelData loadedData = JsonUtility.FromJson<LevelData>(jsonText);
			Debug.Log("Loaded json data into memory");
            return loadedData;
        }
        else
        {
            Debug.LogError("Save file not found at " + this.file_path);
            return null;
        }
    }
}
