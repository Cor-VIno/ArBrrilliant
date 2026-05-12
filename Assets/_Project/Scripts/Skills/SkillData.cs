using JingHongLu.Combat;
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

        [Header("Timing")]
        [SerializeField] private float cooldown = 0.25f;
        [SerializeField] private float castTime = 0.03f;
        [SerializeField] private float recoveryTime = 0.12f;

        [Header("Damage")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private bool canCritical = true;
        [SerializeField] private bool canKnockUp = false;

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

        [Header("Debug")]
        [SerializeField] private Color gizmoColor = new Color(1f, 0.25f, 0.1f, 0.35f);

        public string SkillId => skillId;
        public string DisplayName => displayName;
        public StrokeType StrokeType => strokeType;
        public Sprite Icon => icon;
        public SkillExecutionType ExecutionType => executionType;
        public float Cooldown => cooldown;
        public float CastTime => castTime;
        public float RecoveryTime => recoveryTime;
        public float Damage => damage;
        public bool CanCritical => canCritical;
        public bool CanKnockUp => canKnockUp;
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
        public Color GizmoColor => gizmoColor;
    }
}
