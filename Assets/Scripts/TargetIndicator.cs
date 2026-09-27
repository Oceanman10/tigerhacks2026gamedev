using UnityEngine;

// A 2D square that closes in on the current orb as it approaches. When the orb is cleared,
// the orb shatters into glowing particles, the square shatters into smaller squares, and both fade out
public class TargetIndicator : MonoBehaviour
{
    public float startSize = 2.5f;
    public float endSize = 0.5f;
    public float lineWidth = 0.04f;
    public float spinSpeed = 45f;
    public Color color = new Color(0.3f, 0.9f, 1f);
    // HDR multiplier so bloom picks up the square and the particles
    public float glow = 4f;

    [Header("Orb shatter")]
    public int orbParticleCount = 60;
    public float orbParticleSpeed = 2.5f;
    public float orbParticleSize = 0.06f;

    [Header("Square shatter")]
    // pieces along each side of the square
    public int piecesPerSide = 6;
    public float pieceSpeed = 1.5f;
    public float pieceSize = 0.08f;

    // how long the particles take to fade out
    public float fadeDuration = 0.8f;

    private LineRenderer line;
    private Material lineMaterial;
    private Material orbParticleMaterial;
    private Material pieceMaterial;
    private float progress;
    private float spin;

    // -1 means not exploding yet
    private float explodeTime = -1;
    private Light flash;

    private static Texture2D circleTexture;

    public static TargetIndicator Create(Vector3 position)
    {
        var obj = new GameObject("Orb Indicator");
        obj.transform.position = position;
        return obj.AddComponent<TargetIndicator>();
    }

    void Awake()
    {
        lineMaterial = GlowMaterial(color * glow);
        line = gameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 4;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.material = lineMaterial;
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

        ShatterOrb(orb);
        ShatterSquare();
        line.enabled = false;

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
        // the flash is quicker than the particles
        flash.intensity = Mathf.Lerp(8f, 0f, explodeTime / 0.3f);

        // particles live at most fadeDuration, so everything is gone by then
        if (explodeTime >= fadeDuration) Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (lineMaterial != null) Destroy(lineMaterial);
        if (orbParticleMaterial != null) Destroy(orbParticleMaterial);
        if (pieceMaterial != null) Destroy(pieceMaterial);
    }

    // the orb bursts into round particles flying out in every direction
    private void ShatterOrb(GameObject orb)
    {
        Color orbColor = color;
        float radius = endSize * 0.25f;
        if (orb != null)
        {
            orbColor = OrbColor(orb);
            radius = orb.transform.lossyScale.x / 2;
            orb.SetActive(false);
        }

        orbParticleMaterial = ParticleMaterial(orbColor * glow, CircleTexture());
        var ps = CreateBurst("Orb Particles", orbParticleMaterial);
        var emit = new ParticleSystem.EmitParams();
        for (int i = 0; i < orbParticleCount; i++)
        {
            Vector3 dir = Random.onUnitSphere;
            emit.position = transform.position + dir * radius;
            emit.velocity = dir * orbParticleSpeed * Random.Range(0.4f, 1.2f);
            emit.startSize = orbParticleSize * Random.Range(0.5f, 1.5f);
            emit.startLifetime = fadeDuration * Random.Range(0.6f, 1f);
            ps.Emit(emit, 1);
        }
    }

    // each side of the square breaks into small squares that drift outward and spin
    private void ShatterSquare()
    {
        pieceMaterial = ParticleMaterial(color * glow, null);
        var ps = CreateBurst("Square Pieces", pieceMaterial);
        var emit = new ParticleSystem.EmitParams();

        float radius = endSize * 0.7071f;
        for (int side = 0; side < 4; side++)
        {
            float a0 = (spin + 45 + 90 * side) * Mathf.Deg2Rad;
            float a1 = (spin + 45 + 90 * (side + 1)) * Mathf.Deg2Rad;
            Vector3 from = new Vector3(Mathf.Cos(a0), Mathf.Sin(a0), 0) * radius;
            Vector3 to = new Vector3(Mathf.Cos(a1), Mathf.Sin(a1), 0) * radius;

            for (int i = 0; i < piecesPerSide; i++)
            {
                Vector3 local = Vector3.Lerp(from, to, (i + 0.5f) / piecesPerSide);
                Vector3 outward = local.normalized;
                emit.position = transform.position + local;
                emit.velocity = (outward + (Vector3)Random.insideUnitCircle * 0.4f) * pieceSpeed * Random.Range(0.6f, 1.4f);
                emit.startSize = pieceSize * Random.Range(0.6f, 1.2f);
                // start lined up with the square, then tumble
                emit.rotation = -spin;
                emit.angularVelocity = Random.Range(-360f, 360f);
                emit.startLifetime = fadeDuration * Random.Range(0.7f, 1f);
                ps.Emit(emit, 1);
            }
        }
    }

    // a particle system that only emits what we hand it, slows down, shrinks a little and fades out
    private ParticleSystem CreateBurst(string name, Material material)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(transform, false);
        var ps = obj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = fadeDuration;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSpeed = 0;
        main.maxParticles = 500;

        var emission = ps.emission;
        emission.enabled = false;
        var shape = ps.shape;
        shape.enabled = false;

        var drag = ps.limitVelocityOverLifetime;
        drag.enabled = true;
        drag.drag = 2f;

        var fade = ps.colorOverLifetime;
        fade.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
            new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0.8f, 0.4f), new GradientAlphaKey(0, 1) });
        fade.color = gradient;

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 1, 1, 0.4f));

        obj.GetComponent<ParticleSystemRenderer>().material = material;
        ps.Play();
        return ps;
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

    // the orb's glow color, falling back to the indicator color
    private Color OrbColor(GameObject orb)
    {
        var renderer = orb.GetComponent<Renderer>();
        var material = renderer != null ? renderer.sharedMaterial : null;
        if (material == null) return color;

        if (material.HasProperty("_EmissionColor"))
        {
            Color emission = material.GetColor("_EmissionColor");
            // emission is HDR, so scale it back to a 0-1 color and let glow brighten it
            if (emission.maxColorComponent > 0.01f) return emission / emission.maxColorComponent;
        }
        if (material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
        return color;
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

    // alpha blended and tinted by particle color, so colorOverLifetime can fade it out
    private static Material ParticleMaterial(Color hdrTint, Texture2D texture)
    {
        var material = new Material(Shader.Find("Sprites/Default"));
        material.color = hdrTint;
        if (texture != null) material.mainTexture = texture;
        return material;
    }

    // soft round dot for the orb particles (no texture gives plain squares, which the square pieces use)
    private static Texture2D CircleTexture()
    {
        if (circleTexture != null) return circleTexture;

        const int size = 32;
        circleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        circleTexture.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(size / 2f, size / 2f)) / (size / 2f);
                float alpha = Mathf.Clamp01((1 - d) * 3);
                circleTexture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        circleTexture.Apply();
        return circleTexture;
    }
}
