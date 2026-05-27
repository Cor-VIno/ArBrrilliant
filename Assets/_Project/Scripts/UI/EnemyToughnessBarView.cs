using JingHongLu.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JingHongLu.UI
{
    public sealed class EnemyToughnessBarView : MonoBehaviour
    {
        [SerializeField] private ToughnessComponent toughness;
        [SerializeField] private Health health;
        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.15f, 0f);
        [SerializeField] private GameObject root;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private bool hideWhenBroken;
        [SerializeField] private bool showValueText = true;
        [SerializeField] private bool keepReadableWhenParentFlipped = true;
        [SerializeField] private bool faceCamera = true;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Vector3 baseLocalScale = Vector3.one;

        private void Awake()
        {
            ResolveReferences();
            CacheBaseLocalScale();
        }

        private void OnEnable()
        {
            ResolveReferences();
            CacheBaseLocalScale();
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void LateUpdate()
        {
            Transform rootTransform = RootTransform;
            if (rootTransform == null || followTarget == null)
            {
                return;
            }

            rootTransform.position = followTarget.position + worldOffset;
            ApplyFacing(rootTransform);
            ApplyReadableScale(rootTransform);
        }

        private Transform RootTransform => root != null ? root.transform : transform;

        private void ResolveReferences()
        {
            if (toughness == null)
            {
                toughness = GetComponentInParent<ToughnessComponent>();
            }

            if (health == null)
            {
                health = GetComponentInParent<Health>();
            }

            if (followTarget == null && toughness != null)
            {
                followTarget = toughness.transform;
            }

            if (followTarget == null)
            {
                followTarget = transform;
            }

            if (root == null)
            {
                root = gameObject;
            }
        }

        private void CacheBaseLocalScale()
        {
            Transform rootTransform = RootTransform;
            if (rootTransform == null)
            {
                return;
            }

            if (baseLocalScale == Vector3.one && rootTransform.localScale != Vector3.one)
            {
                baseLocalScale = rootTransform.localScale;
            }
        }

        private void ApplyFacing(Transform rootTransform)
        {
            if (!faceCamera)
            {
                return;
            }

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            rootTransform.rotation = targetCamera != null
                ? targetCamera.transform.rotation
                : Quaternion.identity;
        }

        private void ApplyReadableScale(Transform rootTransform)
        {
            Vector3 scale = baseLocalScale;

            if (keepReadableWhenParentFlipped)
            {
                float parentX = rootTransform.parent != null
                    ? rootTransform.parent.lossyScale.x
                    : 1f;
                scale.x = Mathf.Abs(scale.x) * (parentX < 0f ? -1f : 1f);
            }

            rootTransform.localScale = scale;
        }

        private void Subscribe()
        {
            if (toughness == null)
            {
                return;
            }

            toughness.OnToughnessChanged -= HandleToughnessChanged;
            toughness.OnBroken -= HandleBroken;
            toughness.OnBreakRecovered -= HandleBreakRecovered;
            toughness.OnToughnessChanged += HandleToughnessChanged;
            toughness.OnBroken += HandleBroken;
            toughness.OnBreakRecovered += HandleBreakRecovered;

            if (health != null)
            {
                health.OnDied -= HandleDied;
                health.OnDied += HandleDied;
            }
        }

        private void Unsubscribe()
        {
            if (toughness != null)
            {
                toughness.OnToughnessChanged -= HandleToughnessChanged;
                toughness.OnBroken -= HandleBroken;
                toughness.OnBreakRecovered -= HandleBreakRecovered;
            }

            if (health != null)
            {
                health.OnDied -= HandleDied;
            }
        }

        private void HandleToughnessChanged(float current, float max)
        {
            Refresh(current, max);
        }

        private void HandleBroken()
        {
            Refresh(0f, toughness != null ? toughness.MaxToughness : 0f);
        }

        private void HandleBreakRecovered()
        {
            Refresh();
        }

        private void HandleDied()
        {
            SetRootActive(false);
        }

        private void Refresh()
        {
            if (toughness == null)
            {
                SetRootActive(false);
                return;
            }

            Refresh(toughness.CurrentToughness, toughness.MaxToughness);
        }

        private void Refresh(float current, float max)
        {
            float normalized = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            bool isBroken = current <= 0f;

            if (fillImage != null)
            {
                SetHorizontalFill(fillImage.rectTransform, normalized);
            }

            if (valueText != null)
            {
                valueText.gameObject.SetActive(showValueText && (!hideWhenBroken || !isBroken));
                valueText.text = isBroken
                    ? "Break"
                    : $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }

            SetRootActive(!hideWhenBroken || !isBroken);
        }

        private static void SetHorizontalFill(RectTransform fill, float normalized)
        {
            if (fill == null)
            {
                return;
            }

            normalized = Mathf.Clamp01(normalized);
            fill.anchorMin = new Vector2(0f, 0f);
            fill.anchorMax = new Vector2(normalized, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
        }

        private void SetRootActive(bool active)
        {
            if (root != null && root.activeSelf != active)
            {
                root.SetActive(active);
            }
        }
    }
}
