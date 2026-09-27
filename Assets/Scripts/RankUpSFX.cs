using UnityEngine;

/// <summary>
/// Plays a sound whenever the streak rank goes up, and a break sound whenever it drops.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class RankUpSFX : MonoBehaviour
{
    [Header("Clip played on reaching each rank (index 0 = D, 4 = S)")]
    [SerializeField] private AudioClip[] rankUpClips = new AudioClip[5];
    [Header("Clip played on dropping to any lower rank")]
    [SerializeField] private AudioClip rankDownClip;

    private AudioSource audioSource;
    private int lastRank;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        StreakTracker.OnRankChanged += HandleRankChanged;
    }

    private void OnDisable()
    {
        StreakTracker.OnRankChanged -= HandleRankChanged;
    }

    private void HandleRankChanged(int newRank)
    {
        bool rankedUp = newRank > lastRank;
        lastRank = newRank;

        AudioClip clip = rankedUp
            ? (newRank < rankUpClips.Length ? rankUpClips[newRank] : null)
            : rankDownClip;
        if (clip != null) audioSource.PlayOneShot(clip);
    }
}
