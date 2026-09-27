using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

[Serializable]
public class Point {
    public Vector3 point;
    // set curved to true to bend the trail to the next point toward point2
    public bool curved;
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
    // spawned orbs, in the same order as level_data.points
    private readonly List<GameObject> targets = new List<GameObject>();

    public LevelLoader(string file_path)
    {
        this.file_path = file_path;
    }

    public LevelData GetLevelData()
    {
        return this.level_data;
    }

    public List<GameObject> GetTargets()
    {
        return this.targets;
    }

	public void LoadLevel()
	{
		if(this.level_data is null)
		{
			Debug.Log("Loading level " + file_path);
			this.level_data = this.LoadLevelData();
			ClampToCameraView(this.level_data);
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
			this.targets.Add(GameObject.Instantiate(target_obj, p.point, Quaternion.identity));
		}
	}

    // Keeps every orb (and the curve control points between them) within what the
    // camera can actually see once it reaches the cursor plane - the tightest point
    // of the frustum an orb still needs to be visible at, since it only gets closer
    // (and the frustum only gets narrower) from there (see WorldService.cursorDepth).
    // Levels are authored in world units with no idea of the camera's FOV or aspect
    // ratio, so without this a narrower window/monitor can push orbs off-screen.
    private static void ClampToCameraView(LevelData level)
    {
        if (level?.points == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        WorldService world = UnityEngine.Object.FindAnyObjectByType<WorldService>();
        float depth = world != null ? world.cursorDepth : 3.5f;

        float halfHeight = depth * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * cam.aspect;

        // leave room for the orb's own radius and its shrinking target indicator
        const float margin = 0.3f;
        halfWidth = Mathf.Max(0f, halfWidth - margin);
        halfHeight = Mathf.Max(0f, halfHeight - margin);

        foreach (var p in level.points)
        {
            p.point.x = Mathf.Clamp(p.point.x, -halfWidth, halfWidth);
            p.point.y = Mathf.Clamp(p.point.y, -halfHeight, halfHeight);
            if (p.curved)
            {
                p.point2.x = Mathf.Clamp(p.point2.x, -halfWidth, halfWidth);
                p.point2.y = Mathf.Clamp(p.point2.y, -halfHeight, halfHeight);
            }
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
