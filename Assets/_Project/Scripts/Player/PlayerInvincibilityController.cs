using System;
using UnityEngine;

namespace JingHongLu.Player
{
    public sealed class PlayerInvincibilityController : MonoBehaviour
    {
        [SerializeField] private bool isInvincible;
        [SerializeField] private bool logStateChange;

        public bool IsInvincible => isInvincible;

        public event Action<bool> OnInvincibilityChanged;

        public void SetInvincible(bool value)
        {
            if (isInvincible == value)
            {
                return;
            }

            isInvincible = value;
            OnInvincibilityChanged?.Invoke(isInvincible);

            if (logStateChange)
            {
                Debug.Log(
                    $"{name} invincibility set to {isInvincible}.",
                    this);
            }
        }
    }
}
