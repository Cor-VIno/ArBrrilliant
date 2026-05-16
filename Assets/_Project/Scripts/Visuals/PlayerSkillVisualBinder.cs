using JingHongLu.Player;
using JingHongLu.Skills;
using UnityEngine;

namespace JingHongLu.Visuals
{
    public sealed class PlayerSkillVisualBinder : MonoBehaviour
    {
        [SerializeField] private PlayerSkillController skillController;
        [SerializeField] private PlayerAim2D aim;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform casterCenter;
        [SerializeField] private Transform casterFront;
        [SerializeField] private Transform casterFeet;
        [SerializeField] private Transform weaponSocket;
        [SerializeField] private bool logMissingReferences = true;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (skillController == null)
            {
                if (logMissingReferences)
                {
                    Debug.LogWarning(
                        $"{nameof(PlayerSkillVisualBinder)} requires a {nameof(PlayerSkillController)}.",
                        this);
                }

                return;
            }

            skillController.OnSkillCastStarted += HandleSkillCastStarted;
            skillController.OnSkillExecuted += HandleSkillExecuted;
            skillController.OnSkillCastFinished += HandleSkillCastFinished;
        }

        private void OnDisable()
        {
            if (skillController == null)
            {
                return;
            }

            skillController.OnSkillCastStarted -= HandleSkillCastStarted;
            skillController.OnSkillExecuted -= HandleSkillExecuted;
            skillController.OnSkillCastFinished -= HandleSkillCastFinished;
        }

        private void ResolveReferences()
        {
            if (skillController == null)
            {
                TryGetComponent(out skillController);
            }

            if (aim == null)
            {
                TryGetComponent(out aim);
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (casterCenter == null)
            {
                casterCenter = transform;
            }
        }

        private void HandleSkillCastStarted(SkillData skill)
        {
            PlayCue(skill?.VisualData?.CastStartedCue);
        }

        private void HandleSkillExecuted(SkillData skill)
        {
            PlayCue(skill?.VisualData?.ExecutedCue);
        }

        private void HandleSkillCastFinished(SkillData skill)
        {
            PlayCue(skill?.VisualData?.CastFinishedCue);
        }

        private void PlayCue(VisualCueData cue)
        {
            if (cue == null)
            {
                return;
            }

            if (animator != null && !string.IsNullOrWhiteSpace(cue.AnimatorTrigger))
            {
                animator.SetTrigger(cue.AnimatorTrigger);
            }

            if (cue.VfxPrefab == null)
            {
                return;
            }

            Transform spawnTransform = ResolveSpawnTransform(cue.SpawnPoint);
            Vector3 position = ResolveSpawnPosition(cue, spawnTransform);
            Quaternion rotation = cue.RotateToAimDirection
                ? Quaternion.Euler(0f, 0f, GetAimAngleDegrees())
                : Quaternion.identity;

            GameObject instance = Instantiate(cue.VfxPrefab, position, rotation);

            if (cue.ParentToSpawnPoint && spawnTransform != null)
            {
                instance.transform.SetParent(spawnTransform, true);
            }

            if (cue.DestroyDelay > 0f)
            {
                Destroy(instance, cue.DestroyDelay);
            }
        }

        private Transform ResolveSpawnTransform(VisualSpawnPointType spawnPoint)
        {
            return spawnPoint switch
            {
                VisualSpawnPointType.CasterFront => casterFront != null
                    ? casterFront
                    : casterCenter,
                VisualSpawnPointType.CasterFeet => casterFeet != null
                    ? casterFeet
                    : casterCenter,
                VisualSpawnPointType.Weapon => weaponSocket != null
                    ? weaponSocket
                    : casterCenter,
                VisualSpawnPointType.WorldPosition => null,
                _ => casterCenter
            };
        }

        private Vector3 ResolveSpawnPosition(
            VisualCueData cue,
            Transform spawnTransform)
        {
            Vector3 offset = cue.LocalOffset;

            if (cue.SpawnPoint == VisualSpawnPointType.WorldPosition)
            {
                Vector3 worldPosition = aim != null
                    ? (Vector3)aim.MouseWorldPosition
                    : transform.position;
                return worldPosition + offset;
            }

            Vector3 origin = spawnTransform != null
                ? spawnTransform.position
                : transform.position;
            return origin + offset;
        }

        private float GetAimAngleDegrees()
        {
            Vector2 direction = aim != null && aim.AimDirection.sqrMagnitude > 0.0001f
                ? aim.AimDirection
                : Vector2.right;

            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
    }
}
