using UnityEngine;

/// <summary>
/// Plays a random break sound each time an orb is destroyed. Never repeats the
/// same clip twice in a row.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class OrbBreakSFX : MonoBehaviour
{
    [SerializeField] private AudioClip[] breakClips;

    private AudioSource audioSource;
    private int lastIndex = -1;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        WorldService.OnOrbHit += HandleOrbHit;
    }

    private void OnDisable()
    {
        WorldService.OnOrbHit -= HandleOrbHit;
    }

    private void HandleOrbHit()
    {
        if (breakClips == null || breakClips.Length == 0) return;

        int index = PickIndex();
        lastIndex = index;

        AudioClip clip = breakClips[index];
        if (clip != null) audioSource.PlayOneShot(clip);
    }

    private int PickIndex()
    {
        if (breakClips.Length == 1) return 0;

        // pick uniformly among every index except the one played last time
        int index = Random.Range(0, breakClips.Length - 1);
        if (index >= lastIndex) index++;
        return index;
    }
}
