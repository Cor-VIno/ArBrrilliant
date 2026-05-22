using System.Collections.Generic;
using JingHongLu.Combat;
using JingHongLu.Player;
using JingHongLu.Skills;
using UnityEngine;

namespace JingHongLu.Feedback
{
    public sealed class PlayerSkillFeedbackBinder : MonoBehaviour
    {
        [SerializeField] private PlayerSkillController skillController;
        [SerializeField] private PlayerDashController2D dashController;
        [SerializeField] private PlayerPerfectDodgeController2D perfectDodgeController;
        [SerializeField] private Animator animator;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Transform casterRoot;
        [SerializeField] private Transform casterCenter;
        [SerializeField] private Transform casterFeet;
        [SerializeField] private Transform feedbackParent;
        [SerializeField] private SkillFeedbackData[] skillFeedbacks;
        [SerializeField] private SkillFeedbackData dashSkillFeedback;
        [SerializeField] private FeedbackCue perfectDodgeCue = new FeedbackCue();
        [SerializeField] private bool logFeedback;

        private readonly Dictionary<SkillData, SkillFeedbackData> feedbackBySkill = new();
        private Vector2 lastResolvedDirection = Vector2.right;

        private void Awake()
        {
            ResolveReferences();
            RebuildLookup();
        }

        private void OnEnable()
        {
            ResolveReferences();
            RebuildLookup();

            if (skillController != null)
            {
                skillController.OnSkillCastStarted += HandleSkillCastStarted;
                skillController.OnSkillExecuted += HandleSkillExecuted;
                skillController.OnSkillCastFinished += HandleSkillCastFinished;
                skillController.OnSkillChargeStarted += HandleSkillChargeStarted;
                skillController.OnSkillChargeReleased += HandleSkillChargeReleased;
                skillController.OnSkillDirectionResolved += HandleSkillDirectionResolved;
                skillController.OnProjectileSpawned += HandleProjectileSpawned;
            }

            if (dashController != null)
            {
                dashController.OnDashStarted += HandleDashStarted;
                dashController.OnDashFinished += HandleDashFinished;
            }

            if (perfectDodgeController != null)
            {
                perfectDodgeController.OnPerfectDodgeTriggered += HandlePerfectDodgeTriggered;
            }
        }

        private void OnDisable()
        {
            if (skillController != null)
            {
                skillController.OnSkillCastStarted -= HandleSkillCastStarted;
                skillController.OnSkillExecuted -= HandleSkillExecuted;
                skillController.OnSkillCastFinished -= HandleSkillCastFinished;
                skillController.OnSkillChargeStarted -= HandleSkillChargeStarted;
                skillController.OnSkillChargeReleased -= HandleSkillChargeReleased;
                skillController.OnSkillDirectionResolved -= HandleSkillDirectionResolved;
                skillController.OnProjectileSpawned -= HandleProjectileSpawned;
            }

            if (dashController != null)
            {
                dashController.OnDashStarted -= HandleDashStarted;
                dashController.OnDashFinished -= HandleDashFinished;
            }

            if (perfectDodgeController != null)
            {
                perfectDodgeController.OnPerfectDodgeTriggered -= HandlePerfectDodgeTriggered;
            }
        }

        private void ResolveReferences()
        {
            if (skillController == null)
            {
                TryGetComponent(out skillController);
            }

            if (dashController == null)
            {
                TryGetComponent(out dashController);
            }

            if (perfectDodgeController == null)
            {
                TryGetComponent(out perfectDodgeController);
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
            feedbackBySkill.Clear();

            if (skillFeedbacks == null)
            {
                return;
            }

            foreach (SkillFeedbackData feedbackData in skillFeedbacks)
            {
                if (feedbackData == null || feedbackData.Skill == null)
                {
                    continue;
                }

                feedbackBySkill[feedbackData.Skill] = feedbackData;
            }
        }

        private void HandleSkillCastStarted(SkillData skill)
        {
            if (TryGetFeedback(skill, out SkillFeedbackData feedbackData))
            {
                PlayCue(feedbackData.CastStartedCue, skill, lastResolvedDirection);
            }
        }

        private void HandleSkillExecuted(SkillData skill)
        {
            if (TryGetFeedback(skill, out SkillFeedbackData feedbackData))
            {
                PlayCue(feedbackData.ExecutedCue, skill, lastResolvedDirection);
            }
        }

        private void HandleSkillCastFinished(SkillData skill)
        {
            if (TryGetFeedback(skill, out SkillFeedbackData feedbackData))
            {
                PlayCue(feedbackData.CastFinishedCue, skill, lastResolvedDirection);
            }
        }

        private void HandleSkillChargeStarted(SkillData skill)
        {
            if (TryGetFeedback(skill, out SkillFeedbackData feedbackData))
            {
                PlayCue(feedbackData.ChargeStartedCue, skill, lastResolvedDirection);
            }
        }

        private void HandleSkillChargeReleased(SkillData skill, float chargeTime)
        {
            if (TryGetFeedback(skill, out SkillFeedbackData feedbackData))
            {
                PlayCue(feedbackData.ChargeReleasedCue, skill, lastResolvedDirection);
            }
        }

        private void HandleSkillDirectionResolved(SkillData skill, Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.0001f)
            {
                lastResolvedDirection = direction.normalized;
            }
        }

