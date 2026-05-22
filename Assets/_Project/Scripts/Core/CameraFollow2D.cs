using UnityEngine;

namespace JingHongLu.Core
{
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private bool followX = true;
        [SerializeField] private bool followY;
        [SerializeField] private float fixedY;
        [SerializeField] private float fixedZ = -10f;
        [SerializeField] private float smoothTime = 0.15f;
        [SerializeField] private bool useBounds = true;
        [SerializeField] private float minX = -10f;
        [SerializeField] private float maxX = 10f;
        [SerializeField] private float minY = -5f;
        [SerializeField] private float maxY = 5f;
        [SerializeField] private bool logCameraFollow;

        private Vector3 velocity;
        private bool hasWarnedMissingTarget;

        public Transform Target
        {
            get => target;
            set
            {
                target = value;
                hasWarnedMissingTarget = false;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                if (logCameraFollow && !hasWarnedMissingTarget)
                {
                    Debug.LogWarning("CameraFollow2D target is missing.", this);
                    hasWarnedMissingTarget = true;
                }

                return;
            }

            Vector3 currentPosition = transform.position;
            float targetX = followX ? target.position.x : currentPosition.x;
            float targetY = followY ? target.position.y : fixedY;

            if (useBounds)
            {
                targetX = Mathf.Clamp(targetX, minX, maxX);

                if (followY)
                {
                    targetY = Mathf.Clamp(targetY, minY, maxY);
                }
            }

            Vector3 desiredPosition = new Vector3(targetX, targetY, fixedZ);

            if (smoothTime <= 0f)
            {
                transform.position = desiredPosition;
                velocity = Vector3.zero;
                return;
            }

            transform.position = Vector3.SmoothDamp(
                currentPosition,
                desiredPosition,
                ref velocity,
                smoothTime);
        }

        private void OnDrawGizmosSelected()
        {
            if (!useBounds)
            {
                return;
            }

            Gizmos.color = Color.cyan;

            float centerY = followY
                ? (minY + maxY) * 0.5f
                : fixedY;
            float height = followY
                ? Mathf.Max(0.1f, maxY - minY)
                : 10f;

            Vector3 leftBottom = new Vector3(minX, centerY - height * 0.5f, fixedZ);
            Vector3 leftTop = new Vector3(minX, centerY + height * 0.5f, fixedZ);
            Vector3 rightBottom = new Vector3(maxX, centerY - height * 0.5f, fixedZ);
            Vector3 rightTop = new Vector3(maxX, centerY + height * 0.5f, fixedZ);

            Gizmos.DrawLine(leftBottom, leftTop);
            Gizmos.DrawLine(rightBottom, rightTop);

            if (!followY)
            {
                return;
            }

            Gizmos.DrawLine(leftBottom, rightBottom);
            Gizmos.DrawLine(leftTop, rightTop);
        }
    }
}
