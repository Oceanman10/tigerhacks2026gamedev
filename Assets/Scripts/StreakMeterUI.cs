using UnityEngine;
using TMPro;

public class StreakMeterUI : MonoBehaviour
{
    public StreakTracker streakTracker;
    public TextMeshProUGUI rankText;
    public RectTransform rankTransform;

    [Header("Wobble")]
    public float wobbleSpeed = 6f;
    public float wobbleAngle = 4f;
    public float pulseSpeed = 4f;
    public float pulseScale = 0.05f;

    [Header("Rank Colors (index 0 = D, 4 = S)")]
    public Color[] rankColors = new Color[]
    {
        new Color(0.65f, 0.65f, 0.65f), // D - gray
        new Color(0.35f, 0.85f, 0.35f), // C - green
        new Color(0.3f, 0.55f, 1f),     // B - blue
        new Color(1f, 0.6f, 0.1f),      // A - orange
        new Color(1f, 0.15f, 0.15f)     // S - red
    };

    private Vector3 baseScale;
    private float popTimer;

    void Start()
    {
        baseScale = rankTransform.localScale;
        StreakTracker.OnRankChanged += HandleRankChanged;
    }

    void OnDestroy()
    {
        StreakTracker.OnRankChanged -= HandleRankChanged;
    }

    void Update()
    {
        if (streakTracker == null) return;

        int rank = streakTracker.CurrentRank;
        rankText.text = StreakTracker.RankNames[rank];
        rankText.color = rankColors[Mathf.Clamp(rank, 0, rankColors.Length - 1)];

        float intensity = 0.4f + rank * 0.2f;
        float angle = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAngle * intensity;
        float scalePulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale * intensity;

        if (popTimer > 0)
        {
            popTimer -= Time.deltaTime;
            scalePulse += (popTimer / 0.3f) * 0.3f;
        }

        rankTransform.localRotation = Quaternion.Euler(0, 0, angle);
        rankTransform.localScale = baseScale * scalePulse;
    }

    private void HandleRankChanged(int newRank)
    {
        popTimer = 0.3f;
    }
}