        private void HandleProjectileSpawned(SkillData skill, GameObject projectile)
        {
            if (TryGetFeedback(skill, out SkillFeedbackData feedbackData))
            {
                Vector3? worldPosition = projectile != null
                    ? projectile.transform.position
                    : null;
                PlayCue(
                    feedbackData.ProjectileSpawnedCue,
                    skill,
                    lastResolvedDirection,
                    projectile,
                    worldPosition);
            }
        }

        private void HandleDashStarted(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.0001f)
            {
                lastResolvedDirection = direction.normalized;
            }

            SkillFeedbackData feedbackData = ResolveDashFeedback();
            if (feedbackData != null)
            {
                PlayCue(feedbackData.DashStartedCue, feedbackData.Skill, lastResolvedDirection);
            }
        }

        private void HandleDashFinished()
        {
            SkillFeedbackData feedbackData = ResolveDashFeedback();
            if (feedbackData != null)
            {
                PlayCue(feedbackData.DashFinishedCue, feedbackData.Skill, lastResolvedDirection);
            }
        }

        private void HandlePerfectDodgeTriggered(PerfectDodgeEventData eventData)
        {
            Vector2 direction = eventData.DodgeDirection.sqrMagnitude > 0.0001f
                ? eventData.DodgeDirection.normalized
                : lastResolvedDirection;
            PlayCue(perfectDodgeCue, null, direction, null, eventData.ContactPoint);
        }

        private bool TryGetFeedback(SkillData skill, out SkillFeedbackData feedbackData)
        {
            if (skill != null && feedbackBySkill.TryGetValue(skill, out feedbackData))
            {
                return true;
            }

            feedbackData = null;
            return false;
        }

        private SkillFeedbackData ResolveDashFeedback()
        {
            if (dashSkillFeedback != null)
            {
                return dashSkillFeedback;
            }

            foreach (SkillFeedbackData feedbackData in feedbackBySkill.Values)
            {
                if (feedbackData != null &&
                    feedbackData.Skill != null &&
                    feedbackData.Skill.ExecutionType == SkillExecutionType.Dash)
                {
                    return feedbackData;
                }
            }

            return null;
        }

        private void PlayCue(
            FeedbackCue cue,
            SkillData skill,
            Vector2 direction,
            GameObject projectile = null,
            Vector3? worldPosition = null)
        {
            if (cue == null)
            {
                return;
            }

            Vector2 safeDirection = ResolveSafeDirection(direction);

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
                Vector3 position = ResolveSpawnPosition(cue, safeDirection, projectile, worldPosition);
                Quaternion rotation = cue.RotateToDirection
                    ? Quaternion.Euler(0f, 0f, Mathf.Atan2(safeDirection.y, safeDirection.x) * Mathf.Rad2Deg)
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
                Debug.Log($"[Feedback] Skill cue played. Skill={skill?.DisplayName}", this);
            }
        }

        private Vector2 ResolveSafeDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.0001f)
            {
                return direction.normalized;
            }

            if (lastResolvedDirection.sqrMagnitude > 0.0001f)
            {
                return lastResolvedDirection.normalized;
            }

            return casterRoot != null && casterRoot.localScale.x < 0f
                ? Vector2.left
                : Vector2.right;
        }

        private Vector3 ResolveSpawnPosition(
            FeedbackCue cue,
            Vector2 direction,
            GameObject projectile,
            Vector3? worldPosition)
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
                case FeedbackSpawnPoint.Projectile:
                    return (projectile != null ? projectile.transform.position : fallback.position) +
                           (Vector3)cue.LocalOffset;
                case FeedbackSpawnPoint.WorldPosition:
                    return (worldPosition ?? fallback.position) + (Vector3)cue.LocalOffset;
                case FeedbackSpawnPoint.CasterCenter:
                default:
                    return (casterCenter != null ? casterCenter.position : fallback.position) +
                           (Vector3)cue.LocalOffset;
            }
        }
    }
}
