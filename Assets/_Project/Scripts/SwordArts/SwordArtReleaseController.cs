using System;
using System.Collections.Generic;
using JingHongLu.Input;
using UnityEngine;

namespace JingHongLu.SwordArts
{
    public sealed class SwordArtReleaseController : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private SwordArtMatcher matcher;
        [SerializeField] private InsightSelectionController insightSelection;
        [SerializeField] private bool logRelease = true;

        public event Action OnReleaseFailed;
        public event Action<SwordArtData> OnDirectReleased;
        public event Action<IReadOnlyList<SwordArtData>> OnMultipleSwordArtsAvailable;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (inputReader != null)
            {
                inputReader.OnSwordArtReleasePressed +=
                    HandleSwordArtReleasePressed;
            }

            if (insightSelection != null)
            {
                insightSelection.OnInsightConfirmed += HandleInsightConfirmed;
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.OnSwordArtReleasePressed -=
                    HandleSwordArtReleasePressed;
            }

            if (insightSelection != null)
            {
                insightSelection.OnInsightConfirmed -= HandleInsightConfirmed;
            }
        }

        private void ResolveReferences()
        {
            if (inputReader == null)
            {
                TryGetComponent(out inputReader);
            }

            if (matcher == null)
            {
                TryGetComponent(out matcher);
            }

            if (insightSelection == null)
            {
                TryGetComponent(out insightSelection);
            }
        }

        private void HandleSwordArtReleasePressed()
        {
            if (insightSelection != null && insightSelection.IsActive)
            {
                insightSelection.CancelSelection();
                return;
            }

            if (matcher == null)
            {
                OnReleaseFailed?.Invoke();
                return;
            }

            IReadOnlyList<SwordArtData> available =
                matcher.GetAvailableSwordArts();

            if (available.Count == 0)
            {
                if (logRelease)
                {
                    Debug.Log("No sword art available.", this);
                }

                OnReleaseFailed?.Invoke();
                return;
            }

            if (available.Count == 1)
            {
                SwordArtData swordArt = available[0];
                bool success = matcher.RequestTriggerSwordArt(swordArt);

                if (!success)
                {
                    OnReleaseFailed?.Invoke();
                    return;
                }

                if (logRelease)
                {
                    Debug.Log($"Released sword art: {swordArt.DisplayName}", this);
                }

                OnDirectReleased?.Invoke(swordArt);
                return;
            }

            if (logRelease)
            {
                Debug.Log($"Multiple sword arts available: {available.Count}", this);
            }

            if (insightSelection != null)
            {
                insightSelection.BeginSelection(available);
            }
            else
            {
                OnMultipleSwordArtsAvailable?.Invoke(available);
            }
        }

        private void HandleInsightConfirmed(SwordArtData swordArt)
        {
            if (swordArt == null)
            {
                OnReleaseFailed?.Invoke();
                return;
            }

            bool success = matcher != null &&
                matcher.RequestTriggerSwordArt(swordArt);

            if (success)
            {
                if (logRelease)
                {
                    Debug.Log(
                        $"Released sword art from insight: {swordArt.DisplayName}",
                        this);
                }

                OnDirectReleased?.Invoke(swordArt);
                return;
            }

            if (logRelease)
            {
                Debug.LogWarning(
                    $"Failed to release sword art from insight: {swordArt.DisplayName}",
                    this);
            }

            OnReleaseFailed?.Invoke();
        }
    }
}
