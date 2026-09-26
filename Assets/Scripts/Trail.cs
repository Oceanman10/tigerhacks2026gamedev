using UnityEngine;

// Draws a quadratic bezier from a to b (bent toward c), growing over drawDuration
[RequireComponent(typeof(LineRenderer))]
public class Trail : MonoBehaviour
{
    public float drawDuration = 0.75f;
    public int resolution = 32;

    private LineRenderer line;
    private Vector3 a, c, b;
    // -1 means not started
    private float progress = -1;

    public void Begin(Vector3 start, Vector3 control, Vector3 end)
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 0;
        a = start;
        c = control;
        b = end;
        progress = 0;
    }

    void Update()
    {
        if (progress < 0 || progress >= 1) return;
        progress = Mathf.Min(1, progress + Time.deltaTime / drawDuration);

        int count = Mathf.Max(2, Mathf.CeilToInt(resolution * progress) + 1);
        line.positionCount = count;
        for (int i = 0; i < count; i++)
        {
            line.SetPosition(i, Bezier(a, c, b, progress * i / (count - 1)));
        }
    }

    public static Vector3 Bezier(Vector3 a, Vector3 c, Vector3 b, float t)
    {
        float u = 1 - t;
        return u * u * a + 2 * u * t * c + t * t * b;
    }
}
