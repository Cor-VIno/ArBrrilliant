using JingHongLu.Combat;
using UnityEngine;

namespace JingHongLu.Enemies
{
    [CreateAssetMenu(
        fileName = "EnemyData",
        menuName = "JingHongLu/Enemies/Enemy Data")]
    public sealed class EnemyData : ScriptableObject
    {
        [Header("Detection")]
        [SerializeField] private float aggroRange = 8f;
        [SerializeField] private float loseTargetRange = 12f;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float stopDistance = 1.2f;

        [Header("Attack")]
        [SerializeField] private int attackDamage = 8;
        [SerializeField] private float attackRange = 1.4f;
        [SerializeField] private float attackCooldown = 1.2f;
        [SerializeField] private float attackWindup = 0.25f;
        [SerializeField] private float attackRecovery = 0.35f;

        [Header("Ranged Attack")]
        [SerializeField] private bool canUseRangedAttack = true;
        [SerializeField] private int rangedAttackDamage = 10;
        [SerializeField] private float rangedAttackMinDistance = 3f;
        [SerializeField] private float rangedAttackMaxDistance = 8f;
        [SerializeField] private float rangedAttackCooldown = 2.2f;
        [SerializeField] private float rangedAttackWindup = 0.35f;
        [SerializeField] private ProjectileData harpoonProjectileData;

        [Header("Combat Spacing")]
        [SerializeField] private bool enableCombatSpacing = true;
        [SerializeField] private float preferredMinDistance = 1.8f;
        [SerializeField] private float preferredMaxDistance = 4.5f;
        [SerializeField] private float postAttackIdleTime = 0.25f;

        [Header("Backstep")]
        [SerializeField] private bool enableBackstep = true;
        [SerializeField] private float backstepSpeed = 4f;
        [SerializeField] private float backstepDuration = 0.25f;
        [SerializeField] private float backstepCooldown = 1.2f;
        [SerializeField]
        [Range(0f, 1f)]
        private float backstepChanceAfterAttack = 0.35f;

        [Header("Player Cast Reaction")]
        [SerializeField] private bool reactToDangerousPlayerSkill = true;
        [SerializeField]
        [Range(0f, 1f)]
        private float dangerousSkillReactionChance = 0.5f;
        [SerializeField] private float dangerousSkillReactionCooldown = 1.5f;

        [Header("Attack Hitbox")]
        [SerializeField] private HitboxShape hitboxShape = HitboxShape.Box;
        [SerializeField] private Vector2 hitboxSize = new Vector2(1.2f, 0.8f);
        [SerializeField] private Vector2 hitboxOffset = new Vector2(0.8f, 0f);
        [SerializeField] private float hitboxDuration = 0.12f;
        [SerializeField] private LayerMask targetLayerMask;
        [SerializeField] private Color gizmoColor = Color.red;

        public float AggroRange => aggroRange;
        public float LoseTargetRange => loseTargetRange;
        public float MoveSpeed => moveSpeed;
        public float StopDistance => stopDistance;
        public int AttackDamage => attackDamage;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public float AttackWindup => attackWindup;
        public float AttackRecovery => attackRecovery;
        public bool CanUseRangedAttack => canUseRangedAttack;
        public int RangedAttackDamage => rangedAttackDamage;
        public float RangedAttackMinDistance => rangedAttackMinDistance;
        public float RangedAttackMaxDistance => rangedAttackMaxDistance;
        public float RangedAttackCooldown => rangedAttackCooldown;
        public float RangedAttackWindup => rangedAttackWindup;
        public ProjectileData HarpoonProjectileData => harpoonProjectileData;
        public bool EnableCombatSpacing => enableCombatSpacing;
        public float PreferredMinDistance => preferredMinDistance;
        public float PreferredMaxDistance => preferredMaxDistance;
        public float PostAttackIdleTime => postAttackIdleTime;
        public bool EnableBackstep => enableBackstep;
        public float BackstepSpeed => backstepSpeed;
        public float BackstepDuration => backstepDuration;
        public float BackstepCooldown => backstepCooldown;
        public float BackstepChanceAfterAttack => backstepChanceAfterAttack;
        public bool ReactToDangerousPlayerSkill => reactToDangerousPlayerSkill;
        public float DangerousSkillReactionChance => dangerousSkillReactionChance;
        public float DangerousSkillReactionCooldown => dangerousSkillReactionCooldown;
        public HitboxShape HitboxShape => hitboxShape;
        public Vector2 HitboxSize => hitboxSize;
        public Vector2 HitboxOffset => hitboxOffset;
        public float HitboxDuration => hitboxDuration;
        public LayerMask TargetLayerMask => targetLayerMask;
        public Color GizmoColor => gizmoColor;
    }
}
