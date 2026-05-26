using System.Collections;
using JingHongLu.Combat;
using JingHongLu.Skills;
using UnityEngine;

namespace JingHongLu.Enemies
{
    public sealed class EnemyBrain2D : MonoBehaviour
    {
        [SerializeField] private EnemyData data;
        [SerializeField] private Transform target;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private AirborneTarget2D airborneTarget;
        [SerializeField] private HitStunReceiver2D hitStunReceiver;
        [SerializeField] private EnemyKnockbackReceiver2D knockbackReceiver;
        [SerializeField] private PerfectDodgeSlowMotionController slowMotionController;
        [SerializeField] private PlayerSkillController targetSkillController;
        [SerializeField] private TeamId ownerTeam = TeamId.Enemy;
        [SerializeField] private bool logAttack = true;
        [SerializeField] private bool logRangedDecision;
        [SerializeField] private float rangedDecisionLogInterval = 0.5f;
        [Header("Battle Bounds")]
        [SerializeField] private bool useBattleBounds = true;
        [SerializeField] private float minX = -9f;
        [SerializeField] private float maxX = 9f;
        [SerializeField] private bool clampPositionWhenOutOfBounds = true;
        [SerializeField] private bool logBattleBounds;

        private bool isAttacking;
        private bool isRepositioning;
        private float cooldownTimer;
        private float rangedCooldownTimer;
        private float backstepCooldownTimer;
        private float reactionCooldownTimer;
        private int facingSign = 1;
        private bool warnedMissingTarget;
        private float nextRangedDecisionLogTime;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeAirborneEvents();
            SubscribeHitStunEvents();
            SubscribeKnockbackEvents();
            SubscribeTargetSkillEvents();
        }

