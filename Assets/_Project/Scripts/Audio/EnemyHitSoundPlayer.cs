using UnityEngine;
using JingHongLu.Combat;

public class EnemyHitSoundPlayer : MonoBehaviour
{
    [Header("Health Hit Sounds")]
    [SerializeField] private AudioClip[] hitClips;

    [Header("Shield Hit Sounds")]
    [SerializeField] private AudioClip[] shieldHitClips;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Damageable")]
    [SerializeField] private Damageable damageable;

    [Header("Shield")]
    [SerializeField] private ShieldComponent shieldComponent;

    private int lastHealthIndex = -1;
    private int lastShieldIndex = -1;

    private float lastShieldValue;

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

        if (shieldComponent == null)
        {
            shieldComponent =
                GetComponent<ShieldComponent>();
        }

        if (shieldComponent == null)
        {
            shieldComponent =
                GetComponentInParent<ShieldComponent>();
        }

        if (shieldComponent != null)
        {
            lastShieldValue =
                shieldComponent.CurrentShield;
        }
    }

    private void OnEnable()
    {
        if (damageable != null)
        {
            damageable.OnDamageTaken +=
                PlayRandomHealthHitSound;
        }

        if (shieldComponent != null)
        {
            shieldComponent.OnShieldChanged +=
                OnShieldChanged;
        }
    }

    private void OnDisable()
    {
        if (damageable != null)
        {
            damageable.OnDamageTaken -=
                PlayRandomHealthHitSound;
        }

        if (shieldComponent != null)
        {
            shieldComponent.OnShieldChanged -=
                OnShieldChanged;
        }
    }

    private void PlayRandomHealthHitSound(
        DamageInfo info)
    {
        // shield still exists
        // don't play flesh hit
        if (shieldComponent != null &&
            shieldComponent.HasShield)
        {
            return;
        }

        PlayRandomClip(
            hitClips,
            ref lastHealthIndex);
    }

    private void OnShieldChanged(
        float current,
        float max)
    {
        // shield damaged
        if (current < lastShieldValue)
        {
            PlayRandomClip(
                shieldHitClips,
                ref lastShieldIndex);
        }

        lastShieldValue = current;
    }

    private void PlayRandomClip(
        AudioClip[] clips,
        ref int lastIndex)
    {
        if (clips == null ||
            clips.Length == 0)
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
            index = Random.Range(
                0,
                clips.Length);
        }
        while (
            clips.Length > 1 &&
            index == lastIndex);

        lastIndex = index;

        audioSource.PlayOneShot(
            clips[index]);
    }
}