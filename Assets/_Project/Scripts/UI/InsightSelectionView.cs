using System.Collections.Generic;
using JingHongLu.SwordArts;
using UnityEngine;

namespace JingHongLu.UI
{
    public sealed class InsightSelectionView : MonoBehaviour
    {
        [SerializeField] private InsightSelectionController insightSelection;
        [SerializeField] private GameObject root;
        [SerializeField] private Transform itemRoot;
        [SerializeField] private InsightSelectionItemView itemPrefab;
        [SerializeField] private bool clearItemsOnEnd = true;

        private readonly List<InsightSelectionItemView> itemViews =
            new List<InsightSelectionItemView>();
        private bool hasWarnedMissingReferences;

        private void Awake()
        {
            ResolveReferences();

            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (insightSelection == null)
            {
                return;
            }

            insightSelection.OnInsightStarted += HandleInsightStarted;
            insightSelection.OnSelectionChanged += HandleSelectionChanged;
            insightSelection.OnInsightEnded += HandleInsightEnded;
        }

        private void OnDisable()
        {
            if (insightSelection == null)
            {
                return;
            }

            insightSelection.OnInsightStarted -= HandleInsightStarted;
            insightSelection.OnSelectionChanged -= HandleSelectionChanged;
            insightSelection.OnInsightEnded -= HandleInsightEnded;
        }

        private void HandleInsightStarted(
            IReadOnlyList<SwordArtData> candidates,
            int selectedIndex)
        {
            if (!HasRequiredReferences())
            {
                return;
            }

            root.SetActive(true);
            ClearItems();

            if (candidates == null)
            {
                return;
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                InsightSelectionItemView item = Instantiate(itemPrefab, itemRoot);
                item.Setup(candidates[i], i == selectedIndex);
                item.OnClicked -= HandleItemClicked;
                item.OnClicked += HandleItemClicked;
                item.OnHovered -= HandleItemHovered;
                item.OnHovered += HandleItemHovered;
                itemViews.Add(item);
            }

            RefreshSelection(selectedIndex);
        }

        private void HandleSelectionChanged(
            IReadOnlyList<SwordArtData> candidates,
            int selectedIndex)
        {
            RefreshSelection(selectedIndex);
        }

        private void HandleInsightEnded()
        {
            if (root != null)
            {
                root.SetActive(false);
            }

            if (clearItemsOnEnd)
            {
                ClearItems();
            }
        }

        private void RefreshSelection(int selectedIndex)
        {
            for (int i = 0; i < itemViews.Count; i++)
            {
                itemViews[i].SetSelected(i == selectedIndex);
            }
        }

        private void ClearItems()
        {
            for (int i = itemViews.Count - 1; i >= 0; i--)
            {
                if (itemViews[i] != null)
                {
                    itemViews[i].OnClicked -= HandleItemClicked;
                    itemViews[i].OnHovered -= HandleItemHovered;
                    Destroy(itemViews[i].gameObject);
                }
            }

            itemViews.Clear();
        }

        private void HandleItemClicked(SwordArtData swordArt)
        {
            Debug.Log($"Insight view received click: {swordArt?.DisplayName}", this);

            if (insightSelection == null)
            {
                return;
            }

            insightSelection.ConfirmSelection(swordArt);
        }

        private void HandleItemHovered(InsightSelectionItemView hoveredItem)
        {
            int index = itemViews.IndexOf(hoveredItem);

            if (index < 0)
            {
                return;
            }

            RefreshSelection(index);
        }

        private void ResolveReferences()
        {
            if (insightSelection == null)
            {
                insightSelection = FindAnyObjectByType<InsightSelectionController>();
            }
        }

        private bool HasRequiredReferences()
        {
            if (root != null && itemRoot != null && itemPrefab != null)
            {
                return true;
            }

            if (!hasWarnedMissingReferences)
            {
                Debug.LogWarning(
                    "InsightSelectionView missing Root, ItemRoot or ItemPrefab.",
                    this);
                hasWarnedMissingReferences = true;
            }

            return false;
        }
    }
}
