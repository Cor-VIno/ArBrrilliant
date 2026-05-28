using JingHongLu.Cameras;
using JingHongLu.Combat;
using UnityEngine;

namespace JingHongLu.Feedback
{
    public sealed class EnemyCombatFeedbackBinder : MonoBehaviour
    {
        [SerializeField] private EnemyCombatFeedbackData feedbackData;
        [SerializeField] private Damageable damageable;
        [SerializeField] private ShieldComponent shield;
        [SerializeField] private ToughnessComponent toughness;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private CameraShakeController2D cameraShake;
        [SerializeField] private Transform vfxSpawnPoint;
        [SerializeField] private bool logFeedback;

        private float previousShield = -1f;

        private void Awake()
        {
            ResolveReferences();
            CacheShieldValue();
        }

        private void OnEnable()
        {
            ResolveReferences();
            CacheShieldValue();

            if (damageable != null)
            {
                damageable.OnDamageTaken += HandleDamageTaken;
            }

            if (shield != null)
            {
                shield.OnShieldChanged += HandleShieldChanged;
                shield.OnShieldBlocked += HandleShieldBlocked;
                shield.OnShieldBroken += HandleShieldBroken;
            }

            if (toughness != null)
            {
                toughness.OnBroken += HandleToughnessBroken;
            }
        }

        private void OnDisable()
        {
            if (damageable != null)
            {
                damageable.OnDamageTaken -= HandleDamageTaken;
            }

            if (shield != null)
            {
                shield.OnShieldChanged -= HandleShieldChanged;
                shield.OnShieldBlocked -= HandleShieldBlocked;
                shield.OnShieldBroken -= HandleShieldBroken;
            }

            if (toughness != null)
            {
                toughness.OnBroken -= HandleToughnessBroken;
            }
        }

        private void ResolveReferences()
        {
            if (damageable == null)
            {
                damageable = GetComponentInParent<Damageable>();
            }

            if (shield == null)
            {
                shield = GetComponentInParent<ShieldComponent>();
            }

            if (toughness == null)
            {
                toughness = GetComponentInParent<ToughnessComponent>();
            }

            if (audioSource == null)
            {
                TryGetComponent(out audioSource);
            }

            if (cameraShake == null && Camera.main != null)
            {
                cameraShake = Camera.main.GetComponent<CameraShakeController2D>();
            }

            if (vfxSpawnPoint == null)
            {
                vfxSpawnPoint = transform;
            }
        }

        private void CacheShieldValue()
        {
            previousShield = shield != null ? shield.CurrentShield : -1f;
        }

        private void HandleDamageTaken(DamageInfo damageInfo)
        {
            if (feedbackData == null)
            {
                return;
            }

            PlayCue("Hit", feedbackData.HitCue, ResolveDirection(damageInfo));

            if (feedbackData.ShakeOnHit)
            {
                PlayShake(
                    feedbackData.HitShakeDuration,
                    feedbackData.HitShakeStrength);
            }
        }

        private void HandleShieldChanged(float current, float max)
        {
            if (feedbackData == null)
            {
                previousShield = current;
                return;
            }

            bool hasPreviousValue = previousShield >= 0f;
            bool shieldReduced = hasPreviousValue && current < previousShield;
            previousShield = current;

            if (!shieldReduced)
            {
                return;
            }

            PlayCue("Shield hit", feedbackData.ShieldHitCue, ResolveFacingDirection());
        }

        private void HandleShieldBlocked()
        {
            if (feedbackData == null)
            {
                return;
            }

            PlayCue("Shield block", feedbackData.ShieldBlockCue, ResolveFacingDirection());
        }

        private void HandleShieldBroken()
        {
            if (feedbackData == null)
            {
                return;
            }

            PlayCue("Shield break", feedbackData.ShieldBreakCue, ResolveFacingDirection());

            if (feedbackData.ShakeOnShieldBreak)
            {
                PlayShake(
                    feedbackData.ShieldBreakShakeDuration,
                    feedbackData.ShieldBreakShakeStrength);
            }
        }

