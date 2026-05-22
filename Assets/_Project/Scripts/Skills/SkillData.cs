using JingHongLu.Combat;
using JingHongLu.Visuals;
using UnityEngine;

namespace JingHongLu.Skills
{
    [CreateAssetMenu(
        fileName = "SkillData",
        menuName = "JingHongLu/Skills/Skill Data")]
    public sealed class SkillData : ScriptableObject
    {
        [Header("Basic")]
        [SerializeField] private string skillId = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private StrokeType strokeType = StrokeType.None;
        [SerializeField] private Sprite icon = null;

        [Header("Execution")]
        [Tooltip("How this skill runs. InstantHitbox creates a static hitbox; Projectile creates a moving hitbox; Dash moves the caster.")]
        [SerializeField] private SkillExecutionType executionType = SkillExecutionType.InstantHitbox;

        [Header("Direction")]
        [SerializeField] private SkillDirectionMode directionMode = SkillDirectionMode.FacingHorizontal;

        [Header("Timing")]
        [SerializeField] private float cooldown = 0.25f;
        [SerializeField] private float castTime = 0.03f;
        [SerializeField] private float recoveryTime = 0.12f;

        [Header("Charge")]
        [SerializeField] private bool chargeUntilRelease;
        [SerializeField] private float minChargeTime = 0.15f;
        [SerializeField] private float maxChargeTime = 1.2f;
        [SerializeField] private bool lockMovementWhileCharging = true;
        [SerializeField] private bool blockOtherSkillsWhileCharging = true;
        [SerializeField] private bool superArmorWhileCharging = true;

        [Header("Super Armor")]
        [SerializeField] private bool superArmorDuringEntireSkill;

        [Header("Heavy Two Stage")]
        [SerializeField] private bool useHeavyTwoStage;
        [SerializeField] private float heavyStage1CastTime = 0.1f;
        [SerializeField] private float heavyStage1HitboxDuration = 0.2f;
        [SerializeField] private float heavyStage1RecoveryTime = 0f;
        [SerializeField] private float heavyStage1Damage = 10f;
        [SerializeField] private Vector2 heavyStage1HitboxSize = new Vector2(0.6f, 0.6f);
        [SerializeField] private bool heavyStage1CanApplyHitStun = true;
        [SerializeField] private float heavyStage1HitStunDuration = 0.1f;
        [SerializeField] private float heavyStage2MaxChargeTime = 1.5f;
        [SerializeField] private float heavyStage2HitboxDuration = 0.2f;
        [SerializeField] private float heavyStage2RecoveryTime = 0.2f;
        [SerializeField] private float heavyStage2BaseDamage = 15f;
        [SerializeField] private float heavyStage2DamageBonusPerSecond = 20f;
        [SerializeField] private float heavyStage2MaxDamageBonus = 30f;
        [SerializeField] private Vector2 heavyStage2HitboxSize = new Vector2(1f, 1f);
        [SerializeField] private bool heavyStage2CanApplyHitStun = true;
        [SerializeField] private float heavyStage2HitStunDuration = 0.2f;

