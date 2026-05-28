using System.Collections.Generic;
using UnityEngine;

namespace JingHongLu.GameFlow
{
    public sealed class CombatPauseController : MonoBehaviour
    {
        private static readonly HashSet<object> pauseOwners = new();

        public static bool IsCombatPaused => pauseOwners.Count > 0;

        public void AddPause(object owner)
        {
            if (owner == null)
            {
                return;
            }

            pauseOwners.Add(owner);
        }

        public void RemovePause(object owner)
        {
            if (owner == null)
            {
                return;
            }

            pauseOwners.Remove(owner);
        }

        private void OnDisable()
        {
            pauseOwners.Clear();
        }
    }
}
