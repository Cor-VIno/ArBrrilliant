using JingHongLu.Input;
using JingHongLu.Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JingHongLu.SwordArts
{
    public sealed class InsightInputGuard : MonoBehaviour
    {
        [SerializeField] private InsightSelectionController insightSelection;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private Rigidbody2D playerBody;
        [SerializeField] private PlayerInvincibilityController invincibilityController;
        [SerializeField] private PlayerControlLockController controlLock;
        [SerializeField] private bool blockGameplayInput = true;
        [SerializeField] private bool enableInvincibility = true;
        [SerializeField] private bool stopHorizontalVelocityOnStart = true;
        [SerializeField] private bool cancelOnBlockedGameplayInput = false;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (insightSelection != null)
            {
                insightSelection.OnInsightStarted += HandleInsightStarted;
                insightSelection.OnInsightEnded += HandleInsightEnded;
            }

            if (inputReader != null)
            {
                inputReader.OnCancelPressed += HandleCancelPressed;
                inputReader.OnBlockedGameplayInputPressed +=
                    HandleBlockedGameplayInputPressed;
            }
        }

        private void OnDisable()
        {
            if (insightSelection != null)
            {
                insightSelection.OnInsightStarted -= HandleInsightStarted;
                insightSelection.OnInsightEnded -= HandleInsightEnded;
            }

            if (inputReader != null)
            {
                inputReader.OnCancelPressed -= HandleCancelPressed;
                inputReader.OnBlockedGameplayInputPressed -=
                    HandleBlockedGameplayInputPressed;
                if (controlLock == null)
                {
                    inputReader.SetGameplayInputBlocked(false);
                }
            }

            if (controlLock != null)
            {
                controlLock.RemoveLock(this);
            }

            if (invincibilityController != null)
            {
                invincibilityController.SetInvincible(false);
            }
        }

        private void ResolveReferences()
        {
            if (insightSelection == null)
            {
                TryGetComponent(out insightSelection);
            }

            if (inputReader == null)
            {
                TryGetComponent(out inputReader);
            }

            if (playerBody == null)
            {
                TryGetComponent(out playerBody);
            }

            if (invincibilityController == null)
            {
                TryGetComponent(out invincibilityController);
            }

            if (controlLock == null)
            {
                controlLock = GetComponentInParent<PlayerControlLockController>();
            }
        }

        private void HandleInsightStarted(
            System.Collections.Generic.IReadOnlyList<SwordArtData> candidates,
            int selectedIndex)
        {
            if (blockGameplayInput && controlLock != null)
            {
                controlLock.AddLock(this, PlayerControlLockFlags.Gameplay);
            }
            else if (blockGameplayInput && inputReader != null)
            {
                inputReader.SetGameplayInputBlocked(true);
            }

            if (stopHorizontalVelocityOnStart && playerBody != null)
            {
                Vector2 velocity = playerBody.linearVelocity;
                velocity.x = 0f;
                playerBody.linearVelocity = velocity;
            }

            if (enableInvincibility && invincibilityController != null)
            {
                invincibilityController.SetInvincible(true);
            }
        }

        private void HandleInsightEnded()
        {
            if (controlLock != null)
            {
                controlLock.RemoveLock(this);
            }
            else if (inputReader != null)
            {
                inputReader.SetGameplayInputBlocked(false);
            }

            if (invincibilityController != null)
            {
                invincibilityController.SetInvincible(false);
            }
        }

        private void HandleCancelPressed()
        {
            if (insightSelection != null && insightSelection.IsActive)
            {
                insightSelection.CancelSelection();
            }
        }

        private void HandleBlockedGameplayInputPressed()
        {
            if (!cancelOnBlockedGameplayInput || IsPointerOverUi())
            {
                return;
            }

            if (insightSelection != null && insightSelection.IsActive)
            {
                insightSelection.CancelSelection();
            }
        }

        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject();
        }
    }
}
