using JingHongLu.Input;
using UnityEngine;

namespace JingHongLu.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMotor2D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Collider2D bodyCollider;

        [Header("Movement")]
        [SerializeField] private float maxMoveSpeed = 7f;
        [SerializeField] private float groundAcceleration = 80f;
        [SerializeField] private float groundDeceleration = 90f;
        [SerializeField] private float airAcceleration = 45f;

        [Header("Jump")]
        [SerializeField] private float jumpVelocity = 14f;
        [SerializeField] private float jumpBufferTime = 0.12f;
        [SerializeField] private float coyoteTime = 0.1f;
        [Range(0f, 1f)]
        [SerializeField] private float jumpCutMultiplier = 0.45f;
        [SerializeField] private float fallGravityMultiplier = 1.8f;
        [SerializeField] private float maxFallSpeed = 22f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundMask = Physics2D.DefaultRaycastLayers;
        [SerializeField] private float groundCheckDistance = 0.08f;
        [Range(0.1f, 1f)]
        [SerializeField] private float groundCheckWidthScale = 0.85f;

        private readonly Collider2D[] groundCheckHits = new Collider2D[8];

        private Rigidbody2D body;
        private ContactFilter2D groundContactFilter;
        private bool isGrounded;
        private bool wasJumpHeld;
        private bool jumpCutRequested;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private Vector2 groundCheckCenter;
        private Vector2 groundCheckSize;

        public int FacingDirection { get; private set; } = 1;
        public bool IsExternalMotionActive { get; private set; }
        public bool IsGrounded => isGrounded;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();

            if (inputReader == null)
            {
                TryGetComponent(out inputReader);
            }

            if (bodyCollider == null)
            {
                TryGetComponent(out bodyCollider);
            }

            if (visualRoot == null)
            {
                visualRoot = transform;
            }

            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            groundContactFilter.useLayerMask = true;
            groundContactFilter.useTriggers = false;
            groundContactFilter.SetLayerMask(groundMask);
        }

        private void Update()
        {
            UpdateGroundedState();
            UpdateJumpTimers();
            CacheJumpCutRequest();
            UpdateFacingDirection();
        }

        private void FixedUpdate()
        {
            if (IsExternalMotionActive)
            {
                return;
            }

            MoveHorizontally();
            TryJump();
            ApplyJumpCut();
            ApplyFallGravity();
            ClampFallSpeed();
        }

        public void BeginExternalMotion()
        {
            IsExternalMotionActive = true;
        }

        public void EndExternalMotion()
        {
            IsExternalMotionActive = false;
        }

        public void SetExternalVelocity(Vector2 velocity)
        {
            body.linearVelocity = velocity;
        }

        private void UpdateGroundedState()
        {
            groundContactFilter.SetLayerMask(groundMask);
            CalculateGroundCheckBounds();

            int hitCount = Physics2D.OverlapBox(
                groundCheckCenter,
                groundCheckSize,
                0f,
                groundContactFilter,
                groundCheckHits);

            isGrounded = false;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = groundCheckHits[i];

                if (hit != null && !IsOwnCollider(hit))
                {
                    isGrounded = true;
                    break;
                }
            }

            for (int i = 0; i < hitCount; i++)
            {
                groundCheckHits[i] = null;
            }
        }

        private void UpdateJumpTimers()
        {
            if (isGrounded)
            {
                coyoteTimer = coyoteTime;
            }
            else
            {
                coyoteTimer -= Time.deltaTime;
            }

            if (inputReader != null && inputReader.JumpPressed)
            {
                jumpBufferTimer = jumpBufferTime;
            }
            else
            {
                jumpBufferTimer -= Time.deltaTime;
            }
        }

        private void CacheJumpCutRequest()
        {
            bool jumpHeld = inputReader != null && inputReader.JumpHeld;

            if (wasJumpHeld && !jumpHeld && body.linearVelocity.y > 0f)
            {
                jumpCutRequested = true;
            }

            wasJumpHeld = jumpHeld;
        }

        private void UpdateFacingDirection()
        {
            if (inputReader == null)
            {
                return;
            }

            float horizontalInput = inputReader.MoveInput.x;

            if (Mathf.Approximately(horizontalInput, 0f))
            {
                return;
            }

            FacingDirection = horizontalInput > 0f ? 1 : -1;

            if (visualRoot == null)
            {
                return;
            }

            Vector3 scale = visualRoot.localScale;
            float absX = Mathf.Abs(scale.x);
            scale.x = FacingDirection > 0 ? absX : -absX;
            visualRoot.localScale = scale;
        }

        private void MoveHorizontally()
        {
            float horizontalInput = inputReader != null ? inputReader.MoveInput.x : 0f;
            float targetSpeed = horizontalInput * maxMoveSpeed;
            float acceleration = GetHorizontalAcceleration(targetSpeed);

            Vector2 velocity = body.linearVelocity;
            velocity.x = Mathf.MoveTowards(
                velocity.x,
                targetSpeed,
                acceleration * Time.fixedDeltaTime);
            body.linearVelocity = velocity;
        }

        private void TryJump()
        {
            if (jumpBufferTimer <= 0f || coyoteTimer <= 0f)
            {
                return;
            }

            Vector2 velocity = body.linearVelocity;
            velocity.y = jumpVelocity;
            body.linearVelocity = velocity;

            isGrounded = false;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
            jumpCutRequested = inputReader != null && !inputReader.JumpHeld;
        }

        private void ApplyJumpCut()
        {
            if (!jumpCutRequested)
            {
                return;
            }

            Vector2 velocity = body.linearVelocity;

            if (velocity.y > 0f)
            {
                velocity.y *= jumpCutMultiplier;
                body.linearVelocity = velocity;
            }

            jumpCutRequested = false;
        }

        private void ApplyFallGravity()
        {
            if (fallGravityMultiplier <= 1f)
            {
                return;
            }

            Vector2 velocity = body.linearVelocity;

            if (velocity.y >= 0f)
            {
                return;
            }

            velocity.y += Physics2D.gravity.y
                * body.gravityScale
                * (fallGravityMultiplier - 1f)
                * Time.fixedDeltaTime;
            body.linearVelocity = velocity;
        }

        private void ClampFallSpeed()
        {
            Vector2 velocity = body.linearVelocity;

            if (velocity.y >= -maxFallSpeed)
            {
                return;
            }

            velocity.y = -maxFallSpeed;
            body.linearVelocity = velocity;
        }

        private float GetHorizontalAcceleration(float targetSpeed)
        {
            if (!isGrounded)
            {
                return airAcceleration;
            }

            return Mathf.Approximately(targetSpeed, 0f)
                ? groundDeceleration
                : groundAcceleration;
        }

        private void CalculateGroundCheckBounds()
        {
            float distance = Mathf.Max(0.01f, groundCheckDistance);

            if (bodyCollider == null)
            {
                groundCheckCenter = (Vector2)transform.position + Vector2.down * (distance * 0.5f);
                groundCheckSize = new Vector2(0.8f * groundCheckWidthScale, distance);
                return;
            }

            Bounds bounds = bodyCollider.bounds;
            groundCheckCenter = new Vector2(bounds.center.x, bounds.min.y - distance * 0.5f);
            groundCheckSize = new Vector2(bounds.size.x * groundCheckWidthScale, distance);
        }

        private bool IsOwnCollider(Collider2D hit)
        {
            return hit == bodyCollider
                || hit.attachedRigidbody == body
                || hit.transform == transform
                || hit.transform.IsChildOf(transform);
        }

        private void OnDrawGizmosSelected()
        {
            CalculateGroundCheckBounds();

            Gizmos.color = isGrounded ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(groundCheckCenter, groundCheckSize);
        }
    }
}
