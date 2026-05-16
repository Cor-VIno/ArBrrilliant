using JingHongLu.Player;
using UnityEngine;

namespace JingHongLu.Visuals
{
    public sealed class PlayerLocomotionAnimationBinder : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private PlayerMotor2D motor;
        [SerializeField] private Animator animator;
        [SerializeField] private float moveThreshold = 0.05f;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Update()
        {
            if (body == null || animator == null)
            {
                ResolveReferences();
            }

            if (body == null || animator == null)
            {
                return;
            }

            float horizontalSpeed = Mathf.Abs(body.linearVelocity.x);
            bool isMoving = horizontalSpeed > moveThreshold;

            animator.SetBool("IsMoving", isMoving);
            animator.SetFloat("MoveSpeed", horizontalSpeed);
            animator.SetFloat("VerticalSpeed", body.linearVelocity.y);

            if (motor != null)
            {
                animator.SetBool("IsGrounded", motor.IsGrounded);
            }
        }

        private void ResolveReferences()
        {
            if (body == null)
            {
                TryGetComponent(out body);
            }

            if (motor == null)
            {
                TryGetComponent(out motor);
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }
    }
}
