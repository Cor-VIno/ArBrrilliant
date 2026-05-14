using JingHongLu.Combat;
using JingHongLu.Player;
using UnityEngine;

namespace JingHongLu.SwordArts
{
    public sealed class SwordArtExecutor : MonoBehaviour
    {
        [SerializeField] private SwordArtMatcher matcher;
        [SerializeField] private PlayerAim2D aim;
        [SerializeField] private TeamId ownerTeam = TeamId.Player;
        [SerializeField] private bool logExecution = true;

        private bool warnedMissingMatcher;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (matcher != null)
            {
                matcher.OnSwordArtTriggered += HandleSwordArtTriggered;
                return;
            }

            LogMissingMatcherWarning();
        }

        private void OnDisable()
        {
            if (matcher != null)
            {
                matcher.OnSwordArtTriggered -= HandleSwordArtTriggered;
            }
        }

        private void ResolveReferences()
        {
            if (matcher == null)
            {
                TryGetComponent(out matcher);
            }

            if (aim == null)
            {
                TryGetComponent(out aim);
            }
        }

        private void HandleSwordArtTriggered(SwordArtData swordArt)
        {
            if (swordArt == null)
            {
                return;
            }

            SwordArtEffectData effectData = swordArt.EffectData;

            if (effectData == null)
            {
                Debug.LogWarning(
                    $"{swordArt.DisplayName} has no SwordArtEffectData assigned.",
                    this);
                return;
            }

            switch (effectData.EffectType)
            {
                case SwordArtEffectType.InstantHitbox:
                    SpawnInstantHitbox(swordArt, effectData);
                    break;
                case SwordArtEffectType.LineArea:
                    SpawnLineArea(swordArt, effectData);
                    break;
                case SwordArtEffectType.HealingArea:
                    SpawnHealingArea(swordArt, effectData);
                    break;
                default:
                    Debug.LogWarning(
                        $"{swordArt.DisplayName} uses unsupported sword art effect: {effectData.EffectType}.",
                        this);
                    break;
            }
        }

        private void SpawnInstantHitbox(
            SwordArtData swordArt,
            SwordArtEffectData effectData)
        {
            Vector2 aimDirection = GetAimDirection();
            Vector3 center = CalculateHitboxCenter(effectData, aimDirection);
            float angleDegrees = GetAngleDegrees(aimDirection, effectData);

            GameObject hitboxObject =
                new GameObject($"{swordArt.SwordArtId}_SwordArtHitbox");
            hitboxObject.transform.position = center;

            Hitbox2D hitbox = hitboxObject.AddComponent<Hitbox2D>();
            hitbox.Initialize(
                owner: gameObject,
                ownerTeam: ownerTeam,
                damage: effectData.Damage,
                size: effectData.HitboxSize,
                direction: aimDirection,
                lifetime: effectData.HitboxDuration,
                targetLayerMask: effectData.TargetLayerMask,
                hitOncePerTarget: true,
                canCritical: effectData.CanCritical,
                sourceSkill: null,
                gizmoColor: effectData.GizmoColor,
                shape: effectData.HitboxShape,
                angleDegrees: angleDegrees,
                radius: effectData.HitboxRadius,
                innerRadius: effectData.HitboxInnerRadius,
                arcAngle: effectData.HitboxArcAngle,
                destroyOnFirstHit: false,
                sourceDisplayName: swordArt.DisplayName,
                canKnockUp: effectData.CanKnockUp,
                knockUpVelocity: effectData.KnockUpVelocity,
                airborneDuration: effectData.AirborneDuration);

            if (logExecution)
            {
                Debug.Log($"执行剑招效果：{swordArt.DisplayName}", this);
            }
        }

        private Vector2 GetAimDirection()
        {
            if (aim != null && aim.AimDirection.sqrMagnitude > 0.0001f)
            {
                return aim.AimDirection.normalized;
            }

            return transform.localScale.x >= 0f ? Vector2.right : Vector2.left;
        }

