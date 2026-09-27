using UnityEngine;
using System;

public class StreakTracker : MonoBehaviour
{
    public int[] rankThresholds = { 0, 3, 6, 9, 12 }; // consecutive hits needed to REACH C, B, A, S
    public static readonly string[] RankNames = { "D", "C", "B", "A", "S" };

    public static event Action<int> OnRankChanged;

    public int ConsecutiveHits { get; private set; }
    public int CurrentRank { get; private set; }
    public float Multiplier => Mathf.Pow(2f, CurrentRank); // D=1x, C=2x, B=4x, A=8x, S=16x

    void OnEnable()
    {
        WorldService.OnOrbHit += HandleHit;
        WorldService.OnStreakBroken += HandleBreak;
    }

    void OnDisable()
    {
        WorldService.OnOrbHit -= HandleHit;
        WorldService.OnStreakBroken -= HandleBreak;
    }

    private void HandleHit()
    {
        ConsecutiveHits++;
        UpdateRank();
    }

    private void HandleBreak()
    {
        ConsecutiveHits = 0;
        UpdateRank();
    }

    private void UpdateRank()
    {
        int newRank = 0;
        for (int i = rankThresholds.Length - 1; i >= 0; i--)
        {
            if (ConsecutiveHits >= rankThresholds[i]) { newRank = i; break; }
        }
        if (newRank != CurrentRank)
        {
            CurrentRank = newRank;
            OnRankChanged?.Invoke(CurrentRank);
        }
    }
}