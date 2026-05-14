using System;
using System.Collections;
using System.Collections.Generic;
using JingHongLu.Combat;
using JingHongLu.Input;
using JingHongLu.Player;
using UnityEngine;

namespace JingHongLu.Skills
{
    public sealed class PlayerSkillController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputReader inputReader = null;
        [SerializeField] private PlayerMotor2D motor = null;
        [SerializeField] private PlayerAim2D aim = null;
        [SerializeField] private PlayerDashController2D dashController = null;
        [SerializeField] private PlayerAirborneTargetFinder2D airborneTargetFinder = null;
        [SerializeField] private PlayerSkillLoadout skillLoadout = null;

        private readonly Dictionary<SkillData, float> cooldownTimers = new Dictionary<SkillData, float>();
        private readonly List<SkillData> cooldownSkills = new List<SkillData>();

        private bool isCasting;

        public event Action<SkillData> OnSkillCastStarted;
        public event Action<SkillData> OnSkillExecuted;
        public event Action<SkillData> OnSkillCastFinished;

        public bool IsCasting => isCasting;

        private void Awake()
        {
            if (inputReader == null)
            {
                TryGetComponent(out inputReader);
            }

            if (motor == null)
            {
                TryGetComponent(out motor);
            }

            if (aim == null)
            {
                TryGetComponent(out aim);
            }

            if (dashController == null)
            {
                TryGetComponent(out dashController);
            }

            if (airborneTargetFinder == null)
            {
                TryGetComponent(out airborneTargetFinder);
            }
        }

        private void Update()
        {
            TickCooldowns();
            ReadSkillSlotInput();
        }

        public void SetSkillLoadout(PlayerSkillLoadout newSkillLoadout)
        {
            skillLoadout = newSkillLoadout;
        }

        private void TickCooldowns()
        {
            for (int i = cooldownSkills.Count - 1; i >= 0; i--)
            {
                SkillData skill = cooldownSkills[i];

                if (skill == null || !cooldownTimers.TryGetValue(skill, out float timer))
                {
                    cooldownSkills.RemoveAt(i);
                    continue;
                }

                timer -= Time.deltaTime;

                if (timer <= 0f)
                {
                    cooldownTimers.Remove(skill);
                    cooldownSkills.RemoveAt(i);
                    continue;
                }

                cooldownTimers[skill] = timer;
            }
        }

        private void ReadSkillSlotInput()
        {
            if (inputReader == null)
            {
                return;
            }

            if (inputReader.SkillSlot1Pressed)
            {
                TryCastSlot(SkillSlot.Slot1);
            }

            if (inputReader.SkillSlot2Pressed)
            {
                TryCastSlot(SkillSlot.Slot2);
            }

            if (inputReader.SkillSlot3Pressed)
            {
                TryCastSlot(SkillSlot.Slot3);
            }

            if (inputReader.SkillSlot4Pressed)
            {
                TryCastSlot(SkillSlot.Slot4);
            }
        }

        private void TryCastSlot(SkillSlot slot)
        {
            if (skillLoadout == null)
            {
                return;
            }

            SkillData skill = skillLoadout.GetSkill(slot);

            if (!CanCast(skill))
            {
                return;
            }

            StartCoroutine(CastSkillRoutine(skill));
        }

        private bool CanCast(SkillData skill)
        {
            return skill != null
                && !isCasting
                && !cooldownTimers.ContainsKey(skill);
        }

        private IEnumerator CastSkillRoutine(SkillData skill)
        {
            isCasting = true;
            StartCooldown(skill);
            OnSkillCastStarted?.Invoke(skill);

            if (skill.CastTime > 0f)
            {
                yield return new WaitForSeconds(skill.CastTime);
            }

            OnSkillExecuted?.Invoke(skill);
            yield return ExecuteSkillRoutine(skill);

            if (skill.RecoveryTime > 0f)
            {
                yield return new WaitForSeconds(skill.RecoveryTime);
            }

            isCasting = false;
            OnSkillCastFinished?.Invoke(skill);
        }

        private void StartCooldown(SkillData skill)
        {
            if (skill.Cooldown <= 0f)
            {
                return;
            }

            cooldownTimers[skill] = skill.Cooldown;

            if (!cooldownSkills.Contains(skill))
            {
                cooldownSkills.Add(skill);
            }
        }

        private IEnumerator ExecuteSkillRoutine(SkillData skill)
        {
            switch (skill.ExecutionType)
            {
                case SkillExecutionType.InstantHitbox:
                    SpawnInstantHitbox(skill);
                    yield break;
                case SkillExecutionType.Projectile:
                    SpawnProjectile(skill);
                    yield break;
                case SkillExecutionType.Dash:
                    yield return ExecuteDashRoutine(skill);
                    yield break;
            }
        }