        private void SpawnLineArea(
            SwordArtData swordArt,
            SwordArtEffectData effectData)
        {
            Vector2 startPoint = transform.position;
            Vector2 aimDirection = GetAimDirection();
            Vector2 targetPoint = GetLineAreaTargetPoint(startPoint, aimDirection);
            Vector2 toTarget = targetPoint - startPoint;
            float maxLength = Mathf.Max(0.01f, effectData.LineMaxLength);

            if (toTarget.sqrMagnitude < 0.0001f)
            {
                targetPoint = startPoint + aimDirection * (maxLength * 0.5f);
            }
            else if (toTarget.magnitude > maxLength)
            {
                targetPoint = startPoint + toTarget.normalized * maxLength;
            }

            GameObject areaObject =
                new GameObject($"{swordArt.SwordArtId}_LineArea");
            areaObject.transform.position = startPoint;

            SwordArtLineArea2D lineArea =
                areaObject.AddComponent<SwordArtLineArea2D>();
            lineArea.Initialize(
                owner: gameObject,
                ownerTeam: ownerTeam,
                startPoint: startPoint,
                endPoint: targetPoint,
                lineWidth: effectData.LineWidth,
                duration: effectData.AreaDuration,
                tickInterval: effectData.TickInterval,
                tickDamage: effectData.TickDamage,
                finalDamage: effectData.FinalDamage,
                targetLayerMask: effectData.TargetLayerMask,
                sourceDisplayName: swordArt.DisplayName,
                finalCanKnockUp: effectData.FinalCanKnockUp,
                finalKnockUpVelocity: effectData.FinalKnockUpVelocity,
                finalAirborneDuration: effectData.FinalAirborneDuration,
                gizmoColor: effectData.GizmoColor);

            if (logExecution)
            {
                Debug.Log($"执行剑招效果：{swordArt.DisplayName}", this);
            }
        }

        private Vector2 GetLineAreaTargetPoint(
            Vector2 startPoint,
            Vector2 aimDirection)
        {
            if (aim != null)
            {
                return aim.MouseWorldPosition;
            }

            return startPoint + aimDirection;
        }

        private void SpawnHealingArea(
            SwordArtData swordArt,
            SwordArtEffectData effectData)
        {
            if (!TryGetComponent(out Health health))
            {
                health = GetComponentInParent<Health>();
            }

            if (health == null)
            {
                Debug.LogWarning("Healing area requires Health on owner.", this);
                return;
            }

            Vector2 center = transform.position;
            GameObject areaObject =
                new GameObject($"{swordArt.SwordArtId}_HealingArea");
            areaObject.transform.position = center;

            SwordArtHealingArea2D healingArea =
                areaObject.AddComponent<SwordArtHealingArea2D>();
            healingArea.Initialize(
                target: transform,
                targetHealth: health,
                center: center,
                radius: effectData.HealingRadius,
                duration: effectData.HealingDuration,
                tickInterval: effectData.HealingTickInterval,
                healAmountPerTick: effectData.HealingAmountPerTick,
                sourceDisplayName: swordArt.DisplayName,
                gizmoColor: effectData.GizmoColor);

            if (logExecution)
            {
                Debug.Log($"执行剑招效果：{swordArt.DisplayName}", this);
            }
        }

        private Vector3 CalculateHitboxCenter(
            SwordArtEffectData effectData,
            Vector2 aimDirection)
        {
            Vector2 origin = transform.position;
            Vector2 forward = aimDirection.sqrMagnitude > 0.0001f
                ? aimDirection.normalized
                : Vector2.right;
            Vector2 right = new Vector2(-forward.y, forward.x);
            Vector2 offset = effectData.HitboxOffset;
            Vector2 worldOffset = forward * offset.x + right * offset.y;

            return origin + worldOffset;
        }

        private float GetAngleDegrees(
            Vector2 aimDirection,
            SwordArtEffectData effectData)
        {
            float baseAngle =
                Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            return baseAngle + effectData.HitboxRotationOffset;
        }

        private void LogMissingMatcherWarning()
        {
            if (warnedMissingMatcher)
            {
                return;
            }

            warnedMissingMatcher = true;
            Debug.LogWarning(
                $"{nameof(SwordArtExecutor)} requires a {nameof(SwordArtMatcher)} on the same GameObject or an assigned reference.",
                this);
        }
    }
}
