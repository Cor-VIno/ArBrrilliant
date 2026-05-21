using System;
using System.Collections.Generic;
using UnityEngine;

namespace JingHongLu.Player
{
    public sealed class PlayerControlLockController : MonoBehaviour
    {
        [SerializeField] private bool logLocks;

        private readonly Dictionary<object, PlayerControlLockFlags> locks =
            new Dictionary<object, PlayerControlLockFlags>();

        public PlayerControlLockFlags CurrentLocks { get; private set; }

        public bool IsMoveLocked => CurrentLocks.HasFlag(PlayerControlLockFlags.Move);
        public bool IsJumpLocked => CurrentLocks.HasFlag(PlayerControlLockFlags.Jump);
        public bool IsBasicSkillLocked => CurrentLocks.HasFlag(PlayerControlLockFlags.BasicSkill);
        public bool IsSwordArtLocked => CurrentLocks.HasFlag(PlayerControlLockFlags.SwordArt);
        public bool IsDashLocked => CurrentLocks.HasFlag(PlayerControlLockFlags.Dash);

        public event Action<PlayerControlLockFlags> OnLocksChanged;

        public void AddLock(object source, PlayerControlLockFlags flags)
        {
            if (source == null || flags == PlayerControlLockFlags.None)
            {
                return;
            }

            locks[source] = flags;
            RecalculateLocks();

            if (logLocks)
            {
                Debug.Log($"{name} added control lock: {flags}", this);
            }
        }

        public void RemoveLock(object source)
        {
            if (source == null)
            {
                return;
            }

            if (!locks.Remove(source))
            {
                return;
            }

            RecalculateLocks();

            if (logLocks)
            {
                Debug.Log($"{name} removed control lock.", this);
            }
        }

        public void ClearAllLocks()
        {
            if (locks.Count == 0 && CurrentLocks == PlayerControlLockFlags.None)
            {
                return;
            }

            locks.Clear();
            RecalculateLocks();
        }

        private void RecalculateLocks()
        {
            PlayerControlLockFlags nextLocks = PlayerControlLockFlags.None;

            foreach (PlayerControlLockFlags flags in locks.Values)
            {
                nextLocks |= flags;
            }

            if (nextLocks == CurrentLocks)
            {
                return;
            }

            CurrentLocks = nextLocks;
            OnLocksChanged?.Invoke(CurrentLocks);
        }

        private void OnDisable()
        {
            ClearAllLocks();
        }
    }
}
