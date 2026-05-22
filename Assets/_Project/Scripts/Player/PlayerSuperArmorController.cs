using System;
using System.Collections.Generic;
using UnityEngine;

namespace JingHongLu.Player
{
    public sealed class PlayerSuperArmorController : MonoBehaviour
    {
        [SerializeField] private bool hasSuperArmor;
        [SerializeField] private bool logStateChange;

        private readonly HashSet<object> armorSources = new HashSet<object>();
        private readonly object legacySource = new object();

        public bool HasSuperArmor => hasSuperArmor;

        public event Action<bool> OnSuperArmorChanged;

        public void SetSuperArmor(bool value)
        {
            if (value)
            {
                AddSuperArmor(legacySource);
            }
            else
            {
                RemoveSuperArmor(legacySource);
            }
        }

        public void AddSuperArmor(object source)
        {
            if (source == null)
            {
                return;
            }

            armorSources.Add(source);
            RecalculateSuperArmor();
        }

        public void RemoveSuperArmor(object source)
        {
            if (source == null)
            {
                return;
            }

            armorSources.Remove(source);
            RecalculateSuperArmor();
        }

        private void RecalculateSuperArmor()
        {
            SetSuperArmorState(armorSources.Count > 0);
        }

        private void SetSuperArmorState(bool value)
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
