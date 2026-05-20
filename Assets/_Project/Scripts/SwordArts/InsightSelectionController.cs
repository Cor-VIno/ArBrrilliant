using System;
using System.Collections.Generic;
using JingHongLu.Core;
using UnityEngine;

namespace JingHongLu.SwordArts
{
    public sealed class InsightSelectionController : MonoBehaviour
    {
        [SerializeField] private TimeScaleController timeScaleController;
        [SerializeField] private float slowMotionScale = 0.25f;
        [SerializeField] private float maxDuration = 1.5f;
        [SerializeField] private bool useTimeStop = true;
        [SerializeField] private bool logInsight = true;

        private readonly List<SwordArtData> candidates = new List<SwordArtData>();
        private int selectedIndex;
        private float timer;
        private bool isActive;

        public bool IsActive => isActive;
        public IReadOnlyList<SwordArtData> Candidates => candidates;
        public int SelectedIndex => selectedIndex;

        public SwordArtData SelectedSwordArt
        {
            get
            {
                if (!isActive || candidates.Count == 0)
                {
                    return null;
                }

                int safeIndex = Mathf.Clamp(selectedIndex, 0, candidates.Count - 1);
                return candidates[safeIndex];
            }
        }

        public event Action<IReadOnlyList<SwordArtData>, int> OnInsightStarted;
        public event Action<IReadOnlyList<SwordArtData>, int> OnSelectionChanged;
        public event Action<SwordArtData> OnInsightConfirmed;
        public event Action OnInsightCanceled;
        public event Action OnInsightTimedOut;
        public event Action OnInsightEnded;

        private void Update()
        {
            if (!isActive)
            {
                return;
            }

            timer += Time.unscaledDeltaTime;

            if (timer >= Mathf.Max(0.01f, maxDuration))
            {
                TimeoutSelection();
            }
        }

        private void OnDisable()
        {
            if (isActive)
            {
                EndSelection();
            }
        }

        public void BeginSelection(IReadOnlyList<SwordArtData> availableSwordArts)
        {
            if (availableSwordArts == null || availableSwordArts.Count == 0)
            {
                return;
            }

            if (isActive)
            {
                EndSelection();
            }

            candidates.Clear();

            for (int i = 0; i < availableSwordArts.Count; i++)
            {
                if (availableSwordArts[i] != null)
                {
                    candidates.Add(availableSwordArts[i]);
                }
            }

            if (candidates.Count == 0)
            {
                return;
            }

            selectedIndex = 0;
            timer = 0f;
            isActive = true;

            if (timeScaleController != null)
            {
                if (useTimeStop)
                {
                    timeScaleController.EnterTimeStop();
                }
                else
                {
                    timeScaleController.EnterSlowMotion(slowMotionScale);
                }
            }

            if (logInsight)
            {
                Debug.Log($"Insight selection started. Candidates: {candidates.Count}", this);
            }

            OnInsightStarted?.Invoke(candidates, selectedIndex);
        }

        public void SelectNext()
        {
            if (!isActive || candidates.Count == 0)
            {
                return;
            }

            selectedIndex = (selectedIndex + 1) % candidates.Count;
            OnSelectionChanged?.Invoke(candidates, selectedIndex);
        }

        public void SelectPrevious()
        {
            if (!isActive || candidates.Count == 0)
            {
                return;
            }

            selectedIndex = (selectedIndex - 1 + candidates.Count) % candidates.Count;
            OnSelectionChanged?.Invoke(candidates, selectedIndex);
        }

        public void ConfirmSelection()
        {
            ConfirmSelection(SelectedSwordArt);
        }

        public void ConfirmSelection(SwordArtData selectedSwordArt)
        {
            Debug.Log($"Insight confirmed: {selectedSwordArt?.DisplayName}", this);

            if (!isActive)
            {
                return;
            }

            if (selectedSwordArt == null || !candidates.Contains(selectedSwordArt))
            {
                return;
            }

            if (logInsight)
            {
                Debug.Log(
                    $"Insight selection confirmed: {selectedSwordArt.DisplayName}",
                    this);
            }

            OnInsightConfirmed?.Invoke(selectedSwordArt);
            EndSelection();
        }

        public void CancelSelection()
        {
            if (!isActive)
            {
                return;
            }

            if (logInsight)
            {
                Debug.Log("Insight selection canceled.", this);
            }

            OnInsightCanceled?.Invoke();
            EndSelection();
        }

        private void TimeoutSelection()
        {
            if (!isActive)
            {
                return;
            }

            if (logInsight)
            {
                Debug.Log("Insight selection timed out.", this);
            }

            OnInsightTimedOut?.Invoke();
            EndSelection();
        }

        private void EndSelection()
        {
            if (!isActive)
            {
                return;
            }

            isActive = false;

            if (timeScaleController != null)
            {
                timeScaleController.ExitSlowMotion();
            }

            OnInsightEnded?.Invoke();
        }
    }
}
