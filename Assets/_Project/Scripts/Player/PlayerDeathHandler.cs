using JingHongLu.Combat;
using JingHongLu.Input;
using JingHongLu.Skills;
using UnityEngine;

namespace JingHongLu.Player
{
    public sealed class PlayerDeathHandler : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerSkillController skillController;
        [SerializeField] private PlayerMotor2D motor;
        [SerializeField] private PlayerDashController2D dashController;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private bool logDeath = true;

        private bool isDead;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (health != null)
            {
                health.OnDied += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDied -= HandleDied;
            }
        }

        private void ResolveReferences()
        {
            if (health == null)
            {
                TryGetComponent(out health);
            }

            if (inputReader == null)
            {
                TryGetComponent(out inputReader);
            }

            if (skillController == null)
            {
                TryGetComponent(out skillController);
            }

            if (motor == null)
            {
                TryGetComponent(out motor);
            }

            if (dashController == null)
            {
                TryGetComponent(out dashController);
            }

            if (body == null)
            {
                TryGetComponent(out body);
            }
        }

        private void HandleDied()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;

            if (logDeath)
            {
                Debug.Log("Player died.", this);
            }

            if (inputReader != null)
            {
                inputReader.enabled = false;
            }

            if (skillController != null)
            {
                skillController.enabled = false;
            }

            if (motor != null)
            {
                motor.enabled = false;
            }

            if (dashController != null)
            {
                dashController.enabled = false;
            }

            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }
    }
}