        [Header("Damage")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private bool canCritical = true;
        [SerializeField] private bool canKnockUp = false;

        [Header("Hit Stun")]
        [SerializeField] private bool canApplyHitStun;
        [SerializeField] private float hitStunDuration;

        [Header("Knock Up")]
        [SerializeField] private Vector2 knockUpVelocity = new Vector2(2f, 7f);
        [SerializeField] private float airborneDuration = 1.2f;

        [Header("Hitbox")]
        [Tooltip("Used by Box / Capsule. Circle / Arc usually ignore this.")]
        [InspectorName("Size")]
        [SerializeField] private Vector2 hitboxSize = new Vector2(1.8f, 1f);

        [Tooltip("Local cast offset. X follows aim direction; Y is perpendicular to aim direction.")]
        [InspectorName("Offset")]
        [SerializeField] private Vector2 hitboxOffset = new Vector2(0.9f, 0f);

        [Tooltip("Hitbox lifetime in seconds.")]
        [InspectorName("Duration")]
        [SerializeField] private float hitboxDuration = 0.08f;

        [Tooltip("Layers this hitbox checks. Usually Hurtbox.")]
        [InspectorName("Target Layers")]
        [SerializeField] private LayerMask targetLayerMask = default;

        [Header("Cast Position")]
        [Tooltip("AroundCaster spawns near the caster. AtAimPoint spawns at mouse position.")]
        [InspectorName("Position Mode")]
        [SerializeField] private SkillCastPositionMode castPositionMode = SkillCastPositionMode.AroundCaster;

        [Tooltip("Maximum cast distance for AtAimPoint. Usually unused by AroundCaster.")]
        [InspectorName("Max Range")]
        [SerializeField] private float maxCastRange = 6f;

        [Header("Hitbox Shape")]
        [Tooltip("Hitbox shape: Box, Circle, Capsule, or Arc.")]
        [InspectorName("Shape")]
        [SerializeField] private HitboxShape hitboxShape = HitboxShape.Box;

        [Tooltip("Used by Circle / Arc as radius or outer radius.")]
        [InspectorName("Radius")]
        [SerializeField] private float hitboxRadius = 1.5f;

        [Tooltip("Used by Arc. Radius of the empty inner area. Set 0 for no inner gap.")]
        [InspectorName("Inner Radius")]
        [SerializeField] private float hitboxInnerRadius = 0f;

        [Tooltip("Used by Arc. Total arc angle in degrees, for example 110.")]
        [InspectorName("Arc Angle")]
        [SerializeField] private float hitboxArcAngle = 100f;

        [Tooltip("Extra rotation in degrees relative to aim direction. Useful for tuning hitboxes or matching effects.")]
        [InspectorName("Rotation Offset")]
        [SerializeField] private float hitboxRotationOffset = 0f;

        [Header("Projectile")]
        [Tooltip("Used by Projectile execution. Defines prefab, motion type, speed, lifetime, gravity, and hit destroy rules.")]
        [SerializeField] private ProjectileData projectileData = null;

        [Header("Dash")]
        [Tooltip("Used by Dash execution. Defines dash distance, duration, and invincibility.")]
        [SerializeField] private DashData dashData = null;

        [Header("Visual")]
        [SerializeField] private SkillVisualData visualData = null;

        [Header("Debug")]
        [SerializeField] private Color gizmoColor = new Color(1f, 0.25f, 0.1f, 0.35f);

        public string SkillId => skillId;
        public string DisplayName => displayName;
        public StrokeType StrokeType => strokeType;
        public Sprite Icon => icon;
        public SkillExecutionType ExecutionType => executionType;
        public SkillDirectionMode DirectionMode => directionMode;
        public float Cooldown => cooldown;
        public float CastTime => castTime;
        public float RecoveryTime => recoveryTime;
        public bool ChargeUntilRelease => chargeUntilRelease;
        public float MinChargeTime => minChargeTime;
        public float MaxChargeTime => maxChargeTime;
        public bool LockMovementWhileCharging => lockMovementWhileCharging;
        public bool BlockOtherSkillsWhileCharging => blockOtherSkillsWhileCharging;
        public bool SuperArmorWhileCharging => superArmorWhileCharging;
        public bool SuperArmorDuringEntireSkill => superArmorDuringEntireSkill;
        public bool UseHeavyTwoStage => useHeavyTwoStage;
        public float HeavyStage1CastTime => Mathf.Max(0f, heavyStage1CastTime);
        public float HeavyStage1HitboxDuration => Mathf.Max(0.01f, heavyStage1HitboxDuration);
        public float HeavyStage1RecoveryTime => Mathf.Max(0f, heavyStage1RecoveryTime);
        public float HeavyStage1Damage => heavyStage1Damage;
        public Vector2 HeavyStage1HitboxSize => heavyStage1HitboxSize;
        public bool HeavyStage1CanApplyHitStun => heavyStage1CanApplyHitStun;
        public float HeavyStage1HitStunDuration => Mathf.Max(0f, heavyStage1HitStunDuration);
        public float HeavyStage2MaxChargeTime => Mathf.Max(0f, heavyStage2MaxChargeTime);
        public float HeavyStage2HitboxDuration => Mathf.Max(0.01f, heavyStage2HitboxDuration);
        public float HeavyStage2RecoveryTime => Mathf.Max(0f, heavyStage2RecoveryTime);
        public float HeavyStage2BaseDamage => heavyStage2BaseDamage;
        public float HeavyStage2DamageBonusPerSecond => heavyStage2DamageBonusPerSecond;
        public float HeavyStage2MaxDamageBonus => Mathf.Max(0f, heavyStage2MaxDamageBonus);
        public Vector2 HeavyStage2HitboxSize => heavyStage2HitboxSize;
        public bool HeavyStage2CanApplyHitStun => heavyStage2CanApplyHitStun;
        public float HeavyStage2HitStunDuration => Mathf.Max(0f, heavyStage2HitStunDuration);
        public float Damage => damage;
        public bool CanCritical => canCritical;
        public bool CanKnockUp => canKnockUp;
        public bool CanApplyHitStun => canApplyHitStun;
        public float HitStunDuration => hitStunDuration;
        public Vector2 KnockUpVelocity => knockUpVelocity;
        public float AirborneDuration => airborneDuration;
        public Vector2 HitboxSize => hitboxSize;
        public Vector2 HitboxOffset => hitboxOffset;
        public float HitboxDuration => hitboxDuration;
        public LayerMask TargetLayerMask => targetLayerMask;
        public SkillCastPositionMode CastPositionMode => castPositionMode;
        public float MaxCastRange => maxCastRange;
        public HitboxShape HitboxShape => hitboxShape;
        public float HitboxRadius => hitboxRadius;
        public float HitboxInnerRadius => hitboxInnerRadius;
        public float HitboxArcAngle => hitboxArcAngle;
        public float HitboxRotationOffset => hitboxRotationOffset;
        public ProjectileData ProjectileData => projectileData;
        public DashData DashData => dashData;
        public SkillVisualData VisualData => visualData;
        public Color GizmoColor => gizmoColor;
    }
}
