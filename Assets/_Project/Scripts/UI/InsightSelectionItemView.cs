using System;
using JingHongLu.SwordArts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JingHongLu.UI
{
    public sealed class InsightSelectionItemView : MonoBehaviour,
        IPointerEnterHandler
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject selectedRoot;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button button;

        private SwordArtData swordArt;
        private bool hasWarnedMissingButton;

        public SwordArtData SwordArt => swordArt;

        public event Action<SwordArtData> OnClicked;
        public event Action<InsightSelectionItemView> OnHovered;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Reset()
        {
            button = GetComponent<Button>();
        }

        public void InitializeReferences(
            TextMeshProUGUI nameText,
            Image iconImage,
            GameObject selectedRoot,
            CanvasGroup canvasGroup,
            Button button = null)
        {
            this.nameText = nameText;
            this.iconImage = iconImage;
            this.selectedRoot = selectedRoot;
            this.canvasGroup = canvasGroup;
            this.button = button;
        }

        public void Setup(SwordArtData swordArt, bool selected)
        {
            ResolveReferences();
            this.swordArt = swordArt;

            if (nameText != null)
            {
                nameText.text = swordArt != null ? swordArt.DisplayName : "Unknown";
            }

            if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }

            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
                button.onClick.AddListener(HandleClick);
                button.interactable = true;
            }
            else if (!hasWarnedMissingButton)
            {
                Debug.LogWarning(
                    "InsightSelectionItemView requires a Button component.",
                    this);
                hasWarnedMissingButton = true;
            }

            SetSelected(selected);
        }

        public void SetSelected(bool selected)
        {
            if (selectedRoot != null)
            {
                selectedRoot.SetActive(selected);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = selected ? 1f : 0.55f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        private void ResolveReferences()
        {
            if (canvasGroup == null)
            {
                TryGetComponent(out canvasGroup);
            }

            if (button == null)
            {
                TryGetComponent(out button);
            }

            if (nameText == null)
            {
                Transform nameTransform = transform.Find("NameText");

                if (nameTransform != null)
                {
                    nameText = nameTransform.GetComponent<TextMeshProUGUI>();
                }
            }

            if (selectedRoot == null)
            {
                Transform selectedTransform = transform.Find("SelectedRoot");

                if (selectedTransform != null)
                {
                    selectedRoot = selectedTransform.gameObject;
                }
            }
        }

        private void HandleClick()
        {
            Debug.Log($"Insight item clicked: {swordArt?.DisplayName}", this);
            OnClicked?.Invoke(swordArt);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHovered?.Invoke(this);
        }
    }
}
