using JingHongLu.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JingHongLu.UI
{
    public sealed class EnemyHealthBarView : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.45f, 0f);
        [SerializeField] private GameObject root;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI valueText;
        [Header("Shield Overlay")]
        [SerializeField] private ShieldComponent shield;
        [SerializeField] private GameObject shieldOverlayRoot;
        [SerializeField] private Image shieldOverlayFill;
        [SerializeField] private bool hideWhenDead = true;
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
            if (health == null)
            {
                health = GetComponentInParent<Health>();
            }

            if (shield == null)
            {
                shield = GetComponentInParent<ShieldComponent>();
            }

            if (followTarget == null && health != null)
            {
                followTarget = health.transform;
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
            if (health == null)
            {
                return;
            }

            health.OnHealthChanged -= HandleHealthChanged;
            health.OnDied -= HandleDied;
            health.OnHealthChanged += HandleHealthChanged;
            health.OnDied += HandleDied;

            if (shield != null)
            {
                shield.OnShieldChanged -= HandleShieldChanged;
                shield.OnShieldBroken -= HandleShieldBroken;
                shield.OnShieldChanged += HandleShieldChanged;
                shield.OnShieldBroken += HandleShieldBroken;
            }
        }

        private void Unsubscribe()
        {
            if (health != null)
            {
                health.OnHealthChanged -= HandleHealthChanged;
                health.OnDied -= HandleDied;
            }

            if (shield != null)
            {
                shield.OnShieldChanged -= HandleShieldChanged;
                shield.OnShieldBroken -= HandleShieldBroken;
            }
        }

        private void HandleHealthChanged(float current, float max)
        {
            Refresh(current, max);
        }

        private void HandleDied()
        {
            Refresh(0f, health != null ? health.MaxHealth : 0f);
        }

        private void HandleShieldChanged(float current, float max)
        {
            RefreshShieldOverlay(current, max);
        }

        private void HandleShieldBroken()
        {
            RefreshShieldOverlay(0f, shield != null ? shield.MaxShield : 0f);
        }

        private void Refresh()
        {
            if (health == null)
            {
                SetRootActive(false);
                return;
            }

            Refresh(health.CurrentHealth, health.MaxHealth);
        }

        private void Refresh(float current, float max)
        {
            float normalized = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            bool isAlive = current > 0f;

            if (fillImage != null)
            {
                fillImage.fillAmount = normalized;
            }

            if (valueText != null)
            {
                valueText.gameObject.SetActive(showValueText && (!hideWhenDead || isAlive));
                valueText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }

            RefreshShieldOverlay();
            SetRootActive(!hideWhenDead || isAlive);
        }

        private void RefreshShieldOverlay()
        {
            if (shield == null)
            {
                SetShieldOverlayActive(false);
                return;
            }

            RefreshShieldOverlay(shield.CurrentShield, shield.MaxShield);
        }

        private void RefreshShieldOverlay(float current, float max)
        {
            bool hasOverlay = shieldOverlayFill != null;
            bool hasShield = current > 0f && max > 0f;

            if (shieldOverlayFill != null)
            {
                shieldOverlayFill.fillAmount = hasShield ? Mathf.Clamp01(current / max) : 0f;
            }

            SetShieldOverlayActive(hasOverlay && hasShield);
        }

        private void SetShieldOverlayActive(bool active)
        {
            GameObject target = shieldOverlayRoot != null
                ? shieldOverlayRoot
                : shieldOverlayFill != null
                    ? shieldOverlayFill.gameObject
                    : null;

            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
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
