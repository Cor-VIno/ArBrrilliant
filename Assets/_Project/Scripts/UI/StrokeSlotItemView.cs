using JingHongLu.Skills;
using JingHongLu.SwordArts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JingHongLu.UI
{
    public sealed class StrokeSlotItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI strokeText;
        [SerializeField] private Image background;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Color normalColor = new Color(0f, 0f, 0f, 0.55f);
        [SerializeField] private Color highlightedColor = new Color(0.25f, 0.75f, 1f, 0.75f);

        private bool warnedMissingText;

        public void Initialize(
            TextMeshProUGUI text,
            Image backgroundImage,
            CanvasGroup group)
        {
            strokeText = text;
            background = backgroundImage;
            canvasGroup = group;
        }

        public void SetEmpty()
        {
            SetText("-");

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0.45f;
            }

            SetHighlighted(false);
        }

        public void SetStroke(StrokeRecord record)
        {
            SetText(ToDisplayName(record.StrokeType));

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            SetHighlighted(false);
        }

        public void SetHighlighted(bool highlighted)
        {
            if (background != null)
            {
                background.color = highlighted ? highlightedColor : normalColor;
            }
        }

        private void SetText(string value)
        {
            if (strokeText == null)
            {
                LogMissingTextWarning();
                return;
            }

            strokeText.text = value;
        }

        private void LogMissingTextWarning()
        {
            if (warnedMissingText)
            {
                return;
            }

            warnedMissingText = true;
            Debug.LogWarning(
                $"{nameof(StrokeSlotItemView)} requires a {nameof(TextMeshProUGUI)} reference.",
                this);
        }

        private static string ToDisplayName(StrokeType strokeType)
        {
            return strokeType switch
            {
                StrokeType.Horizontal => "横",
                StrokeType.Vertical => "竖",
                StrokeType.LeftFalling => "撇",
                StrokeType.RightFalling => "捺",
                _ => "-"
            };
        }
    }
}