        private void HandleToughnessBroken()
        {
            if (feedbackData == null)
            {
                return;
            }

            PlayCue(
                "Toughness break",
                feedbackData.ToughnessBreakCue,
                ResolveFacingDirection());

            if (feedbackData.ShakeOnToughnessBreak)
            {
                PlayShake(
                    feedbackData.ToughnessBreakShakeDuration,
                    feedbackData.ToughnessBreakShakeStrength);
            }
        }

        private void PlayCue(string label, FeedbackCue cue, Vector2 direction)
        {
            if (cue == null)
            {
                if (logFeedback)
                {
                    Debug.Log($"[EnemyFeedback] {label} cue missing.", this);
                }

                return;
            }

            Vector2 safeDirection = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : ResolveFacingDirection();
            bool playedAny = false;

            AudioClip audioClip = cue.GetAudioClip();

            if (audioClip != null && audioSource != null)
            {
                audioSource.pitch = Mathf.Max(0.01f, cue.Pitch);
                audioSource.PlayOneShot(audioClip, Mathf.Max(0f, cue.Volume));
                playedAny = true;
            }

            if (cue.VfxPrefab != null)
            {
                Vector3 position = ResolveSpawnPosition(cue, safeDirection);
                Quaternion rotation = cue.RotateToDirection
                    ? Quaternion.Euler(
                        0f,
                        0f,
                        Mathf.Atan2(safeDirection.y, safeDirection.x) * Mathf.Rad2Deg)
                    : Quaternion.identity;
                Transform parent = cue.ParentToCaster ? transform : null;
                GameObject instance = Instantiate(cue.VfxPrefab, position, rotation, parent);

                if (cue.DestroyDelay > 0f)
                {
                    Destroy(instance, cue.DestroyDelay);
                }

                playedAny = true;
            }

            if (logFeedback)
            {
                string result = playedAny ? "played" : "has no configured output";
                Debug.Log($"[EnemyFeedback] {label} cue {result}.", this);
            }
        }

        private Vector3 ResolveSpawnPosition(FeedbackCue cue, Vector2 direction)
        {
            Transform origin = vfxSpawnPoint != null ? vfxSpawnPoint : transform;

            switch (cue.SpawnPoint)
            {
                case FeedbackSpawnPoint.CasterForward:
                    return origin.position +
                           (Vector3)(direction * cue.LocalOffset.x) +
                           Vector3.up * cue.LocalOffset.y;
                case FeedbackSpawnPoint.CasterFeet:
                case FeedbackSpawnPoint.CasterCenter:
                case FeedbackSpawnPoint.Projectile:
                case FeedbackSpawnPoint.WorldPosition:
                default:
                    return origin.position + ResolveDirectionalOffset(cue.LocalOffset, direction);
            }
        }

        private static Vector3 ResolveDirectionalOffset(Vector2 localOffset, Vector2 direction)
        {
            float sign = direction.x < 0f ? -1f : 1f;
            return new Vector3(localOffset.x * sign, localOffset.y, 0f);
        }

        private Vector2 ResolveDirection(DamageInfo damageInfo)
        {
            if (damageInfo.KnockbackDirection.sqrMagnitude > 0.0001f)
            {
                return damageInfo.KnockbackDirection.normalized;
            }

            if (damageInfo.Attacker != null)
            {
                float delta = transform.position.x - damageInfo.Attacker.transform.position.x;

                if (Mathf.Abs(delta) > 0.0001f)
                {
                    return delta < 0f ? Vector2.left : Vector2.right;
                }
            }

            return ResolveFacingDirection();
        }

        private Vector2 ResolveFacingDirection()
        {
            return transform.localScale.x < 0f ? Vector2.left : Vector2.right;
        }

        private void PlayShake(float duration, float strength)
        {
            if (cameraShake == null)
            {
                return;
            }

            cameraShake.Shake(duration, strength);
        }
    }
}
