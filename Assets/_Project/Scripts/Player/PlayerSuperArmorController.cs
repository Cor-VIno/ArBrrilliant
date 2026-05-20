using System;
using UnityEngine;

namespace JingHongLu.Player
{
    public sealed class PlayerSuperArmorController : MonoBehaviour
    {
        [SerializeField] private bool hasSuperArmor;
        [SerializeField] private bool logStateChange;

        public bool HasSuperArmor => hasSuperArmor;

        public event Action<bool> OnSuperArmorChanged;

        public void SetSuperArmor(bool value)
        {
            if (hasSuperArmor == value)
            {
                return;
            }

            hasSuperArmor = value;
            OnSuperArmorChanged?.Invoke(hasSuperArmor);

            if (logStateChange)
            {
                Debug.Log($"{name} super armor set to {hasSuperArmor}.", this);
            }
        }
    }
}
