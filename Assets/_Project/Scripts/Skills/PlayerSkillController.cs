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
        [SerializeField] private PlayerSuperArmorController superArmorController = null;
        [SerializeField] private PlayerControlLockController controlLock = null;
        [SerializeField] private PlayerSkillLoadout skillLoadout = null;
        [SerializeField] private bool logSkillInterruptDebug = true;

        private readonly Dictionary<SkillData, float> cooldownTimers = new Dictionary<SkillData, float>();
        private readonly List<SkillData> cooldownSkills = new List<SkillData>();
        private readonly object skillCastLockSource = new object();
        private readonly object chargeLockSource = new object();
        private readonly object fullSkillSuperArmorSource = new object();

        private bool isCasting;
        private bool isChargingSkill;
        private SkillData chargingSkill;
        private SkillSlot chargingSlot;
        private SkillData currentSkill;
        private Coroutine currentCastRoutine;
        private float chargeTimer;
        private bool chargeLockedMovement;
        private bool chargeAppliedSuperArmor;
        private bool isHeavyTwoStageCharging;
        private bool fullSkillSuperArmorApplied;
        private Vector2 lastSkillDirection = Vector2.right;

        public event Action<SkillData> OnSkillCastStarted;
        public event Action<SkillData> OnSkillExecuted;
        public event Action<SkillData> OnSkillCastFinished;
        public event Action<SkillData> OnSkillChargeStarted;
        public event Action<SkillData, float, float> OnSkillChargeUpdated;
        public event Action<SkillData, float> OnSkillChargeReleased;
        public event Action<SkillData> OnSkillChargeCanceled;
        public event Action<SkillData> OnSkillInterrupted;
        public event Action<SkillData, Vector2> OnSkillDirectionResolved;
        public event Action<SkillData, GameObject> OnProjectileSpawned;

        public bool IsCasting => isCasting;
        public bool IsChargingSkill => isChargingSkill;
        public SkillData CurrentSkill => currentSkill;
        public Vector2 LastSkillDirection => lastSkillDirection;

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

            if (superArmorController == null)
            {
                TryGetComponent(out superArmorController);
            }

            if (controlLock == null)
            {
                controlLock = GetComponentInParent<PlayerControlLockController>();
            }
        }

        private void Update()
        {
            TickCooldowns();
            ReadSkillSlotInput();
        }

        private void OnDisable()
        {
            CancelChargedSkill();
            CancelCurrentCastState(invokeInterruptedEvent: false);
            RemoveFullSkillSuperArmor();
            RemoveSkillCastLock();
            RemoveChargeLock();
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

            if (isChargingSkill)
            {
                if (!isHeavyTwoStageCharging)
                {
                    UpdateChargingSkill();
                }

                return;
            }

            TryHandleSlotInput(SkillSlot.Slot1);
            TryHandleSlotInput(SkillSlot.Slot2);
            TryHandleSlotInput(SkillSlot.Slot3);
            TryHandleSlotInput(SkillSlot.Slot4);
        }

        private void TryHandleSlotInput(SkillSlot slot)
        {
            if (!IsSlotPressed(slot))
            {
                return;
            }

            SkillData skill = GetSkill(slot);

            if (skill != null && skill.UseHeavyTwoStage)
            {
                TryBeginHeavyTwoStageSkill(slot, skill);
                return;
            }

            if (skill != null && skill.ChargeUntilRelease)
            {
                TryBeginChargedSkill(slot, skill);
                return;
            }

            TryCastSlot(slot);
        }

        private SkillData GetSkill(SkillSlot slot)
        {
            return skillLoadout != null ? skillLoadout.GetSkill(slot) : null;
        }

        private bool IsSlotPressed(SkillSlot slot)
        {
            return slot switch
            {
                SkillSlot.Slot1 => inputReader.SkillSlot1Pressed,
                SkillSlot.Slot2 => inputReader.SkillSlot2Pressed,
                SkillSlot.Slot3 => inputReader.SkillSlot3Pressed,
                SkillSlot.Slot4 => inputReader.SkillSlot4Pressed,
                _ => false
            };
        }

        private bool IsSlotReleased(SkillSlot slot)
        {
            if (isChargingSkill && IsRawSlotReleased(slot))
            {
                return true;
            }

            return slot switch
            {
                SkillSlot.Slot1 => inputReader.SkillSlot1Released,
                SkillSlot.Slot2 => inputReader.SkillSlot2Released,
                SkillSlot.Slot3 => inputReader.SkillSlot3Released,
                SkillSlot.Slot4 => inputReader.SkillSlot4Released,
                _ => false
            };
        }

        private bool IsRawSlotHeld(SkillSlot slot)
        {
            if (inputReader == null || inputReader.ActiveBindingProfile == null)
            {
                return false;
            }

            KeyCode key = inputReader.ActiveBindingProfile.GetPrimaryKey(GetInputAction(slot));
            return key != KeyCode.None && global::UnityEngine.Input.GetKey(key);
        }

        private bool IsRawSlotReleased(SkillSlot slot)
        {
            if (inputReader == null || inputReader.ActiveBindingProfile == null)
            {
                return false;
            }

            KeyCode key = inputReader.ActiveBindingProfile.GetPrimaryKey(GetInputAction(slot));
            return key != KeyCode.None && global::UnityEngine.Input.GetKeyUp(key);
        }

        private static GameplayInputAction GetInputAction(SkillSlot slot)
        {
            return slot switch
            {
                SkillSlot.Slot1 => GameplayInputAction.SkillSlot1,
                SkillSlot.Slot2 => GameplayInputAction.SkillSlot2,
                SkillSlot.Slot3 => GameplayInputAction.SkillSlot3,
                SkillSlot.Slot4 => GameplayInputAction.SkillSlot4,
                _ => GameplayInputAction.SkillSlot1
            };
        }

        private void TryCastSlot(SkillSlot slot)
        {
            if (skillLoadout == null)
            {
                return;
            }

            SkillData skill = GetSkill(slot);

            if (!CanCast(skill))
            {
                return;
            }

            currentCastRoutine = StartCoroutine(CastSkillRoutine(skill));
        }

        private bool CanCast(SkillData skill)
        {
            return skill != null
                && !isCasting
                && !isChargingSkill
                && !IsSkillControlLocked(skill)
                && !cooldownTimers.ContainsKey(skill);
        }

        private void TryBeginChargedSkill(SkillSlot slot, SkillData skill)
        {
            if (!CanCast(skill))
            {
                return;
            }

            CacheSkillDirection(skill);
            BeginChargeState(slot, skill, heavyTwoStage: false);
            OnSkillCastStarted?.Invoke(skill);
        }

        private void BeginChargeState(
            SkillSlot slot,
            SkillData skill,
            bool heavyTwoStage)
        {
            isChargingSkill = true;
            isHeavyTwoStageCharging = heavyTwoStage;
            chargingSkill = skill;
            currentSkill = skill;
            chargingSlot = slot;
            chargeTimer = 0f;
            chargeLockedMovement = skill.LockMovementWhileCharging && motor != null;
            chargeAppliedSuperArmor = skill.SuperArmorWhileCharging &&
                superArmorController != null;

            if (chargeLockedMovement)
            {
                motor.BeginExternalMotion();
                StopHorizontalMotion();
            }

            if (chargeAppliedSuperArmor)
            {
                superArmorController.SetSuperArmor(true);
            }

            AddChargeLock();
            OnSkillChargeStarted?.Invoke(skill);
        }

        private void TryBeginHeavyTwoStageSkill(SkillSlot slot, SkillData skill)
        {
            if (!CanCast(skill))
            {
                return;
            }

            currentCastRoutine = StartCoroutine(RunHeavyTwoStageRoutine(slot, skill));
        }

        private void UpdateChargingSkill()
        {
            if (chargingSkill == null)
            {
                CancelChargedSkill();
                return;
            }

            chargeTimer += Time.deltaTime;
            OnSkillChargeUpdated?.Invoke(
                chargingSkill,
                chargeTimer,
                CalculateNormalizedCharge(chargingSkill, chargeTimer));

            if (chargeLockedMovement)
            {
                StopHorizontalMotion();
            }

            if (IsSlotReleased(chargingSlot))
            {
                ReleaseChargedSkill();
            }
        }

        private void ReleaseChargedSkill()
        {
            SkillData skill = chargingSkill;
            float releasedChargeTime = chargeTimer;
            EndChargeState();

            if (skill == null)
            {
                return;
            }

            CacheSkillDirection(skill);
            OnSkillChargeReleased?.Invoke(skill, releasedChargeTime);
            currentCastRoutine = StartCoroutine(CastChargedSkillRoutine(skill));
        }

        private IEnumerator CastChargedSkillRoutine(SkillData skill)
        {
            isCasting = true;
            currentSkill = skill;
            AddSkillCastLock(PlayerControlLockFlags.Gameplay);
            ApplyFullSkillSuperArmor(skill);
            StartCooldown(skill);
            CacheSkillDirection(skill);
            OnSkillExecuted?.Invoke(skill);
            yield return ExecuteSkillRoutine(skill);

            AddSkillCastLock(GetSkillRecoveryLockFlags());

            if (skill.RecoveryTime > 0f)
            {
                yield return new WaitForSeconds(skill.RecoveryTime);
            }

            RemoveSkillCastLock();
            RemoveFullSkillSuperArmor();
            isCasting = false;
            currentSkill = null;
            currentCastRoutine = null;
            OnSkillCastFinished?.Invoke(skill);
        }

        private void CancelChargedSkill()
        {
            if (!isChargingSkill)
            {
                return;
            }

            SkillData skill = chargingSkill;
            EndChargeState();
            OnSkillChargeCanceled?.Invoke(skill);
        }

        private void EndChargeState()
        {
            RemoveChargeLock();

            if (chargeAppliedSuperArmor && superArmorController != null)
            {
                superArmorController.SetSuperArmor(false);
            }

            if (chargeLockedMovement && motor != null)
            {
                motor.EndExternalMotion();
            }

            isChargingSkill = false;
            isHeavyTwoStageCharging = false;
            chargingSkill = null;
            currentSkill = null;
            chargeTimer = 0f;
            chargeLockedMovement = false;
            chargeAppliedSuperArmor = false;
        }

        public void CancelCurrentSkillByInterrupt()
        {
            if (isChargingSkill)
            {
                if (logSkillInterruptDebug)
                {
                    Debug.Log(
                        $"[PlayerSkill] Charge interrupted. Skill={currentSkill?.DisplayName ?? chargingSkill?.DisplayName ?? "None"}",
                        this);
                }

                CancelChargedSkill();

                if (currentCastRoutine != null)
                {
                    CancelCurrentCastState(invokeInterruptedEvent: true);
                }

                return;
            }

            if (isCasting)
            {
                if (logSkillInterruptDebug)
                {
                    Debug.Log(
                        $"[PlayerSkill] Cast interrupted. Skill={currentSkill?.DisplayName ?? "None"}",
                        this);
                }

                CancelCurrentCastState(invokeInterruptedEvent: true);
                return;
            }

            if (logSkillInterruptDebug)
            {
                Debug.Log("[PlayerSkill] Interrupt requested, but no active skill.", this);
            }
        }

        private void CancelCurrentCastState(bool invokeInterruptedEvent)
        {
            if (currentCastRoutine != null)
            {
                StopCoroutine(currentCastRoutine);
                currentCastRoutine = null;
            }

            SkillData interruptedSkill = currentSkill;
            bool wasCasting = isCasting;

            isCasting = false;
            currentSkill = null;
            RemoveFullSkillSuperArmor();
            RemoveSkillCastLock();

            if (invokeInterruptedEvent && wasCasting && interruptedSkill != null)
            {
                OnSkillInterrupted?.Invoke(interruptedSkill);
            }
        }

        private static float CalculateNormalizedCharge(SkillData skill, float currentChargeTime)
        {
            if (skill == null || skill.MaxChargeTime <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01(currentChargeTime / skill.MaxChargeTime);
        }

        private void StopHorizontalMotion()
        {
            if (motor == null)
            {
                return;
            }

            Rigidbody2D body = GetComponent<Rigidbody2D>();

            if (body == null)
            {
                return;
            }

            Vector2 velocity = body.linearVelocity;
            velocity.x = 0f;
            motor.SetExternalVelocity(velocity);
        }

        private IEnumerator CastSkillRoutine(SkillData skill)
        {
            isCasting = true;
            currentSkill = skill;
            AddSkillCastLock(PlayerControlLockFlags.Gameplay);
            ApplyFullSkillSuperArmor(skill);
            StartCooldown(skill);
            CacheSkillDirection(skill);
            OnSkillCastStarted?.Invoke(skill);

            if (skill.CastTime > 0f)
            {
                yield return new WaitForSeconds(skill.CastTime);
            }

            CacheSkillDirection(skill);
            OnSkillExecuted?.Invoke(skill);
            yield return ExecuteSkillRoutine(skill);

            AddSkillCastLock(GetSkillRecoveryLockFlags());

            if (skill.RecoveryTime > 0f)
            {
                yield return new WaitForSeconds(skill.RecoveryTime);
            }

            RemoveSkillCastLock();
            RemoveFullSkillSuperArmor();
            isCasting = false;
            currentSkill = null;
            currentCastRoutine = null;
            OnSkillCastFinished?.Invoke(skill);
        }

        private IEnumerator RunHeavyTwoStageRoutine(SkillSlot slot, SkillData skill)
        {
            isCasting = true;
            currentSkill = skill;
            AddSkillCastLock(PlayerControlLockFlags.Gameplay);
            ApplyFullSkillSuperArmor(skill);
            StartCooldown(skill);
            CacheSkillDirection(skill);
            OnSkillCastStarted?.Invoke(skill);

            if (skill.HeavyStage1CastTime > 0f)
            {
                yield return new WaitForSeconds(skill.HeavyStage1CastTime);
            }

            SpawnInstantHitbox(
                skill,
                skill.HeavyStage1Damage,
                skill.HeavyStage1HitboxSize,
                skill.HeavyStage1HitboxDuration,
                skill.HeavyStage1CanApplyHitStun,
                skill.HeavyStage1HitStunDuration,
                skill.HeavyStage1ShieldDamage,
                skill.HeavyStage1ToughnessDamage,
                overrideKnockback: true,
                canApplyKnockback: skill.HeavyStage1CanApplyKnockback,
                knockbackDistance: skill.HeavyStage1KnockbackDistance,
                knockbackDuration: skill.HeavyStage1KnockbackDuration);

            if (skill.HeavyStage1HitboxDuration > 0f)
            {
                yield return new WaitForSeconds(skill.HeavyStage1HitboxDuration);
            }

            if (skill.HeavyStage1RecoveryTime > 0f)
            {
                yield return new WaitForSeconds(skill.HeavyStage1RecoveryTime);
            }

            CacheSkillDirection(skill);
            BeginChargeState(slot, skill, heavyTwoStage: true);

            while (isChargingSkill && chargingSkill == skill)
            {
                bool shouldRelease = !IsRawSlotHeld(slot);
                float maxChargeTime = skill.HeavyStage2MaxChargeTime;

                if (!shouldRelease)
                {
                    chargeTimer += Time.deltaTime;
                    float normalizedCharge = maxChargeTime <= 0f
                        ? 1f
                        : Mathf.Clamp01(chargeTimer / maxChargeTime);
                    OnSkillChargeUpdated?.Invoke(skill, chargeTimer, normalizedCharge);

                    if (chargeLockedMovement)
                    {
                        StopHorizontalMotion();
                    }

                    shouldRelease = IsRawSlotReleased(slot) ||
                        (maxChargeTime > 0f && chargeTimer >= maxChargeTime);
                }

                if (shouldRelease)
                {
                    break;
                }

                yield return null;
            }

            if (!isChargingSkill || chargingSkill != skill)
            {
                yield break;
            }

            float releasedChargeTime = chargeTimer;
            float damageBonus = Mathf.Min(
                releasedChargeTime * skill.HeavyStage2DamageBonusPerSecond,
                skill.HeavyStage2MaxDamageBonus);
            float finalDamage = skill.HeavyStage2BaseDamage + damageBonus;
            float shieldDamageBonus = Mathf.Min(
                releasedChargeTime * skill.HeavyStage2ShieldDamageBonusPerSecond,
                skill.HeavyStage2MaxShieldDamageBonus);
            float finalShieldDamage =
                skill.HeavyStage2BaseShieldDamage + shieldDamageBonus;
            EndChargeState();

            currentSkill = skill;
            CacheSkillDirection(skill);
            OnSkillChargeReleased?.Invoke(skill, releasedChargeTime);

            SpawnInstantHitbox(
                skill,
                finalDamage,
                skill.HeavyStage2HitboxSize,
                skill.HeavyStage2HitboxDuration,
                skill.HeavyStage2CanApplyHitStun,
                skill.HeavyStage2HitStunDuration,
                finalShieldDamage,
                skill.HeavyStage2ToughnessDamage,
                overrideKnockback: true,
                canApplyKnockback: skill.HeavyStage2CanApplyKnockback,
                knockbackDistance: skill.HeavyStage2KnockbackDistance,
                knockbackDuration: skill.HeavyStage2KnockbackDuration);
            OnSkillExecuted?.Invoke(skill);

            if (skill.HeavyStage2HitboxDuration > 0f)
            {
                yield return new WaitForSeconds(skill.HeavyStage2HitboxDuration);
            }

            AddSkillCastLock(GetSkillRecoveryLockFlags());

            if (skill.HeavyStage2RecoveryTime > 0f)
            {
                yield return new WaitForSeconds(skill.HeavyStage2RecoveryTime);
            }

            RemoveSkillCastLock();
            RemoveFullSkillSuperArmor();
            isCasting = false;
            currentSkill = null;
            currentCastRoutine = null;
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

        private void ApplyFullSkillSuperArmor(SkillData skill)
        {
            if (skill == null ||
                !skill.SuperArmorDuringEntireSkill ||
                superArmorController == null ||
                fullSkillSuperArmorApplied)
            {
                return;
            }

            fullSkillSuperArmorApplied = true;
            superArmorController.AddSuperArmor(fullSkillSuperArmorSource);
        }

        private void RemoveFullSkillSuperArmor()
        {
            if (!fullSkillSuperArmorApplied)
            {
                return;
            }

            fullSkillSuperArmorApplied = false;

            if (superArmorController != null)
            {
                superArmorController.RemoveSuperArmor(fullSkillSuperArmorSource);
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

            Vector2 aimDirection = ResolveDashDirection(skill, skill.DashData);
            SpawnDashHitbox(skill, aimDirection, skill.DashData.Duration);

            yield return dashController.DashRoutine(skill.DashData, aimDirection);
        }

        private void SpawnInstantHitbox(SkillData skill)
        {
            SpawnInstantHitbox(
                skill,
                skill.Damage,
                skill.HitboxSize,
                skill.HitboxDuration,
                skill.CanApplyHitStun,
                skill.HitStunDuration,
                skill.ShieldDamage,
                skill.ToughnessDamage);
        }

        private void SpawnInstantHitbox(
            SkillData skill,
            float damage,
            Vector2 hitboxSize,
            float hitboxDuration,
            bool canApplyHitStun,
            float hitStunDuration,
            float shieldDamage,
            float toughnessDamage,
            bool overrideKnockback = false,
            bool canApplyKnockback = false,
            float knockbackDistance = 0f,
            float knockbackDuration = 0f)
        {
            Vector2 aimDirection = ResolveSkillDirection(skill);
            float angleDegrees = GetAimAngleDegrees(aimDirection, skill);
            Vector3 center = CalculateHitboxCenter(skill, aimDirection);

            GameObject hitboxObject = new GameObject($"{skill.SkillId}_Hitbox");
            hitboxObject.transform.position = center;

            Hitbox2D hitbox = hitboxObject.AddComponent<Hitbox2D>();
            hitbox.Initialize(
                owner: gameObject,
                ownerTeam: TeamId.Player,
                damage: damage,
                size: hitboxSize,
                direction: aimDirection,
                lifetime: hitboxDuration,
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
                airborneDuration: skill.AirborneDuration,
                canApplyHitStun: canApplyHitStun,
                hitStunDuration: hitStunDuration,
                shieldDamage: shieldDamage,
                toughnessDamage: toughnessDamage,
                overrideKnockback: overrideKnockback,
                canApplyKnockback: canApplyKnockback,
                knockbackDistance: knockbackDistance,
                knockbackDuration: knockbackDuration);
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

            Vector2 aimDirection = ResolveSkillDirection(skill);
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
                rotateToVelocity: projectileData.RotateToVelocity,
                ownerTeam: TeamId.Player);

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
                airborneDuration: skill.AirborneDuration,
                canApplyHitStun: skill.CanApplyHitStun,
                hitStunDuration: skill.HitStunDuration,
                shieldDamage: skill.ShieldDamage,
                toughnessDamage: skill.ToughnessDamage);

            OnProjectileSpawned?.Invoke(skill, projectileObject);
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
                airborneDuration: skill.AirborneDuration,
                canApplyHitStun: skill.CanApplyHitStun,
                hitStunDuration: skill.HitStunDuration,
                shieldDamage: skill.ShieldDamage,
                toughnessDamage: skill.ToughnessDamage);
        }

        private Vector2 ResolveDashDirection(SkillData skill, DashData dashData)
        {
            Vector2 fallbackDirection = ResolveSkillDirection(skill);

            if (skill == null ||
                skill.DirectionMode != SkillDirectionMode.AimDirection2D ||
                dashData == null ||
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

            return SetLastSkillDirection(skill, toTarget.normalized);
        }

        private bool IsSkillControlLocked(SkillData skill)
        {
            if (controlLock == null)
            {
                return false;
            }

            if (controlLock.IsBasicSkillLocked)
            {
                return true;
            }

            return skill != null &&
                skill.ExecutionType == SkillExecutionType.Dash &&
                controlLock.IsDashLocked;
        }

        private void AddSkillCastLock(PlayerControlLockFlags flags)
        {
            if (controlLock != null)
            {
                controlLock.AddLock(skillCastLockSource, flags);
            }
        }

        private void RemoveSkillCastLock()
        {
            if (controlLock != null)
            {
                controlLock.RemoveLock(skillCastLockSource);
            }
        }

        private void AddChargeLock()
        {
            if (controlLock != null)
            {
                controlLock.AddLock(chargeLockSource, PlayerControlLockFlags.Gameplay);
            }
        }

        private void RemoveChargeLock()
        {
            if (controlLock != null)
            {
                controlLock.RemoveLock(chargeLockSource);
            }
        }

        private static PlayerControlLockFlags GetSkillRecoveryLockFlags()
        {
            return PlayerControlLockFlags.Move |
                PlayerControlLockFlags.Jump |
                PlayerControlLockFlags.BasicSkill |
                PlayerControlLockFlags.Dash;
        }

        private Vector2 ResolveSkillDirection(SkillData skill)
        {
            Vector2 direction = skill != null
                ? skill.DirectionMode switch
                {
                    SkillDirectionMode.MouseHorizontal => GetMouseHorizontalDirection(),
                    SkillDirectionMode.AimDirection2D => GetAimDirection2D(),
                    _ => GetFacingHorizontalDirection()
                }
                : GetFacingHorizontalDirection();

            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = GetFacingHorizontalDirection();
            }

            return SetLastSkillDirection(skill, direction);
        }

        private Vector2 CacheSkillDirection(SkillData skill)
        {
            return ResolveSkillDirection(skill);
        }

        private Vector2 SetLastSkillDirection(SkillData skill, Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = GetFacingHorizontalDirection();
            }

            direction = direction.normalized;
            lastSkillDirection = direction;
            OnSkillDirectionResolved?.Invoke(skill, direction);
            return direction;
        }

        private Vector2 GetFacingHorizontalDirection()
        {
            int facing = motor != null ? motor.FacingDirection : 1;

            if (facing == 0)
            {
                facing = transform.localScale.x >= 0f ? 1 : -1;
            }

            return facing >= 0 ? Vector2.right : Vector2.left;
        }

        private Vector2 GetMouseHorizontalDirection()
        {
            if (aim == null)
            {
                return GetFacingHorizontalDirection();
            }

            return aim.MouseWorldPosition.x >= transform.position.x
                ? Vector2.right
                : Vector2.left;
        }

        private Vector2 GetAimDirection2D()
        {
            if (aim == null || aim.AimDirection.sqrMagnitude < 0.0001f)
            {
                return GetFacingHorizontalDirection();
            }

            return aim.AimDirection.normalized;
        }

        private Vector2 GetAimDirection()
        {
            return GetAimDirection2D();
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
