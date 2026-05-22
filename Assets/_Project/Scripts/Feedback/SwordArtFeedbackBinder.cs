using System.Collections.Generic;
using JingHongLu.SwordArts;
using UnityEngine;

namespace JingHongLu.Feedback
{
    public sealed class SwordArtFeedbackBinder : MonoBehaviour
    {
        [SerializeField] private SwordArtExecutor swordArtExecutor;
        [SerializeField] private Animator animator;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Transform casterRoot;
        [SerializeField] private Transform casterCenter;
        [SerializeField] private Transform casterFeet;
        [SerializeField] private Transform feedbackParent;
        [SerializeField] private SwordArtFeedbackData[] swordArtFeedbacks;
        [SerializeField] private bool logFeedback;

        private readonly Dictionary<SwordArtData, SwordArtFeedbackData> feedbackBySwordArt = new();

        private void Awake()
        {
            ResolveReferences();
            RebuildLookup();
        }

        private void OnEnable()
        {
            ResolveReferences();
            RebuildLookup();

            if (swordArtExecutor != null)
            {
                swordArtExecutor.OnSwordArtExecutionStarted += HandleSwordArtExecutionStarted;
                swordArtExecutor.OnSwordArtExecutionFinished += HandleSwordArtExecutionFinished;
            }
        }

        private void OnDisable()
        {
            if (swordArtExecutor != null)
            {
                swordArtExecutor.OnSwordArtExecutionStarted -= HandleSwordArtExecutionStarted;
                swordArtExecutor.OnSwordArtExecutionFinished -= HandleSwordArtExecutionFinished;
            }
        }

        private void ResolveReferences()
        {
            if (swordArtExecutor == null)
            {
                TryGetComponent(out swordArtExecutor);
            }

            if (audioSource == null)
            {
                TryGetComponent(out audioSource);
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (casterRoot == null)
            {
                casterRoot = transform;
            }
        }

        private void RebuildLookup()
        {
            feedbackBySwordArt.Clear();

            if (swordArtFeedbacks == null)
            {
                return;
            }

            foreach (SwordArtFeedbackData feedbackData in swordArtFeedbacks)
            {
                if (feedbackData == null || feedbackData.SwordArt == null)
                {
                    continue;
                }

                feedbackBySwordArt[feedbackData.SwordArt] = feedbackData;
            }
        }

        private void HandleSwordArtExecutionStarted(SwordArtData swordArt)
        {
            if (TryGetFeedback(swordArt, out SwordArtFeedbackData feedbackData))
            {
                PlayCue(feedbackData.ExecutionStartedCue, swordArt);
            }
        }

        private void HandleSwordArtExecutionFinished(SwordArtData swordArt)
        {
            if (TryGetFeedback(swordArt, out SwordArtFeedbackData feedbackData))
            {
                PlayCue(feedbackData.ExecutionFinishedCue, swordArt);
            }
        }

        private bool TryGetFeedback(SwordArtData swordArt, out SwordArtFeedbackData feedbackData)
        {
            if (swordArt != null && feedbackBySwordArt.TryGetValue(swordArt, out feedbackData))
            {
                return true;
            }

            feedbackData = null;
            return false;
        }

        private void PlayCue(FeedbackCue cue, SwordArtData swordArt)
        {
            if (cue == null)
            {
                return;
            }

            Vector2 direction = ResolveSafeDirection();

            if (!string.IsNullOrWhiteSpace(cue.AnimatorTrigger) && animator != null)
            {
                animator.SetTrigger(cue.AnimatorTrigger);
            }

            if (cue.AudioClip != null && audioSource != null)
            {
                audioSource.pitch = Mathf.Max(0.01f, cue.Pitch);
                audioSource.PlayOneShot(cue.AudioClip, Mathf.Max(0f, cue.Volume));
            }

            if (cue.VfxPrefab != null)
            {
                Vector3 position = ResolveSpawnPosition(cue, direction);
                Quaternion rotation = cue.RotateToDirection
                    ? Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg)
                    : Quaternion.identity;
                Transform parent = cue.ParentToCaster ? feedbackParent != null ? feedbackParent : casterRoot : null;
                GameObject instance = Instantiate(cue.VfxPrefab, position, rotation, parent);

                if (cue.DestroyDelay > 0f)
                {
                    Destroy(instance, cue.DestroyDelay);
                }
            }

            if (logFeedback)
            {
                Debug.Log($"[Feedback] Sword art cue played. SwordArt={swordArt?.DisplayName}", this);
            }
        }

        private Vector2 ResolveSafeDirection()
        {
            return casterRoot != null && casterRoot.localScale.x < 0f
                ? Vector2.left
                : Vector2.right;
        }

        private Vector3 ResolveSpawnPosition(FeedbackCue cue, Vector2 direction)
        {
            Transform fallback = casterRoot != null ? casterRoot : transform;

            switch (cue.SpawnPoint)
            {
                case FeedbackSpawnPoint.CasterFeet:
                    return (casterFeet != null ? casterFeet.position : fallback.position) +
                           (Vector3)cue.LocalOffset;
                case FeedbackSpawnPoint.CasterForward:
                    return fallback.position +
                           (Vector3)(direction * cue.LocalOffset.x) +
                           Vector3.up * cue.LocalOffset.y;
                case FeedbackSpawnPoint.WorldPosition:
                case FeedbackSpawnPoint.Projectile:
                case FeedbackSpawnPoint.CasterCenter:
                default:
                    return (casterCenter != null ? casterCenter.position : fallback.position) +
                           (Vector3)cue.LocalOffset;
            }
        }
    }
}
