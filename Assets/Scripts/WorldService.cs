using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

	[Header("Scoring")]
	// the cursor is tracked on a plane this far in front of the camera (match BallController.distanceFromCamera)
	public float cursorDepth = 3.75f;
	// how close the cursor has to be (world units) to count as on the line / orb
	public float lineTolerance = 0.35f;
	public float orbTolerance = 0.4f;
	public float linePointsPerSecond = 10f;
	public float orbPointsPerSecond = 25f;
	// an orb can be scored while it's within this z distance of the cursor plane
	public float orbScoreWindow = 1f;

	[Header("Orb indicator")]
	// the square appears once the current orb is this far (along z) from the cursor plane
	public float indicatorDistance = 8f;
	// optional: the full screen Glitch.mat, spiked briefly whenever an orb blows up
	public Material glitchMaterial;
	public float glitchSpike = 0.45f;
	public float glitchDuration = 0.12f;

	[Header("Level end")]
	// scene loaded once the last orb is destroyed; leave empty to stay in the level
	public string endSceneName = "MainMenu";
	// time to let the last orb's shatter play out before leaving
	public float endDelay = 1.5f;

	// fired with the final score when the last orb is destroyed
	public static event Action<float> OnLevelComplete;
	public static event Action OnOrbHit;
	public static event Action OnStreakBroken;

	public float Score { get; private set; }
	// true while the camera is stopped waiting for the cursor to reach the current orb
	public bool IsWaiting { get; private set; }
	public bool IsFinished { get; private set; }
	public bool IsOnLine { get; private set; }

	// index of the next point whose trail hasn't been drawn yet
	private int next = 0;
	// index of the orb the player needs to reach next
	private int currentOrb = 0;
	private TargetIndicator indicator;
	private float glitchBase;
	private float glitchTimer;
	private float endTimer;

	void Start()
	{
		if (glitchMaterial != null) glitchBase = glitchMaterial.GetFloat("_Intensity");
	}

    void Update()
    {
		var cam = Camera.main;
		if (cam == null) return;

		if (IsFinished)
		{
			UpdateGlitch();
			UpdateLevelEnd();
			return;
		}

		if (!IsWaiting)
		{
			cam.transform.position += Vector3.forward * scrollSpeed * Time.deltaTime;
		}
		UpdateGlitch();

		var data = levelLoader?.GetLevelData();
		if (data is null || data.points is null || data.points.Length == 0) return;
		var pts = data.points;

		// only z matters, so height and sideways position don't affect when a trail appears
		while (next < pts.Length - 1 && pts[next].point.z - cam.transform.position.z < revealDistance)
		{
			CreateTrail().Begin(pts[next].point, Control(pts, next), pts[next + 1].point);
			next++;
		}

		float cursorZ = cam.transform.position.z + cursorDepth;
		Vector3 cursor = CursorPosition(cam);

		// tracing the line
		IsOnLine = cursorZ >= pts[0].point.z && cursorZ <= pts[^1].point.z
    	&& FlatDistance(cursor, PathPointAtZ(pts, cursorZ)) <= lineTolerance;

		if (IsOnLine)
		{
    	Score += linePointsPerSecond * Time.deltaTime; // unchanged, still exists, just now conditional on the new bool
		}

		Vector3 orb = pts[currentOrb].point;
		float orbAhead = orb.z - cursorZ;
		bool onOrb = FlatDistance(cursor, orb) <= orbTolerance;

		// holding the cursor on the orb as it arrives
		if (onOrb && Mathf.Abs(orbAhead) <= orbScoreWindow)
		{
			Score += orbPointsPerSecond * Time.deltaTime;
		}

		if (indicator == null && orbAhead <= indicatorDistance)
		{
			indicator = TargetIndicator.Create(orb);
		}
		if (indicator != null)
		{
			indicator.SetProgress(1 - orbAhead / indicatorDistance);
		}

		// the orb has reached the cursor plane: clear it if the cursor is on it, otherwise stop and wait
		if (orbAhead <= 0){
    if (onOrb) ClearOrb();
    else if (!IsWaiting)
    {
        IsWaiting = true;
        OnStreakBroken?.Invoke();
    }
	}
	
    }

	void OnDisable()
	{
		// the material is an asset, so put its intensity back or the change sticks after play mode
		if (glitchMaterial != null) glitchMaterial.SetFloat("_Intensity", glitchBase);
	}

	private void ClearOrb()
	{
		var targets = levelLoader.GetTargets();
		GameObject orbObject = currentOrb < targets.Count ? targets[currentOrb] : null;
		if (indicator != null) indicator.Explode(orbObject);
		else if (orbObject != null) orbObject.SetActive(false);
		indicator = null;
		glitchTimer = glitchDuration;

		IsWaiting = false;
		currentOrb++;
		OnOrbHit?.Invoke();
		if (currentOrb >= levelLoader.GetLevelData().points.Length)
		{
			// last orb destroyed: the camera stops here and the score is final
			IsFinished = true;
			endTimer = endDelay;
			OnLevelComplete?.Invoke(Score);
		}
	}

	private void UpdateLevelEnd()
	{
		if (string.IsNullOrEmpty(endSceneName) || endTimer <= 0) return;
		endTimer -= Time.deltaTime;
		if (endTimer <= 0) SceneManager.LoadScene(endSceneName);
	}

	private void UpdateGlitch()
	{
		if (glitchMaterial == null) return;
		glitchTimer -= Time.deltaTime;
		glitchMaterial.SetFloat("_Intensity", glitchTimer > 0 ? glitchSpike : glitchBase);
	}

	// where the mouse is on the plane cursorDepth in front of the camera
	private Vector3 CursorPosition(Camera cam)
	{
		Vector2 mouse = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
		return cam.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, cursorDepth));
	}

	// control point for the curve from point i to point i + 1
	private static Vector3 Control(Point[] pts, int i)
	{
		return pts[i].curved ? pts[i].point2 : (pts[i].point + pts[i + 1].point) / 2;
	}

	// the point on the path at depth z (z must be within the level's range)
	private static Vector3 PathPointAtZ(Point[] pts, float z)
	{
		int i = 0;
		while (i < pts.Length - 2 && z > pts[i + 1].point.z) i++;

		Vector3 a = pts[i].point, c = Control(pts, i), b = pts[i + 1].point;
		// z increases along each segment, so bisect for the t that lands on this depth
		float lo = 0, hi = 1;
		for (int n = 0; n < 20; n++)
		{
			float mid = (lo + hi) / 2;
			if (Trail.Bezier(a, c, b, mid).z < z) lo = mid;
			else hi = mid;
		}
		return Trail.Bezier(a, c, b, (lo + hi) / 2);
	}

	// distance across the screen plane, ignoring depth
	private static float FlatDistance(Vector3 p, Vector3 q)
	{
		return new Vector2(p.x - q.x, p.y - q.y).magnitude;
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
