using UnityEngine;

public class WorldService : MonoBehaviour
{
	public LevelLoader levelLoader;
	// optional: a prefab with a LineRenderer + Trail; a plain one is made if left empty
	public Trail trailPrefab;
	public float trailWidth = 0.1f;
	// how fast the camera scrolls forward along z
	public float scrollSpeed => GameSettings.Instance.ballVelocity;
	// a point's trail is drawn once the point is this far ahead of the camera (along z)
	public float revealDistance = 20f;

	// index of the next point whose trail hasn't been drawn yet
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
		var cam = Camera.main;
		if (cam == null) return;

		cam.transform.position += Vector3.forward * scrollSpeed * Time.deltaTime;

		var data = levelLoader?.GetLevelData();
		if (data is null || data.points is null) return;
		var pts = data.points;

		// only z matters, so height and sideways position don't affect when a trail appears
		while (next < pts.Length - 1 && pts[next].point.z - cam.transform.position.z < revealDistance)
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