        private void OnDisable()
        {
            UnsubscribeTargetSkillEvents();
            UnsubscribeKnockbackEvents();
            UnsubscribeHitStunEvents();
            UnsubscribeAirborneEvents();
            StopAllCoroutines();
            isAttacking = false;
            isRepositioning = false;

            if (body != null)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            }
        }

        private void Update()
        {
            TickCooldown();
            TickRangedCooldown();
            TickBackstepCooldown();
            TickReactionCooldown();

            if (airborneTarget != null && airborneTarget.IsAirborne)
            {
                LogRangedDecision("Reject: currently airborne.");
                return;
            }

            if (hitStunReceiver != null && hitStunReceiver.IsStunned)
            {
                LogRangedDecision("Reject: currently in hit stun.");
                StopHorizontalMovement();
                return;
            }

            if (knockbackReceiver != null && knockbackReceiver.IsKnockbacking)
            {
                LogRangedDecision("Reject: currently knockbacking.");
                return;
            }

            if (data == null)
            {
                StopHorizontalMovement();
                return;
            }

            if (target == null)
            {
                ResolveTarget();

                if (target == null)
                {
                    LogRangedDecision("Reject: target missing.");
                    StopHorizontalMovement();
                    LogMissingTargetWarning();
                    return;
                }

                ResolveTargetSkillController();
                SubscribeTargetSkillEvents();
            }

            if (isAttacking)
            {
                LogRangedDecision("Reject: currently attacking.");
                StopHorizontalMovement();
                return;
            }

            if (isRepositioning)
            {
                return;
            }

            float horizontalDistance = Mathf.Abs(target.position.x - transform.position.x);
            FaceTarget();
            LogRangedDecision(
                $"name={name}, distance={horizontalDistance:F2}, " +
                $"canUseRanged={data.CanUseRangedAttack}, " +
                $"cooldown={rangedCooldownTimer:F2}, " +
                $"min={data.RangedAttackMinDistance:F2}, " +
                $"max={data.RangedAttackMaxDistance:F2}, " +
                $"hasProjectileData={data.HarpoonProjectileData != null}, " +
                $"hasTarget={target != null}");

            if (horizontalDistance > data.LoseTargetRange)
            {
                LogRangedDecision("Reject: distance too far.");
                StopHorizontalMovement();
                return;
            }

            if (horizontalDistance <= data.AttackRange)
            {
                StopHorizontalMovement();
                TryAttack();
                return;
            }

            bool inRangedRange = data.CanUseRangedAttack
                && horizontalDistance >= data.RangedAttackMinDistance
                && horizontalDistance <= data.RangedAttackMaxDistance;

            if (data.CanUseRangedAttack && horizontalDistance < data.RangedAttackMinDistance)
            {
                LogRangedDecision("Reject: distance too close.");
            }
            else if (data.CanUseRangedAttack && horizontalDistance > data.RangedAttackMaxDistance)
            {
                LogRangedDecision("Reject: distance too far.");
            }
            else if (!data.CanUseRangedAttack)
            {
                LogRangedDecision("Reject: CanUseRangedAttack=false.");
            }

            if (inRangedRange)
            {
                StopHorizontalMovement();

                if (TryRangedAttack())
                {
                    return;
                }

                if (rangedCooldownTimer > 0f)
                {
                    LogRangedDecision("Reject: cooldown not ready.");
                    return;
                }
            }

            if (data.EnableCombatSpacing && horizontalDistance < data.PreferredMinDistance)
            {
                if (TryBackstep())
                {
                    return;
                }

                if (horizontalDistance > data.AttackRange)
                {
                    ChaseTarget();
                    return;
                }

                StopHorizontalMovement();
                return;
            }

            if (horizontalDistance <= data.AggroRange)
            {
                ChaseTarget();
                return;
            }

            StopHorizontalMovement();
        }

        private void LateUpdate()
        {
            ClampPositionToBattleBounds();
        }

        private void ResolveReferences()
        {
            if (body == null)
            {
                TryGetComponent(out body);
            }

            if (airborneTarget == null)
            {
                TryGetComponent(out airborneTarget);
            }

            if (airborneTarget == null)
            {
                airborneTarget = GetComponentInChildren<AirborneTarget2D>();
            }

            if (airborneTarget == null)
            {
                airborneTarget = GetComponentInParent<AirborneTarget2D>();
            }

            if (hitStunReceiver == null)
            {
                TryGetComponent(out hitStunReceiver);
            }

            if (hitStunReceiver == null)
            {
                hitStunReceiver = GetComponentInParent<HitStunReceiver2D>();
            }

            if (hitStunReceiver == null)
            {
                hitStunReceiver = GetComponentInChildren<HitStunReceiver2D>();
            }

            if (knockbackReceiver == null)
            {
                TryGetComponent(out knockbackReceiver);
            }

            if (knockbackReceiver == null)
            {
                knockbackReceiver = GetComponentInParent<EnemyKnockbackReceiver2D>();
            }

            if (knockbackReceiver == null)
            {
                knockbackReceiver = GetComponentInChildren<EnemyKnockbackReceiver2D>();
            }

            if (slowMotionController == null)
            {
                slowMotionController = PerfectDodgeSlowMotionController.Instance;
            }

            if (slowMotionController == null)
            {
                slowMotionController = FindAnyObjectByType<PerfectDodgeSlowMotionController>();
            }

            if (target == null)
            {
                ResolveTarget();
            }

            if (targetSkillController == null)
            {
                ResolveTargetSkillController();
            }
        }

        private void SubscribeAirborneEvents()
        {
            if (airborneTarget == null)
            {
                return;
            }

            airborneTarget.OnAirborneStarted -= HandleAirborneStarted;
            airborneTarget.OnAirborneEnded -= HandleAirborneEnded;
            airborneTarget.OnAirborneStarted += HandleAirborneStarted;
            airborneTarget.OnAirborneEnded += HandleAirborneEnded;
        }

        private void UnsubscribeAirborneEvents()
        {
            if (airborneTarget == null)
            {
                return;
            }

            airborneTarget.OnAirborneStarted -= HandleAirborneStarted;
            airborneTarget.OnAirborneEnded -= HandleAirborneEnded;
        }

        private void SubscribeHitStunEvents()
        {
            if (hitStunReceiver == null)
            {
                return;
            }

            hitStunReceiver.OnHitStunStarted -= HandleHitStunStarted;
            hitStunReceiver.OnHitStunEnded -= HandleHitStunEnded;
            hitStunReceiver.OnHitStunStarted += HandleHitStunStarted;
            hitStunReceiver.OnHitStunEnded += HandleHitStunEnded;
        }

        private void UnsubscribeHitStunEvents()
        {
            if (hitStunReceiver == null)
            {
                return;
            }

            hitStunReceiver.OnHitStunStarted -= HandleHitStunStarted;
            hitStunReceiver.OnHitStunEnded -= HandleHitStunEnded;
        }

        private void SubscribeKnockbackEvents()
        {
            if (knockbackReceiver == null)
            {
                return;
            }

            knockbackReceiver.OnKnockbackStarted -= HandleKnockbackStarted;
            knockbackReceiver.OnKnockbackEnded -= HandleKnockbackEnded;
            knockbackReceiver.OnKnockbackStarted += HandleKnockbackStarted;
            knockbackReceiver.OnKnockbackEnded += HandleKnockbackEnded;
        }

        private void UnsubscribeKnockbackEvents()
        {
            if (knockbackReceiver == null)
            {
                return;
            }

            knockbackReceiver.OnKnockbackStarted -= HandleKnockbackStarted;
            knockbackReceiver.OnKnockbackEnded -= HandleKnockbackEnded;
        }

        private void HandleAirborneStarted(AirborneTarget2D target)
        {
            InterruptCurrentAction("airborne");
        }

        private void HandleAirborneEnded(AirborneTarget2D target)
        {
            if (logAttack)
            {
                Debug.Log($"{name} airborne ended, AI resumed.", this);
            }
        }

        private void HandleHitStunStarted(float duration)
        {
            InterruptCurrentAction("hit stun");
            StopHorizontalMovement();
        }

        private void HandleHitStunEnded()
        {
            if (logAttack)
            {
                Debug.Log($"{name} hit stun ended, AI resumed.", this);
            }
        }

        private void HandleKnockbackStarted()
        {
            InterruptCurrentAction("knockback");
        }

        private void HandleKnockbackEnded()
        {
            if (logAttack)
            {
                Debug.Log($"{name} knockback ended, AI resumed.", this);
            }
        }

        private void InterruptCurrentAction(string reason)
        {
            StopAllCoroutines();
            isAttacking = false;
            isRepositioning = false;

            if (logAttack)
            {
                Debug.Log($"{name} action interrupted by {reason}.", this);
            }
        }

        private void HandleTargetSkillCastStarted(SkillData skill)
        {
            if (data == null || !data.ReactToDangerousPlayerSkill)
            {
                return;
            }

            if (skill == null || reactionCooldownTimer > 0f)
            {
                return;
            }

            if (isAttacking || isRepositioning)
            {
                return;
            }

            if (airborneTarget != null && airborneTarget.IsAirborne)
            {
                return;
            }

            bool isDangerous =
                skill.CanKnockUp ||
                skill.ExecutionType == SkillExecutionType.Dash;

            if (!isDangerous)
            {
                return;
            }

            if (Random.value > data.DangerousSkillReactionChance)
            {
                return;
            }

            reactionCooldownTimer = data.DangerousSkillReactionCooldown;
            TryBackstep();
        }

        private void ResolveTarget()
        {
            GameObject playerObject = null;

            try
            {
                playerObject = GameObject.FindGameObjectWithTag("Player");
            }
            catch (UnityException)
            {
                playerObject = null;
            }

            if (playerObject == null)
            {
                playerObject = GameObject.Find("Player");
            }

            if (playerObject != null)
            {
                target = playerObject.transform;
            }
        }

        private void ResolveTargetSkillController()
        {
            if (target == null)
            {
                return;
            }

            targetSkillController = target.GetComponent<PlayerSkillController>();
        }

        private void SubscribeTargetSkillEvents()
        {
            if (targetSkillController == null)
            {
                return;
            }

            targetSkillController.OnSkillCastStarted -=
                HandleTargetSkillCastStarted;
            targetSkillController.OnSkillCastStarted +=
                HandleTargetSkillCastStarted;
        }

        private void UnsubscribeTargetSkillEvents()
        {
            if (targetSkillController == null)
            {
                return;
            }

            targetSkillController.OnSkillCastStarted -=
                HandleTargetSkillCastStarted;
        }

        private void TickCooldown()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= EnemyDeltaTime;
            }
        }

        private void TickRangedCooldown()
        {
            if (rangedCooldownTimer > 0f)
            {
                rangedCooldownTimer -= EnemyDeltaTime;
            }
        }

        private void TickBackstepCooldown()
        {
            if (backstepCooldownTimer > 0f)
            {
                backstepCooldownTimer -= EnemyDeltaTime;
            }
        }

        private void TickReactionCooldown()
        {
            if (reactionCooldownTimer > 0f)
            {
                reactionCooldownTimer -= EnemyDeltaTime;
            }
        }

        private void ChaseTarget()
        {
            if (target == null)
            {
                StopHorizontalMovement();
                return;
            }

            float horizontalDelta = target.position.x - transform.position.x;
            float absHorizontalDelta = Mathf.Abs(horizontalDelta);
            float effectiveStopDistance = Mathf.Min(
                data.StopDistance,
                data.AttackRange * 0.8f);

            if (absHorizontalDelta <= effectiveStopDistance)
            {
                StopHorizontalMovement();
                return;
            }

            int directionSign = horizontalDelta >= 0f ? 1 : -1;
            facingSign = directionSign;
            SetVisualFacing(directionSign);
            SetHorizontalVelocity(directionSign * data.MoveSpeed * GetEnemyTimeScale());
        }

        private void TryAttack()
        {
            if (isAttacking || cooldownTimer > 0f)
            {
                return;
            }

            StartCoroutine(AttackRoutine());
        }

        private bool TryRangedAttack()
        {
            if (isAttacking || rangedCooldownTimer > 0f)
            {
                LogRangedDecision("Reject: cooldown not ready.");
                return false;
            }

            if (data.HarpoonProjectileData == null)
            {
                LogRangedDecision("Reject: projectile data missing.");
                return false;
            }

            StartCoroutine(RangedAttackRoutine());
            return true;
        }

        private IEnumerator AttackRoutine()
        {
            isAttacking = true;
            StopHorizontalMovement();
            FaceTarget();

            if (data.AttackWindup > 0f)
            {
                yield return WaitEnemyScaled(data.AttackWindup);
            }

            if (airborneTarget != null && airborneTarget.IsAirborne)
            {
                isAttacking = false;
                yield break;
            }

            FaceTarget();
            SpawnAttackHitbox();
            cooldownTimer = data.AttackCooldown;

            if (data.AttackRecovery > 0f)
            {
                yield return WaitEnemyScaled(data.AttackRecovery);
            }

            yield return PostAttackRhythmRoutine();
            isAttacking = false;
        }

        private IEnumerator RangedAttackRoutine()
        {
            isAttacking = true;
            StopHorizontalMovement();

            if (data.RangedAttackWindup > 0f)
            {
                yield return WaitEnemyScaled(data.RangedAttackWindup);
            }

            if (airborneTarget != null && airborneTarget.IsAirborne)
            {
                isAttacking = false;
                yield break;
            }

            SpawnHarpoonProjectile();
            rangedCooldownTimer = data.RangedAttackCooldown;

            if (data.AttackRecovery > 0f)
            {
                yield return WaitEnemyScaled(data.AttackRecovery);
            }

            yield return PostAttackRhythmRoutine();
            isAttacking = false;
        }

        private IEnumerator PostAttackRhythmRoutine()
        {
            isAttacking = false;

            if (data.PostAttackIdleTime > 0f)
            {
                isRepositioning = true;
                StopHorizontalMovement();
                yield return WaitEnemyScaled(data.PostAttackIdleTime);
                isRepositioning = false;
            }

            if (airborneTarget != null && airborneTarget.IsAirborne)
            {
                yield break;
            }

            if (data.EnableBackstep &&
                Random.value < data.BackstepChanceAfterAttack)
            {
                yield return BackstepRoutine();
            }
        }

        private bool TryBackstep()
        {
            if (data == null || !data.EnableBackstep)
            {
                return false;
            }

            if (backstepCooldownTimer > 0f)
            {
                return false;
            }

            if (isAttacking || isRepositioning)
            {
                return false;
            }

            if (airborneTarget != null && airborneTarget.IsAirborne)
            {
                return false;
            }

            StartCoroutine(BackstepRoutine());
            return true;
        }

        private IEnumerator BackstepRoutine()
        {
            isRepositioning = true;
            backstepCooldownTimer = data.BackstepCooldown;

            float timer = 0f;
            float direction = -facingSign;

            if (target != null)
            {
                float delta = transform.position.x - target.position.x;

                if (Mathf.Abs(delta) > 0.001f)
                {
                    direction = Mathf.Sign(delta);
                }
            }

            if (!CanBackstepWithinBattleBounds(direction))
            {
                LogBattleBounds(
                    $"Backstep canceled by battle bounds. Enemy={name}");
                StopHorizontalMovement();
                isRepositioning = false;
                yield break;
            }

            while (timer < data.BackstepDuration)
            {
                if (airborneTarget != null && airborneTarget.IsAirborne)
                {
                    break;
                }

                SetHorizontalVelocity(direction * data.BackstepSpeed * GetEnemyTimeScale());
                timer += EnemyDeltaTime;
                yield return null;
            }

            StopHorizontalMovement();
            isRepositioning = false;
        }

        private void SpawnAttackHitbox()
        {
            FaceTarget();
            Vector2 direction = new Vector2(facingSign, 0f);
            Vector2 offset = data.HitboxOffset;
            offset.x *= facingSign;

            GameObject hitboxObject = new GameObject("EnemyAttackHitbox");
            hitboxObject.transform.position = (Vector2)transform.position + offset;

            Hitbox2D hitbox = hitboxObject.AddComponent<Hitbox2D>();
            hitbox.Initialize(
                owner: gameObject,
                ownerTeam: ownerTeam,
                damage: data.AttackDamage,
                size: data.HitboxSize,
                direction: direction,
                lifetime: data.HitboxDuration,
                targetLayerMask: data.TargetLayerMask,
                hitOncePerTarget: true,
                canCritical: false,
                sourceSkill: null,
                gizmoColor: data.GizmoColor,
                shape: data.HitboxShape,
                angleDegrees: direction.x >= 0f ? 0f : 180f,
                radius: 1f,
                innerRadius: 0f,
                arcAngle: 90f,
                destroyOnFirstHit: false,
                sourceDisplayName: "敌人攻击",
                interruptType: AttackInterruptType.Heavy,
                canBePerfectDodged: true);

            if (logAttack)
            {
                Debug.Log($"[EnemyAI] {name} melee attack.", this);
            }
        }

        private void SpawnHarpoonProjectile()
        {
            ProjectileData projectileData = data.HarpoonProjectileData;

            if (projectileData == null)
            {
                return;
            }

            Vector2 aimDirection = GetDirectionToTarget();
            Vector3 spawnPosition = CalculateProjectileSpawnPosition(
                projectileData,
                aimDirection);

            GameObject projectileObject = projectileData.ProjectilePrefab != null
                ? Instantiate(projectileData.ProjectilePrefab, spawnPosition, Quaternion.identity)
                : new GameObject("WaterBandit_Harpoon");
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
                ownerTeam: ownerTeam);

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
                ownerTeam: ownerTeam,
                damage: data.RangedAttackDamage,
                size: data.HitboxSize,
                direction: aimDirection,
                lifetime: projectileData.Lifetime,
                targetLayerMask: data.TargetLayerMask,
                hitOncePerTarget: true,
                canCritical: false,
                sourceSkill: null,
                gizmoColor: data.GizmoColor,
                shape: data.HitboxShape,
                angleDegrees: GetAngleDegrees(aimDirection),
                radius: 1f,
                innerRadius: 0f,
                arcAngle: 90f,
                destroyOnFirstHit: projectileData.DestroyOnFirstHit,
                sourceDisplayName: "水匪鱼叉");

            if (logAttack)
            {
                Debug.Log($"[EnemyAI] {name} threw a harpoon.", this);
            }

            LogRangedDecision($"{name} threw a harpoon.", true);
        }

        private Vector2 GetDirectionToTarget()
        {
            if (target == null)
            {
                return new Vector2(facingSign, 0f);
            }

            Vector2 toTarget = target.position - transform.position;

            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return new Vector2(facingSign, 0f);
            }

            facingSign = toTarget.x >= 0f ? 1 : -1;
            SetVisualFacing(facingSign);

            return toTarget.normalized;
        }

        private Vector3 CalculateProjectileSpawnPosition(
            ProjectileData projectileData,
            Vector2 aimDirection)
        {
            Vector2 origin = transform.position;
            Vector2 forward = aimDirection.sqrMagnitude > 0.0001f
                ? aimDirection.normalized
                : new Vector2(facingSign, 0f);
            Vector2 right = new Vector2(-forward.y, forward.x);
            Vector2 offset = projectileData.SpawnOffset;
            Vector2 worldOffset = forward * offset.x + right * offset.y;

            return origin + worldOffset;
        }

        private static float GetAngleDegrees(Vector2 direction)
        {
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        private void SetHorizontalVelocity(float xVelocity)
        {
            xVelocity = ConstrainVelocityByBattleBounds(xVelocity);

            if (body == null)
            {
                transform.position += Vector3.right * (xVelocity * Time.deltaTime);
                return;
            }

            Vector2 velocity = body.linearVelocity;
            velocity.x = xVelocity;
            body.linearVelocity = velocity;
        }

        private float EnemyDeltaTime => Time.deltaTime * GetEnemyTimeScale();

        private float GetEnemyTimeScale()
        {
            if (slowMotionController == null)
            {
                slowMotionController = PerfectDodgeSlowMotionController.Instance;
            }

            if (slowMotionController == null)
            {
                slowMotionController = FindAnyObjectByType<PerfectDodgeSlowMotionController>();
            }

            return slowMotionController != null &&
                slowMotionController.IsPerfectDodgeSlowActive
                ? slowMotionController.EnemyTimeScale
                : 1f;
        }

        private IEnumerator WaitEnemyScaled(float duration)
        {
            float timer = 0f;

            while (timer < duration)
            {
                timer += EnemyDeltaTime;
                yield return null;
            }
        }

        private void StopHorizontalMovement()
        {
            SetHorizontalVelocity(0f);
        }

        private float ClampXToBattleBounds(float x)
        {
            return useBattleBounds ? Mathf.Clamp(x, minX, maxX) : x;
        }

        private float ConstrainVelocityByBattleBounds(float xVelocity)
        {
            if (!useBattleBounds || Mathf.Approximately(xVelocity, 0f))
            {
                return xVelocity;
            }

            float currentX = transform.position.x;
            bool movingLeftOut = xVelocity < 0f && currentX <= minX;
            bool movingRightOut = xVelocity > 0f && currentX >= maxX;

            if (!movingLeftOut && !movingRightOut)
            {
                return xVelocity;
            }

            LogBattleBounds($"Horizontal movement blocked by battle bounds. Enemy={name}");
            return 0f;
        }

        private bool CanBackstepWithinBattleBounds(float direction)
        {
            if (!useBattleBounds || data == null)
            {
                return true;
            }

            float targetX = transform.position.x +
                direction * data.BackstepSpeed * data.BackstepDuration;
            float clampedX = ClampXToBattleBounds(targetX);
            return Mathf.Approximately(targetX, clampedX);
        }

        private void ClampPositionToBattleBounds()
        {
            if (!useBattleBounds || !clampPositionWhenOutOfBounds)
            {
                return;
            }

            Vector3 position = transform.position;
            float clampedX = ClampXToBattleBounds(position.x);

            if (Mathf.Approximately(position.x, clampedX))
            {
                return;
            }

            transform.position = new Vector3(clampedX, position.y, position.z);

            if (body != null)
            {
                Vector2 velocity = body.linearVelocity;

                if ((position.x < minX && velocity.x < 0f) ||
                    (position.x > maxX && velocity.x > 0f))
                {
                    velocity.x = 0f;
                    body.linearVelocity = velocity;
                }
            }

            LogBattleBounds($"Position clamped by battle bounds. Enemy={name}");
        }

        private void LogBattleBounds(string message)
        {
            if (logBattleBounds)
            {
                Debug.Log($"[EnemyAI][Bounds] {message}", this);
            }
        }

        private void LogRangedDecision(string message, bool force = false)
        {
            if (!logRangedDecision)
            {
                return;
            }

            float now = Time.unscaledTime;

            if (!force && now < nextRangedDecisionLogTime)
            {
                return;
            }

            nextRangedDecisionLogTime = now + Mathf.Max(
                0.1f,
                rangedDecisionLogInterval);

            Debug.Log($"[EnemyAI][Ranged] {message}", this);
        }

        private void FaceTarget()
        {
            if (target == null)
            {
                return;
            }

            float horizontalDelta = target.position.x - transform.position.x;

            if (Mathf.Abs(horizontalDelta) <= 0.001f)
            {
                return;
            }

            int directionSign = horizontalDelta >= 0f ? 1 : -1;
            facingSign = directionSign;
            SetVisualFacing(directionSign);
        }

        private void SetVisualFacing(int directionSign)
        {
            Vector3 scale = transform.localScale;
            float absX = Mathf.Abs(scale.x);
            scale.x = directionSign >= 0 ? absX : -absX;
            transform.localScale = scale;
        }

        private void LogMissingTargetWarning()
        {
            if (warnedMissingTarget)
            {
                return;
            }

            warnedMissingTarget = true;
            Debug.LogWarning($"{nameof(EnemyBrain2D)} on {name} could not find a Player target.", this);
        }
    }
}