        private IEnumerator ExecuteDashRoutine(SkillData skill)
        {
            if (skill.DashData == null)
            {
                Debug.LogWarning(
                    $"{skill.DisplayName} uses Dash execution, but has no DashData assigned.",
                    this);
                yield break;
            }

            if (dashController == null)
            {
                Debug.LogWarning(
                    $"{skill.DisplayName} uses Dash execution, but PlayerDashController2D is missing.",
                    this);
                yield break;
            }

            Vector2 aimDirection = ResolveDashDirection(skill.DashData);
            SpawnDashHitbox(skill, aimDirection, skill.DashData.Duration);

            yield return dashController.DashRoutine(skill.DashData, aimDirection);
        }

        private void SpawnInstantHitbox(SkillData skill)
        {
            Vector2 aimDirection = GetAimDirection();
            float angleDegrees = GetAimAngleDegrees(aimDirection, skill);
            Vector3 center = CalculateHitboxCenter(skill, aimDirection);

            GameObject hitboxObject = new GameObject($"{skill.SkillId}_Hitbox");
            hitboxObject.transform.position = center;

            Hitbox2D hitbox = hitboxObject.AddComponent<Hitbox2D>();
            hitbox.Initialize(
                owner: gameObject,
                ownerTeam: TeamId.Player,
                damage: skill.Damage,
                size: skill.HitboxSize,
                direction: aimDirection,
                lifetime: skill.HitboxDuration,
                targetLayerMask: skill.TargetLayerMask,
                hitOncePerTarget: true,
                canCritical: skill.CanCritical,
                sourceSkill: skill,
                gizmoColor: skill.GizmoColor,
                shape: skill.HitboxShape,
                angleDegrees: angleDegrees,
                radius: skill.HitboxRadius,
                innerRadius: skill.HitboxInnerRadius,
                arcAngle: skill.HitboxArcAngle,
                destroyOnFirstHit: false,
                sourceDisplayName: null,
                canKnockUp: skill.CanKnockUp,
                knockUpVelocity: skill.KnockUpVelocity,
                airborneDuration: skill.AirborneDuration);
        }

        private void SpawnProjectile(SkillData skill)
        {
            ProjectileData projectileData = skill.ProjectileData;

            if (projectileData == null)
            {
                Debug.LogWarning(
                    $"{skill.DisplayName} uses Projectile execution, but has no ProjectileData assigned.",
                    this);
                return;
            }

            Vector2 aimDirection = GetAimDirection();
            float angleDegrees = GetAimAngleDegrees(aimDirection, skill);
            Vector3 spawnPosition = CalculateProjectileSpawnPosition(projectileData, aimDirection);

            Debug.Log(
                $"Spawn Projectile: skill={skill.SkillId}, motion={projectileData.MotionType}, speed={projectileData.Speed}, lifetime={projectileData.Lifetime}, gravity={projectileData.Gravity}",
                this);

            GameObject projectileObject = projectileData.ProjectilePrefab != null
                ? Instantiate(projectileData.ProjectilePrefab, spawnPosition, Quaternion.identity)
                : new GameObject($"{skill.SkillId}_Projectile");
            projectileObject.transform.position = spawnPosition;

            ProjectileMover2D mover = projectileObject.GetComponent<ProjectileMover2D>();

            if (mover == null)
            {
                mover = projectileObject.AddComponent<ProjectileMover2D>();
            }

            mover.Initialize(
                motionType: projectileData.MotionType,
                direction: aimDirection,
                speed: projectileData.Speed,
                lifetime: projectileData.Lifetime,
                gravity: projectileData.Gravity,
                rotateToVelocity: projectileData.RotateToVelocity);

            ProjectileImpact2D impact = projectileObject.GetComponent<ProjectileImpact2D>();

            if (impact == null)
            {
                impact = projectileObject.AddComponent<ProjectileImpact2D>();
            }

            impact.Initialize(
                impactLayerMask: projectileData.ImpactLayerMask,
                checkRadius: projectileData.ImpactCheckRadius,
                destroyOnImpact: projectileData.DestroyOnImpact);

            Hitbox2D hitbox = projectileObject.GetComponent<Hitbox2D>();

            if (hitbox == null)
            {
                hitbox = projectileObject.AddComponent<Hitbox2D>();
            }

            hitbox.Initialize(
                owner: gameObject,
                ownerTeam: TeamId.Player,
                damage: skill.Damage,
                size: skill.HitboxSize,
                direction: aimDirection,
                lifetime: projectileData.Lifetime,
                targetLayerMask: skill.TargetLayerMask,
                hitOncePerTarget: true,
                canCritical: skill.CanCritical,
                sourceSkill: skill,
                gizmoColor: skill.GizmoColor,
                shape: skill.HitboxShape,
                angleDegrees: angleDegrees,
                radius: skill.HitboxRadius,
                innerRadius: skill.HitboxInnerRadius,
                arcAngle: skill.HitboxArcAngle,
                destroyOnFirstHit: projectileData.DestroyOnFirstHit,
                sourceDisplayName: null,
                canKnockUp: skill.CanKnockUp,
                knockUpVelocity: skill.KnockUpVelocity,
                airborneDuration: skill.AirborneDuration);
        }

