using System.Collections;
using JingHongLu.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PerfectDodgeDebug : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private KeyCode triggerKey = KeyCode.K;

    [Header("Cooldown")]
    [SerializeField] private float perfectDodgeCooldown = 2f;

    [Header("Slow Motion")]
    [SerializeField] private TimeScaleController timeScaleController;
    [SerializeField] private float slowMotionScale = 0.15f;
    [SerializeField] private float slowMotionDuration = 0.35f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip enterClip;
    [SerializeField] private AudioClip exitClip;

    [Header("Post Processing")]
    [SerializeField] private Volume globalVolume;

    [Header("Visual")]
    [SerializeField] private float dodgeContrast = 25f;

    [Header("BGM Filter")]
    [SerializeField] private AudioLowPassFilter bgmLowPassFilter;
    [SerializeField] private float dodgeLowPassCutoff = 800f;
    [SerializeField] private float filterRestoreDuration = 0.35f;

    private ColorAdjustments colorAdjustments;

    private float originalSaturation;
    private float originalContrast;
    private float originalCutoff;

    private bool isPlaying;

    // cooldown uses REAL TIME
    private float nextAvailableTime;

    private void Start()
    {
        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out colorAdjustments);

            if (colorAdjustments != null)
            {
                originalSaturation = colorAdjustments.saturation.value;
                originalContrast = colorAdjustments.contrast.value;
            }
        }

        if (bgmLowPassFilter != null)
        {
            originalCutoff = bgmLowPassFilter.cutoffFrequency;
        }
    }

    private void Update()
    {
        // Debug trigger
        if (triggerKey != KeyCode.None &&
            Input.GetKeyDown(triggerKey))
        {
            TriggerPerfectDodgeEffect();
        }
    }

    // Future real perfect dodge event entrance
    public void OnPerfectDodgeTriggered()
    {
        TriggerPerfectDodgeEffect();
    }

    private void TriggerPerfectDodgeEffect()
    {
        // already playing
        if (isPlaying)
        {
            return;
        }

        // cooldown check (unscaled real time)
        if (Time.unscaledTime < nextAvailableTime)
        {
            return;
        }

        nextAvailableTime =
            Time.unscaledTime + perfectDodgeCooldown;

        StartCoroutine(PlayPerfectDodgeEffect());
    }

    private IEnumerator PlayPerfectDodgeEffect()
    {
        isPlaying = true;

        // Enter slow motion
        if (timeScaleController != null)
        {
            timeScaleController.EnterSlowMotion(slowMotionScale);
        }

        // Enter black & white
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = -100f;
            colorAdjustments.contrast.value = dodgeContrast;
        }

        // Apply BGM low pass
        if (bgmLowPassFilter != null)
        {
            bgmLowPassFilter.cutoffFrequency = dodgeLowPassCutoff;
        }

        // Play enter sound
        if (audioSource != null && enterClip != null)
        {
            audioSource.PlayOneShot(enterClip);
        }

        // Wait in real time
        yield return new WaitForSecondsRealtime(
            slowMotionDuration);

        // Exit slow motion
        if (timeScaleController != null)
        {
            timeScaleController.ExitSlowMotion();
        }

        // Restore color
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value =
                originalSaturation;

            colorAdjustments.contrast.value =
                originalContrast;
        }

        // Smoothly restore BGM filter
        if (bgmLowPassFilter != null)
        {
            StartCoroutine(RestoreLowPassFilter());
        }

        // Play exit sound
        if (audioSource != null && exitClip != null)
        {
            audioSource.PlayOneShot(exitClip);
        }

        isPlaying = false;
    }

    private IEnumerator RestoreLowPassFilter()
    {
        float startFrequency =
            bgmLowPassFilter.cutoffFrequency;

        float timer = 0f;

        while (timer < filterRestoreDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.SmoothStep(
                0f,
                1f,
                timer / filterRestoreDuration);

            bgmLowPassFilter.cutoffFrequency =
                Mathf.Lerp(
                    startFrequency,
                    originalCutoff,
                    t);

            yield return null;
        }

        bgmLowPassFilter.cutoffFrequency =
            originalCutoff;
    }
}