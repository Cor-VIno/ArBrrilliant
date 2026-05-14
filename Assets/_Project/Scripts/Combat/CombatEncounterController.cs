using System;
using System.Collections.Generic;
using UnityEngine;

namespace JingHongLu.Combat
{
    public sealed class CombatEncounterController : MonoBehaviour
    {
        [SerializeField] private Health[] enemyHealths;
        [SerializeField] private bool logVictory = true;

        private readonly HashSet<Health> aliveEnemies = new HashSet<Health>();
        private readonly Dictionary<Health, Action> deathHandlers =
            new Dictionary<Health, Action>();
        private bool encounterEnded;

        private void Awake()
        {
            if (enemyHealths == null || enemyHealths.Length == 0)
            {
                enemyHealths = GetComponentsInChildren<Health>();
            }
        }

        private void OnEnable()
        {
            aliveEnemies.Clear();
            deathHandlers.Clear();
            encounterEnded = false;

            if (enemyHealths == null)
            {
                return;
            }

            for (int i = 0; i < enemyHealths.Length; i++)
            {
                Health enemyHealth = enemyHealths[i];

                if (enemyHealth == null || enemyHealth.IsDead)
                {
                    continue;
                }

                aliveEnemies.Add(enemyHealth);
                Action handler = () => HandleEnemyDied(enemyHealth);
                deathHandlers[enemyHealth] = handler;
                enemyHealth.OnDied += handler;
            }

            if (aliveEnemies.Count == 0)
            {
                CompleteEncounter();
            }
        }

        private void OnDisable()
        {
            foreach (KeyValuePair<Health, Action> pair in deathHandlers)
            {
                if (pair.Key != null)
                {
                    pair.Key.OnDied -= pair.Value;
                }
            }

            deathHandlers.Clear();
            aliveEnemies.Clear();
        }

        private void HandleEnemyDied(Health enemyHealth)
        {
            if (enemyHealth != null)
            {
                aliveEnemies.Remove(enemyHealth);
            }

            if (aliveEnemies.Count == 0)
            {
                CompleteEncounter();
            }
        }

        private void CompleteEncounter()
        {
            if (encounterEnded)
            {
                return;
            }

            encounterEnded = true;

            if (logVictory)
            {
                Debug.Log("Encounter completed: victory.", this);
            }
        }
    }
}
