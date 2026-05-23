using UnityEngine;
using JingHongLu.Combat;

public class EnemyHitSoundPlayer : MonoBehaviour
{
    [Header("Hit Sounds")]
    [SerializeField] private AudioClip[] hitClips;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Damageable")]
    [SerializeField] private Damageable damageable;

    private int lastIndex = -1;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (damageable == null)
        {
            damageable = GetComponent<Damageable>();
        }

        if (damageable == null)
        {
            damageable = GetComponentInParent<Damageable>();
        }
    }

    private void OnEnable()
    {
        if (damageable != null)
        {
            damageable.OnDamageTaken += PlayRandomHitSound;
        }
    }

    private void OnDisable()
    {
        if (damageable != null)
        {
            damageable.OnDamageTaken -= PlayRandomHitSound;
        }
    }

    private void PlayRandomHitSound(DamageInfo info)
    {
        if (hitClips == null || hitClips.Length == 0)
        {
            return;
        }

        if (audioSource == null)
        {
            return;
        }

        int index;

        do
        {
            index = Random.Range(0, hitClips.Length);
        }
        while (hitClips.Length > 1 && index == lastIndex);

        lastIndex = index;

        audioSource.PlayOneShot(hitClips[index]);
    }
}