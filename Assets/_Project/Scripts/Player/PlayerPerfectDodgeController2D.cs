using System;
using System.Collections.Generic;
using JingHongLu.Combat;
using UnityEngine;

namespace JingHongLu.Player
{
    public sealed class PlayerPerfectDodgeController2D : MonoBehaviour
    {
        [SerializeField] private PlayerDashController2D dashController;
        [SerializeField] private Collider2D dodgeTrigger;
        [SerializeField] private TeamId enemyTeam = TeamId.Enemy;
        [SerializeField] private bool logPerfectDodge = true;

        private readonly HashSet<Hitbox2D> triggeredHitboxes = new HashSet<Hitbox2D>();
        private bool isDetectionActive;
        private Vector2 currentDashDirection;

        public event Action<PerfectDodgeEventData> OnPerfectDodgeTriggered;

        private void Awake()
        {
            if (dashController == null)
            {
                dashController = GetComponentInParent<PlayerDashController2D>();
            }

            if (dashController == null)
            {
                TryGetComponent(out dashController);
            }

            if (dodgeTrigger == null)
            {
                dodgeTrigger = FindDodgeTriggerInChildren();
            }

            if (dodgeTrigger == null)
            {
                Debug.LogWarning("PlayerPerfectDodgeController2D requires a PerfectDodgeTrigger collider.", this);
            }

            SetDetectionActive(false);
        }

        private void OnEnable()
        {
            if (dashController != null)
            {
                dashController.OnDashStarted += HandleDashStarted;
                dashController.OnDashFinished += HandleDashFinished;
            }
        }

        private void OnDisable()
        {
            if (dashController != null)
            {
                dashController.OnDashStarted -= HandleDashStarted;
                dashController.OnDashFinished -= HandleDashFinished;
            }

            SetDetectionActive(false);
            triggeredHitboxes.Clear();
        }

        private Collider2D FindDodgeTriggerInChildren()
        {
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D candidate = colliders[i];

                if (candidate != null && candidate.gameObject.name == "PerfectDodgeTrigger")
                {
                    return candidate;
                }
            }

            return null;
        }

        private void HandleDashStarted(Vector2 direction)
        {
            currentDashDirection = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : Vector2.right;
            triggeredHitboxes.Clear();
            SetDetectionActive(true);
        }

        private void HandleDashFinished()
        {
            SetDetectionActive(false);
            triggeredHitboxes.Clear();
        }

        private void SetDetectionActive(bool active)
        {
            isDetectionActive = active;

            if (dodgeTrigger != null)
            {
                dodgeTrigger.enabled = active;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryTriggerPerfectDodge(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryTriggerPerfectDodge(other);
        }

        private void TryTriggerPerfectDodge(Collider2D other)
        {
            if (!isDetectionActive || other == null)
            {
                return;
            }

            Hitbox2D hitbox = other.GetComponent<Hitbox2D>();

            if (hitbox == null)
            {
                hitbox = other.GetComponentInParent<Hitbox2D>();
            }

            if (hitbox == null ||
                !hitbox.CanBePerfectDodged ||
                hitbox.OwnerTeam != enemyTeam ||
                triggeredHitboxes.Contains(hitbox))
            {
                return;
            }

            triggeredHitboxes.Add(hitbox);

            Vector2 contactPoint = dodgeTrigger != null
                ? other.ClosestPoint(dodgeTrigger.bounds.center)
                : (Vector2)transform.position;
            PerfectDodgeEventData eventData = new PerfectDodgeEventData(
                player: gameObject,
                enemy: hitbox.Owner,
                dodgedHitbox: hitbox,
                dodgeDirection: currentDashDirection,
                contactPoint: contactPoint,
                time: Time.time);

            OnPerfectDodgeTriggered?.Invoke(eventData);

            if (logPerfectDodge)
            {
                Debug.Log($"[PerfectDodge] Triggered by {hitbox.name}", this);
            }
        }
    }
}
