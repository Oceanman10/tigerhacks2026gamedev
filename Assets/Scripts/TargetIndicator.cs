using UnityEngine;

// A 2D square that closes in on the current orb as it approaches,
// then blows up into glowing shards with a light flash when the orb is cleared
public class TargetIndicator : MonoBehaviour
{
    public float startSize = 2.5f;
    public float endSize = 0.5f;
    public float lineWidth = 0.04f;
    public float spinSpeed = 45f;
    public Color color = new Color(0.3f, 0.9f, 1f);
    // HDR multiplier so bloom picks up the square and the burst
    public float glow = 4f;

    public int shardCount = 14;
    public float shardSpeed = 4f;
    public float explodeDuration = 0.5f;

    private LineRenderer line;
    private Material glowMaterial;
    private float progress;
    private float spin;

    // -1 means not exploding yet
    private float explodeTime = -1;
    private Transform[] shards;
    private Vector3[] shardVelocities;
    private Light flash;

    public static TargetIndicator Create(Vector3 position)
    {
        var obj = new GameObject("Orb Indicator");
        obj.transform.position = position;
        return obj.AddComponent<TargetIndicator>();
    }

    void Awake()
    {
        glowMaterial = GlowMaterial(color * glow);
        line = gameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 4;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.material = glowMaterial;
        SetSquare(startSize, Vector3.zero);
    }

    // 0 = orb just came into range, 1 = orb has reached the cursor
    public void SetProgress(float t)
    {
        progress = Mathf.Clamp01(t);
    }

    public void Explode(GameObject orb)
    {
        if (explodeTime >= 0) return;
        explodeTime = 0;
        if (orb != null) orb.SetActive(false);

        shards = new Transform[shardCount];
        shardVelocities = new Vector3[shardCount];
        for (int i = 0; i < shardCount; i++)
        {
            var shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(shard.GetComponent<Collider>());
            shard.GetComponent<Renderer>().material = glowMaterial;
            shard.transform.position = transform.position;
            shard.transform.rotation = Random.rotation;
            shard.transform.localScale = Vector3.one * Random.Range(0.05f, 0.12f);
            shards[i] = shard.transform;
            // mostly outward across the screen, with a little depth
            Vector2 dir = Random.insideUnitCircle.normalized;
            shardVelocities[i] = new Vector3(dir.x, dir.y, Random.Range(-0.3f, 0.3f)) * shardSpeed * Random.Range(0.6f, 1.2f);
        }

        flash = new GameObject("Orb Flash").AddComponent<Light>();
        flash.transform.SetParent(transform, false);
        flash.type = LightType.Point;
        flash.color = color;
        flash.range = 6f;
    }

    void Update()
    {
        if (explodeTime < 0)
        {
            spin += spinSpeed * Time.deltaTime;
            // once it's fully closed in and waiting on the player, jitter it like a glitch
            Vector3 jitter = progress >= 1 ? (Vector3)(Random.insideUnitCircle * 0.01f) : Vector3.zero;
            SetSquare(Mathf.Lerp(startSize, endSize, progress), jitter);
            return;
        }

        explodeTime += Time.deltaTime;
        float t = explodeTime / explodeDuration;
        if (t >= 1)
        {
            foreach (var shard in shards) Destroy(shard.gameObject);
            Destroy(gameObject);
            return;
        }

        // square bursts outward and thins out
        spin += spinSpeed * 6 * Time.deltaTime;
        SetSquare(endSize * (1 + 5 * t), Vector3.zero);
        line.startWidth = line.endWidth = lineWidth * 3 * (1 - t);

        for (int i = 0; i < shards.Length; i++)
        {
            shards[i].position += shardVelocities[i] * Time.deltaTime;
            shards[i].Rotate(720 * Time.deltaTime * Vector3.one);
            shards[i].localScale = Vector3.Lerp(shards[i].localScale, Vector3.zero, t);
            shardVelocities[i] *= 1 - 3 * Time.deltaTime;
        }

        flash.intensity = Mathf.Lerp(8f, 0f, t);
    }

    void OnDestroy()
    {
        if (glowMaterial != null) Destroy(glowMaterial);
    }

    private void SetSquare(float size, Vector3 offset)
    {
        // corners sit half a diagonal from the center
        float radius = size * 0.7071f;
        for (int i = 0; i < 4; i++)
        {
            float angle = (spin + 45 + 90 * i) * Mathf.Deg2Rad;
            line.SetPosition(i, offset + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius);
        }
    }

    private static Material GlowMaterial(Color hdrColor)
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        var material = new Material(shader);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", hdrColor);
        else material.color = hdrColor;
        return material;
    }
}
