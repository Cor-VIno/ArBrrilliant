using JingHongLu.Combat;
using UnityEngine;

namespace JingHongLu.SwordArts
{
    public sealed class InsightPerfectDodgeSlowGuard : MonoBehaviour
    {
        [SerializeField] private InsightSelectionController insightSelectionController;
        [SerializeField] private PerfectDodgeSlowMotionController slowMotionController;
        [SerializeField] private bool logCancel = true;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (insightSelectionController != null)
            {
                insightSelectionController.OnInsightStarted += HandleInsightStarted;
            }
        }

        private void OnDisable()
        {
            if (insightSelectionController != null)
            {
                insightSelectionController.OnInsightStarted -= HandleInsightStarted;
            }
        }

        private void ResolveReferences()
        {
            if (insightSelectionController == null)
            {
                insightSelectionController = FindAnyObjectByType<InsightSelectionController>();
            }

            if (slowMotionController == null)
            {
                slowMotionController = PerfectDodgeSlowMotionController.Instance;
            }

            if (slowMotionController == null)
            {
                slowMotionController = FindAnyObjectByType<PerfectDodgeSlowMotionController>();
            }
        }

        private void HandleInsightStarted(
            System.Collections.Generic.IReadOnlyList<SwordArtData> candidates,
            int selectedIndex)
        {
            if (slowMotionController == null ||
                !slowMotionController.IsPerfectDodgeSlowActive)
            {
                return;
            }

            slowMotionController.CancelPerfectDodgeSlow();

            if (logCancel)
            {
                Debug.Log("[PerfectDodgeSlow] Canceled by insight.", this);
            }
        }
    }
}
