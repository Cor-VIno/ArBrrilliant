using UnityEngine;

namespace JingHongLu.Player
{
    public sealed class PlayerAim2D : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera = null;
        [SerializeField] private Transform aimOrigin = null;

        public Vector2 MouseWorldPosition { get; private set; }
        public Vector2 AimDirection { get; private set; } = Vector2.right;
        public float AimAngleDegrees { get; private set; }
        public int AimFacingDirection { get; private set; } = 1;

        private void Awake()
        {
            ResolveCamera();

            if (aimOrigin == null)
            {
                aimOrigin = transform;
            }
        }

        private void Update()
        {
            UpdateMouseWorldPosition();
            UpdateAimDirection();
        }

        private void ResolveCamera()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void UpdateMouseWorldPosition()
        {
            ResolveCamera();

            if (targetCamera == null)
            {
                return;
            }

            Vector3 mouseScreenPosition = UnityEngine.Input.mousePosition;
            mouseScreenPosition.z = -targetCamera.transform.position.z;

            Vector3 worldPosition = targetCamera.ScreenToWorldPoint(mouseScreenPosition);
            MouseWorldPosition = new Vector2(worldPosition.x, worldPosition.y);
        }

        private void UpdateAimDirection()
        {
            Vector2 origin = aimOrigin != null
                ? aimOrigin.position
                : transform.position;
            Vector2 direction = MouseWorldPosition - origin;

            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            AimDirection = direction.normalized;
            AimAngleDegrees = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
            AimFacingDirection = AimDirection.x >= 0f ? 1 : -1;
        }
    }
}
