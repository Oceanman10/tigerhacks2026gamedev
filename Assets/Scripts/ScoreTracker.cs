using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    public WorldService worldService;
    public StreakTracker streakTracker;

    public float Score { get; private set; }

    void Update()
    {
        if (worldService == null || streakTracker == null) return;

        if (worldService.IsOnLine)
        {
            Score += worldService.linePointsPerSecond * streakTracker.Multiplier * Time.deltaTime;
        }
    }
}