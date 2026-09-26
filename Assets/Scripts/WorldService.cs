using UnityEngine;

public class WorldService : MonoBehaviour
{
	public LevelLoader levelLoader;
	// optional: a prefab with a LineRenderer + Trail; a plain one is made if left empty
	public Trail trailPrefab;
	public float triggerRadius = 3f;
	public float trailWidth = 0.1f;

	// index of the next point the camera needs to reach
	private int next = 0;

    void Start()
	{
		// scene "Easy" loads Levels/easy.json, "Medium" loads medium.json, etc.
		string level = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower();
		levelLoader = new LevelLoader(System.IO.Path.Combine(Application.dataPath, "Levels", level + ".json"));
		levelLoader.LoadLevel();
	}

    void Update()
    {
		var data = levelLoader?.GetLevelData();
		if (data is null || data.points is null) return;
		var pts = data.points;
		if (next >= pts.Length - 1) return;

		var cam = Camera.main;
		if (cam == null) return;

		// ignore height so only horizontal (x/z) distance matters
		Vector3 camPos = cam.transform.position;
		Vector3 target = pts[next].point;
		Vector2 offset = new Vector2(camPos.x - target.x, camPos.z - target.z);
		if (offset.magnitude < triggerRadius)
		{
			var from = pts[next];
			var to = pts[next + 1];
			Vector3 ctrl = from.curved ? from.point2 : (from.point + to.point) / 2;
			CreateTrail().Begin(from.point, ctrl, to.point);
			next++;
		}
    }

	private Trail CreateTrail()
	{
		if (trailPrefab != null) return Instantiate(trailPrefab);

		var obj = new GameObject("Trail " + next);
		var line = obj.AddComponent<LineRenderer>();
		line.startWidth = trailWidth;
		line.endWidth = trailWidth;
		line.material = new Material(Shader.Find("Sprites/Default"));
		return obj.AddComponent<Trail>();
	}
}