        private void SpawnDashHitbox(SkillData skill, Vector2 aimDirection, float duration)
        {
            float angleDegrees = GetAimAngleDegrees(aimDirection, skill);
            Vector3 center = CalculateHitboxCenter(skill, aimDirection);

            GameObject hitboxObject = new GameObject($"{skill.SkillId}_DashHitbox");
            hitboxObject.transform.position = center;
            hitboxObject.transform.SetParent(transform, true);

            Hitbox2D hitbox = hitboxObject.AddComponent<Hitbox2D>();
            hitbox.Initialize(
                owner: gameObject,
                ownerTeam: TeamId.Player,
                damage: skill.Damage,
                size: skill.HitboxSize,
                direction: aimDirection,
                lifetime: Mathf.Max(0.01f, duration),
                targetLayerMask: skill.TargetLayerMask,
                hitOncePerTarget: true,
                canCritical: skill.CanCritical,
                sourceSkill: skill,
                gizmoColor: skill.GizmoColor,
                shape: skill.HitboxShape,
                angleDegrees: angleDegrees,
                radius: skill.HitboxRadius,
                innerRadius: skill.HitboxInnerRadius,
                arcAngle: skill.HitboxArcAngle,
                destroyOnFirstHit: false,
                sourceDisplayName: null,
                canKnockUp: skill.CanKnockUp,
                knockUpVelocity: skill.KnockUpVelocity,
                airborneDuration: skill.AirborneDuration);
        }

        private Vector2 ResolveDashDirection(DashData dashData)
        {
            Vector2 fallbackDirection = GetAimDirection();

            if (dashData == null ||
                !dashData.EnableAirborneHoming ||
                airborneTargetFinder == null)
            {
                return fallbackDirection;
            }

            AirborneTarget2D target = airborneTargetFinder.FindNearestAirborneTarget(
                transform.position,
                dashData.AirborneHomingSearchRadius,
                dashData.AirborneTargetLayerMask);

            if (target == null)
            {
                return fallbackDirection;
            }

            Vector2 toTarget = target.transform.position - transform.position;

            if (toTarget.magnitude <= dashData.HomingStopDistance ||
                toTarget.sqrMagnitude < 0.0001f)
            {
                return fallbackDirection;
            }

            return toTarget.normalized;
        }

        private Vector2 GetAimDirection()
        {
            if (aim != null)
            {
                return aim.AimDirection;
            }

            int facing = motor != null ? motor.FacingDirection : 1;
            return new Vector2(facing, 0f);
        }

        private float GetAimAngleDegrees(Vector2 aimDirection, SkillData skill)
        {
            return Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg
                + skill.HitboxRotationOffset;
        }

        private Vector3 CalculateHitboxCenter(SkillData skill, Vector2 aimDirection)
        {
            Vector2 origin = transform.position;

            if (skill.CastPositionMode == SkillCastPositionMode.AtAimPoint && aim != null)
            {
                Vector2 toMouse = aim.MouseWorldPosition - origin;

                if (toMouse.magnitude > skill.MaxCastRange)
                {
                    return origin + toMouse.normalized * skill.MaxCastRange;
                }

                return aim.MouseWorldPosition;
            }

            Vector2 forward = aimDirection.sqrMagnitude > 0.0001f
                ? aimDirection.normalized
                : Vector2.right;
            Vector2 right = new Vector2(-forward.y, forward.x);
            Vector2 offset = skill.HitboxOffset;
            Vector2 worldOffset = forward * offset.x + right * offset.y;

            return origin + worldOffset;
        }

        private Vector3 CalculateProjectileSpawnPosition(ProjectileData projectileData, Vector2 aimDirection)
        {
            Vector2 origin = transform.position;
            Vector2 forward = aimDirection.sqrMagnitude > 0.0001f
                ? aimDirection.normalized
                : Vector2.right;
            Vector2 right = new Vector2(-forward.y, forward.x);
            Vector2 offset = projectileData.SpawnOffset;
            Vector2 worldOffset = forward * offset.x + right * offset.y;

            return origin + worldOffset;
        }
    }
}
