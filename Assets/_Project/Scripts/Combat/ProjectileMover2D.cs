using UnityEngine;

namespace JingHongLu.Combat
{
    public sealed class ProjectileMover2D : MonoBehaviour
    {
        [SerializeField] private ProjectileMotionType motionType = ProjectileMotionType.Linear;
        [SerializeField] private Vector2 velocity = Vector2.right;
        [SerializeField] private float gravity = 0f;
        [SerializeField] private float lifetime = 1.2f;
        [SerializeField] private bool rotateToVelocity = true;

        private float remainingLifetime;

        public void Initialize(
            ProjectileMotionType motionType,
            Vector2 direction,
            float speed,
            float lifetime,
            float gravity,
            bool rotateToVelocity)
        {
            this.motionType = motionType;

            Vector2 normalizedDirection = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : Vector2.right;

            velocity = normalizedDirection * Mathf.Max(0f, speed);
            this.lifetime = Mathf.Max(0.01f, lifetime);
            this.gravity = Mathf.Max(0f, gravity);
            this.rotateToVelocity = rotateToVelocity;
            remainingLifetime = this.lifetime;

            UpdateRotation();
        }

        private void Awake()
        {
            remainingLifetime = Mathf.Max(0.01f, lifetime);
            UpdateRotation();
        }

        private void Update()
        {
            UpdateVelocity();
            Move();
            UpdateRotation();
            TickLifetime();
        }

        private void UpdateVelocity()
        {
            if (motionType != ProjectileMotionType.Parabolic)
            {
                return;
            }

            velocity += Vector2.down * gravity * Time.deltaTime;
        }

        private void Move()
        {
            transform.position += (Vector3)(velocity * Time.deltaTime);
        }

        private void TickLifetime()
        {
            remainingLifetime -= Time.deltaTime;

            if (remainingLifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void UpdateRotation()
        {
            if (!rotateToVelocity || velocity.sqrMagnitude < 0.0001f)
            {
                return;
            }

            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
