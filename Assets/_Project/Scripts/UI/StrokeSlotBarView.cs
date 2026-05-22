using System.Collections.Generic;
using System.Text;
using JingHongLu.SwordArts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JingHongLu.UI
{
    public sealed class StrokeSlotBarView : MonoBehaviour
    {
        [SerializeField] private StrokeRecorder strokeRecorder;
        [SerializeField] private StrokeSlotItemView[] slotViews;
        [SerializeField] private bool refreshOnEnable = true;
        [SerializeField] private bool logStrokeSlotUi = false;

        [Header("Fallback UI")]
        [SerializeField] private bool autoCreateMissingSlots = false;
        [SerializeField] private TMP_FontAsset fontAsset;
        [SerializeField] private Vector2 slotSize = new Vector2(48f, 48f);

        [Header("Sword Art Hint")]
        [SerializeField] private TextMeshProUGUI availableSwordArtHintText;
        [SerializeField] private SwordArtMatcher swordArtMatcher;

        private bool warnedMissingRecorder;
        private bool warnedMissingSlots;

        private void Awake()
        {
            ResolveReferences();
            EnsureUiReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (strokeRecorder != null)
            {
                strokeRecorder.OnRecordsChangedDetailed += HandleRecordsChangedDetailed;
                strokeRecorder.OnRecordsChanged += HandleRecordsChanged;
            }
            else
            {
                LogMissingRecorderWarning();
            }

            if (swordArtMatcher != null)
            {
                swordArtMatcher.OnAvailableSwordArtsChanged +=
                    HandleAvailableSwordArtsChanged;
            }

            if (refreshOnEnable)
            {
                RefreshSlots();
                RefreshAvailableSwordArtHint(
                    swordArtMatcher != null
                        ? swordArtMatcher.GetAvailableSwordArts()
                        : null);
            }
        }

        private void OnDisable()
        {
            if (strokeRecorder != null)
            {
                strokeRecorder.OnRecordsChangedDetailed -= HandleRecordsChangedDetailed;
                strokeRecorder.OnRecordsChanged -= HandleRecordsChanged;
            }

            if (swordArtMatcher != null)
            {
                swordArtMatcher.OnAvailableSwordArtsChanged -=
                    HandleAvailableSwordArtsChanged;
            }
        }

        private void ResolveReferences()
        {
            if (strokeRecorder == null)
            {
                strokeRecorder = FindSceneObject<StrokeRecorder>();
            }

            if (swordArtMatcher == null)
            {
                swordArtMatcher = FindSceneObject<SwordArtMatcher>();
            }
        }

        private void EnsureUiReferences()
        {
            if (!autoCreateMissingSlots ||
                slotViews != null && slotViews.Length > 0)
            {
                return;
            }

            RectTransform root = GetComponent<RectTransform>();

            if (root == null)
            {
                return;
            }

            ConfigureRootRect(root);

            GameObject panelObject = new GameObject(
                "Panel",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            panelObject.transform.SetParent(root, false);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.35f);
            panelImage.raycastTarget = false;

            GameObject slotRootObject = new GameObject(
                "SlotRoot",
                typeof(RectTransform),
                typeof(HorizontalLayoutGroup));
            slotRootObject.transform.SetParent(panelObject.transform, false);

            RectTransform slotRootRect =
                slotRootObject.GetComponent<RectTransform>();
            slotRootRect.anchorMin = new Vector2(0.5f, 1f);
            slotRootRect.anchorMax = new Vector2(0.5f, 1f);
            slotRootRect.pivot = new Vector2(0.5f, 1f);
            slotRootRect.anchoredPosition = new Vector2(0f, -16f);
            slotRootRect.sizeDelta = new Vector2(420f, slotSize.y);

            HorizontalLayoutGroup horizontalLayout =
                slotRootObject.GetComponent<HorizontalLayoutGroup>();
            horizontalLayout.spacing = 6f;
            horizontalLayout.childAlignment = TextAnchor.MiddleCenter;
            horizontalLayout.childControlWidth = true;
            horizontalLayout.childControlHeight = true;
            horizontalLayout.childForceExpandWidth = false;
            horizontalLayout.childForceExpandHeight = false;

            slotViews = new StrokeSlotItemView[7];

            for (int i = 0; i < slotViews.Length; i++)
            {
                slotViews[i] = CreateSlotView(slotRootObject.transform, i);
            }

            availableSwordArtHintText = CreateHintText(panelObject.transform);
        }

        private StrokeSlotItemView CreateSlotView(Transform parent, int index)
        {
            GameObject slotObject = new GameObject(
                $"StrokeSlot_{index}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(CanvasGroup),
                typeof(LayoutElement),
                typeof(StrokeSlotItemView));
            slotObject.transform.SetParent(parent, false);

            RectTransform slotRect = slotObject.GetComponent<RectTransform>();
            slotRect.sizeDelta = slotSize;

            Image background = slotObject.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.55f);
            background.raycastTarget = false;

            LayoutElement layoutElement = slotObject.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = slotSize.x;
            layoutElement.preferredHeight = slotSize.y;

            GameObject textObject = new GameObject(
                "StrokeText",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            textObject.transform.SetParent(slotObject.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = "-";
            if (fontAsset != null)
            {
                text.font = fontAsset;
            }
            text.fontSize = 26f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;

            StrokeSlotItemView itemView =
                slotObject.GetComponent<StrokeSlotItemView>();
            itemView.Initialize(
                text,
                background,
                slotObject.GetComponent<CanvasGroup>());
            itemView.SetEmpty();
            return itemView;
        }

        private static void ConfigureRootRect(RectTransform root)
        {
            root.anchorMin = new Vector2(0.5f, 0f);
            root.anchorMax = new Vector2(0.5f, 0f);
            root.pivot = new Vector2(0.5f, 0f);
            root.anchoredPosition = new Vector2(0f, 80f);
            root.sizeDelta = new Vector2(560f, 120f);
        }

        private TextMeshProUGUI CreateHintText(Transform parent)
        {
            GameObject hintObject = new GameObject(
                "AvailableSwordArtHintText",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI),
                typeof(LayoutElement));
            hintObject.transform.SetParent(parent, false);

            RectTransform hintRect = hintObject.GetComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0.5f, 0f);
            hintRect.anchorMax = new Vector2(0.5f, 0f);
            hintRect.pivot = new Vector2(0.5f, 0f);
            hintRect.anchoredPosition = new Vector2(0f, 12f);
            hintRect.sizeDelta = new Vector2(520f, 28f);

            LayoutElement layoutElement = hintObject.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 30f;

            TextMeshProUGUI text = hintObject.GetComponent<TextMeshProUGUI>();
            text.text = string.Empty;
            if (fontAsset != null)
            {
                text.font = fontAsset;
            }
            text.fontSize = 20f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private void HandleRecordsChanged()
        {
            RefreshSlots();
        }

        private void HandleRecordsChangedDetailed(
            IReadOnlyList<StrokeRecord> records)
        {
            RefreshSlots(records);
        }

        private void HandleAvailableSwordArtsChanged(
            IReadOnlyList<SwordArtData> availableSwordArts)
        {
            RefreshAvailableSwordArtHint(availableSwordArts);
        }

        private void RefreshSlots()
        {
            if (strokeRecorder == null)
            {
                LogMissingRecorderWarning();
                ClearSlots();
                return;
            }

            RefreshSlots(strokeRecorder.GetActiveRecords());
        }

        private void RefreshSlots(IReadOnlyList<StrokeRecord> records)
        {
            if (slotViews == null || slotViews.Length == 0)
            {
                LogMissingSlotsWarning();
                return;
            }

            if (slotViews.Length < 7)
            {
                LogMissingSlotsWarning();
            }

            int visibleSlotCount = Mathf.Min(slotViews.Length, 7);

            for (int i = 0; i < visibleSlotCount; i++)
            {
                StrokeSlotItemView slotView = slotViews[i];

                if (slotView == null)
                {
                    continue;
                }

                if (records != null && i < records.Count)
                {
                    slotView.SetStroke(records[i]);
                }
                else
                {
                    slotView.SetEmpty();
                }
            }

            if (logStrokeSlotUi)
            {
                Debug.Log(
                    $"Stroke slot UI refreshed. Count={records?.Count ?? 0}",
                    this);
            }
        }

        private void RefreshAvailableSwordArtHint(
            IReadOnlyList<SwordArtData> availableSwordArts)
        {
            if (availableSwordArtHintText == null)
            {
                return;
            }

            if (availableSwordArts == null || availableSwordArts.Count == 0)
            {
                availableSwordArtHintText.text = string.Empty;
                return;
            }

            if (availableSwordArts.Count == 1)
            {
                availableSwordArtHintText.text =
                    $"可释放：{GetSwordArtDisplayName(availableSwordArts[0])}";
                return;
            }

            StringBuilder builder = new StringBuilder("可识破：");

            for (int i = 0; i < availableSwordArts.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(" / ");
                }

                builder.Append(GetSwordArtDisplayName(availableSwordArts[i]));
            }

            availableSwordArtHintText.text = builder.ToString();
        }

        private void ClearSlots()
        {
            if (slotViews == null)
            {
                return;
            }

            for (int i = 0; i < slotViews.Length; i++)
            {
                if (slotViews[i] != null)
                {
                    slotViews[i].SetEmpty();
                }
            }
        }

        private void LogMissingRecorderWarning()
        {
            if (warnedMissingRecorder)
            {
                return;
            }

            warnedMissingRecorder = true;
            Debug.LogWarning(
                $"{nameof(StrokeSlotBarView)} requires a {nameof(StrokeRecorder)} reference.",
                this);
        }

        private void LogMissingSlotsWarning()
        {
            if (warnedMissingSlots)
            {
                return;
            }

            warnedMissingSlots = true;
            Debug.LogWarning(
                $"{nameof(StrokeSlotBarView)} expects 7 configured slot views.",
                this);
        }

        private static string GetSwordArtDisplayName(SwordArtData swordArt)
        {
            return swordArt != null ? swordArt.DisplayName : "Unknown";
        }

        private static T FindSceneObject<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return FindFirstObjectByType<T>();
#else
            return FindObjectOfType<T>();
#endif
        }
    }
}
