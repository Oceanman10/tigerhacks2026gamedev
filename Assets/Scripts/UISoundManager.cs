using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance { get; private set; }

    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        audioSource = GetComponent<AudioSource>();
        audioSource.ignoreListenerPause = true;
    }

    public void PlayHover()
    {
        if (hoverClip != null) audioSource.PlayOneShot(hoverClip);
    }

    public void PlayClick()
    {
        if (clickClip != null) audioSource.PlayOneShot(clickClip);
    }
}
