using System;
using JingHongLu.Combat;
using JingHongLu.Skills;
using UnityEngine;

namespace JingHongLu.Player
{
    public sealed class PlayerInterruptReceiver : MonoBehaviour
    {
        [SerializeField] private PlayerSkillController skillController;
        [SerializeField] private PlayerDashController2D dashController;
        [SerializeField] private PlayerSuperArmorController superArmorController;
        [SerializeField] private PlayerControlLockController controlLock;
        [SerializeField] private Damageable damageable;
        [SerializeField] private float interruptedStunDuration = 0.25f;
        [SerializeField] private bool logInterrupt = false;
        [SerializeField] private bool logInterruptDebug = true;
        [SerializeField] private bool logInterruptRejectReason = true;
        [SerializeField] private float debugOverrideInterruptedStunDuration = -1f;

        private readonly object interruptLockSource = new object();

        private bool isInterrupted;
        private float interruptTimer;

        public event Action<DamageInfo> OnPlayerInterrupted;
        public event Action OnPlayerInterruptEnded;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (damageable != null)
            {
                damageable.OnDamageTaken += HandleDamageTaken;
            }
        }

        private void OnDisable()
        {
            if (damageable != null)
            {
                damageable.OnDamageTaken -= HandleDamageTaken;
            }

            if (isInterrupted && logInterruptDebug)
            {
                Debug.Log("[PlayerInterrupt] Cleared interrupt state on disable.", this);
            }

            ClearInterruptState(invokeEndedEvent: false);
        }

        private void Update()
        {
            if (!isInterrupted)
            {
                return;
            }

            interruptTimer -= Time.deltaTime;

            if (interruptTimer > 0f)
            {
                return;
            }

            ClearInterruptState(invokeEndedEvent: true);
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

            if (superArmorController == null)
            {
                TryGetComponent(out superArmorController);
            }

            if (controlLock == null)
            {
                TryGetComponent(out controlLock);
            }

            if (controlLock == null)
            {
                controlLock = GetComponentInParent<PlayerControlLockController>();
            }

            if (damageable == null)
            {
                TryGetComponent(out damageable);
            }

            if (damageable == null)
            {
                damageable = GetComponentInParent<Damageable>();
            }
        }

        private void HandleDamageTaken(DamageInfo damageInfo)
        {
            if (damageInfo.InterruptType != AttackInterruptType.Heavy)
            {
                LogReject(
                    $"[PlayerInterrupt] Ignore: interrupt type is {damageInfo.InterruptType}, not Heavy.");
                return;
            }

            if (superArmorController != null && superArmorController.HasSuperArmor)
            {
                LogReject("[PlayerInterrupt] Ignore: player has super armor.");
                return;
            }

            if (dashController != null && dashController.IsDashing)
            {
                LogReject("[PlayerInterrupt] Ignore: player is dashing.");
                return;
            }

            if (skillController == null ||
                (!skillController.IsCasting && !skillController.IsChargingSkill))
            {
                LogReject("[PlayerInterrupt] Ignore: player is not casting or charging.");
                return;
            }

            float duration = ResolveInterruptDuration();
            string skillName = skillController.CurrentSkill != null
                ? skillController.CurrentSkill.DisplayName
                : "None";

            if (logInterruptDebug)
            {
                Debug.Log(
                    $"[PlayerInterrupt] Interrupt success. Skill={skillName}, Duration={duration:0.###}",
                    this);
            }

            skillController.CancelCurrentSkillByInterrupt();
            EnterInterruptState(damageInfo, duration);
        }

        private void EnterInterruptState(DamageInfo damageInfo, float duration)
        {
            isInterrupted = true;
            interruptTimer = Mathf.Max(0f, duration);

            if (controlLock != null)
            {
                controlLock.AddLock(interruptLockSource, PlayerControlLockFlags.All);
            }

            OnPlayerInterrupted?.Invoke(damageInfo);

            if (logInterrupt || logInterruptDebug)
            {
                Debug.Log(
                    $"[PlayerInterrupt] Player interrupt stun started. Duration={interruptTimer:0.###}",
                    this);
            }

            if (interruptTimer <= 0f)
            {
                ClearInterruptState(invokeEndedEvent: true);
            }
        }

        private void ClearInterruptState(bool invokeEndedEvent)
        {
            bool wasInterrupted = isInterrupted;
            isInterrupted = false;
            interruptTimer = 0f;

            if (controlLock != null)
            {
                controlLock.RemoveLock(interruptLockSource);
            }

            if (invokeEndedEvent && wasInterrupted)
            {
                if (logInterruptDebug)
                {
                    Debug.Log("[PlayerInterrupt] Player interrupt stun ended.", this);
                }

                OnPlayerInterruptEnded?.Invoke();
            }
        }

        private float ResolveInterruptDuration()
        {
            return debugOverrideInterruptedStunDuration > 0f
                ? debugOverrideInterruptedStunDuration
                : interruptedStunDuration;
        }

        private void LogReject(string message)
        {
            if (logInterruptRejectReason)
            {
                Debug.Log(message, this);
            }
        }
    }
}
