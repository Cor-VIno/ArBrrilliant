using JingHongLu.Combat;
using UnityEngine;

namespace JingHongLu.Player
{
    public sealed class PlayerEnemyCollisionController2D : MonoBehaviour
    {
        [SerializeField] private Collider2D playerBodyCollider;
        [SerializeField] private Rigidbody2D playerRigidbody;
        [SerializeField] private PlayerDashController2D dashController;
        [SerializeField] private LayerMask enemyBodyLayer;
        [SerializeField] private bool useLayerIgnore = true;
        [SerializeField] private float safePointStep = 0.1f;
        [SerializeField] private int maxSafePointSteps = 12;
        [SerializeField] private bool logCollisionDebug;

        private readonly Collider2D[] overlapResults = new Collider2D[16];
        private bool isIgnoringEnemyBodyCollision;
        private bool waitingForSeparation;
        private bool storedPlayerEnemyIgnore;
        private bool storedEnemyEnemyIgnore;
        private bool hasStoredLayerState;
        private int playerLayer = -1;
        private int enemyLayer = -1;
        private Vector2 lastDashDirection = Vector2.right;

        private void Awake()
        {
            ResolveReferences();
            ResolveLayers();
        }

        private void OnEnable()
        {
            ResolveReferences();
            ResolveLayers();
            StoreLayerState();
            SetEnemyEnemyCollisionIgnored(true);
            RestorePlayerEnemyCollision();

            if (dashController != null)
            {
                dashController.OnDashStarted += HandleDashStarted;
                dashController.OnDashFinished += HandleDashFinished;
            }
        }

        private void Update()
        {
            if (!waitingForSeparation)
            {
                return;
            }

            if (IsPlayerOverlappingEnemyAt(GetPlayerBodyPosition()))
            {
                return;
            }

            waitingForSeparation = false;
            RestorePlayerEnemyCollision();
            Log("Restored collision after separation.");
        }

        private void OnDisable()
        {
            if (dashController != null)
            {
                dashController.OnDashStarted -= HandleDashStarted;
                dashController.OnDashFinished -= HandleDashFinished;
            }

            waitingForSeparation = false;
            RestoreStoredLayerState();
        }

        private void OnDestroy()
        {
            RestoreStoredLayerState();
        }

        private void ResolveReferences()
        {
            if (playerBodyCollider == null)
            {
                playerBodyCollider = GetComponent<Collider2D>();
            }

            if (playerRigidbody == null)
            {
                TryGetComponent(out playerRigidbody);
            }

            if (dashController == null)
            {
                TryGetComponent(out dashController);
            }
        }

        private void ResolveLayers()
        {
            playerLayer = playerBodyCollider != null
                ? playerBodyCollider.gameObject.layer
                : gameObject.layer;

            enemyLayer = ResolveSingleLayer(enemyBodyLayer);

            if (enemyLayer < 0)
            {
                enemyLayer = LayerMask.NameToLayer("Enemy");
            }
        }

        private static int ResolveSingleLayer(LayerMask mask)
        {
            int value = mask.value;

            if (value == 0)
            {
                return -1;
            }

            for (int i = 0; i < 32; i++)
            {
                if ((value & (1 << i)) != 0)
                {
                    return i;
                }
            }

            return -1;
        }

        private void StoreLayerState()
        {
            if (hasStoredLayerState || playerLayer < 0 || enemyLayer < 0)
            {
                return;
            }

            storedPlayerEnemyIgnore =
                Physics2D.GetIgnoreLayerCollision(playerLayer, enemyLayer);
            storedEnemyEnemyIgnore =
                Physics2D.GetIgnoreLayerCollision(enemyLayer, enemyLayer);
            hasStoredLayerState = true;
        }

        private void HandleDashStarted(Vector2 direction)
        {
            lastDashDirection = direction.sqrMagnitude > 0.0001f
                ? new Vector2(Mathf.Sign(direction.x == 0f ? 1f : direction.x), 0f)
                : Vector2.right;
            waitingForSeparation = false;
            BeginIgnorePlayerEnemyCollision();
        }

        private void HandleDashFinished()
        {
            if (!isIgnoringEnemyBodyCollision)
            {
                return;
            }

            Vector2 currentPosition = GetPlayerBodyPosition();

            if (!IsPlayerOverlappingEnemyAt(currentPosition))
            {
                RestorePlayerEnemyCollision();
                return;
            }

            if (TryMoveToSafePoint(currentPosition))
            {
                RestorePlayerEnemyCollision();
                return;
            }

            waitingForSeparation = true;
            Log("Safe point search failed. Waiting for separation.");
        }

        private void BeginIgnorePlayerEnemyCollision()
        {
            if (!useLayerIgnore || playerLayer < 0 || enemyLayer < 0)
            {
                return;
            }

            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
            isIgnoringEnemyBodyCollision = true;
            Log("Player/Enemy body collision ignored for dash.");
        }

        private void RestorePlayerEnemyCollision()
        {
            if (!useLayerIgnore || playerLayer < 0 || enemyLayer < 0)
            {
                isIgnoringEnemyBodyCollision = false;
                return;
            }

            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, storedPlayerEnemyIgnore);
            isIgnoringEnemyBodyCollision = storedPlayerEnemyIgnore;
            Log("Player/Enemy body collision restored.");
        }

        private void SetEnemyEnemyCollisionIgnored(bool ignored)
        {
            if (enemyLayer < 0)
            {
                return;
            }

            Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, ignored);
            Log("Enemy/Enemy body collision ignored.");
        }

        private void RestoreStoredLayerState()
        {
            if (!hasStoredLayerState)
            {
                return;
            }

            if (playerLayer >= 0 && enemyLayer >= 0)
            {
                Physics2D.IgnoreLayerCollision(
                    playerLayer,
                    enemyLayer,
                    storedPlayerEnemyIgnore);
            }

            if (enemyLayer >= 0)
            {
                Physics2D.IgnoreLayerCollision(
                    enemyLayer,
                    enemyLayer,
                    storedEnemyEnemyIgnore);
            }

            isIgnoringEnemyBodyCollision = false;
            waitingForSeparation = false;
        }

        private bool TryMoveToSafePoint(Vector2 currentPosition)
        {
            Vector2 dashDirection = lastDashDirection.sqrMagnitude > 0.0001f
                ? lastDashDirection.normalized
                : Vector2.right;

            for (int i = 1; i <= Mathf.Max(1, maxSafePointSteps); i++)
            {
                Vector2 candidate = currentPosition +
                    dashDirection * (Mathf.Max(0.01f, safePointStep) * i);

                if (IsPlayerOverlappingEnemyAt(candidate))
                {
                    continue;
                }

                MovePlayerBody(candidate);
                Log($"Moved player to safe dash endpoint: {candidate}.");
                return true;
            }

            return false;
        }

        private Vector2 GetPlayerBodyPosition()
        {
            if (playerRigidbody != null)
            {
                return playerRigidbody.position;
            }

            return transform.position;
        }

        private void MovePlayerBody(Vector2 position)
        {
            if (playerRigidbody != null)
            {
                playerRigidbody.position = position;
                Physics2D.SyncTransforms();
                return;
            }

            transform.position = new Vector3(position.x, position.y, transform.position.z);
            Physics2D.SyncTransforms();
        }

        private bool IsPlayerOverlappingEnemyAt(Vector2 bodyPosition)
        {
            if (playerBodyCollider == null || enemyBodyLayer.value == 0)
            {
                return false;
            }

            Bounds bounds = playerBodyCollider.bounds;
            Vector2 bodyCenter = playerRigidbody != null
                ? playerRigidbody.position
                : (Vector2)transform.position;
            Vector2 centerOffset = (Vector2)bounds.center - bodyCenter;
            Vector2 queryCenter = bodyPosition + centerOffset;
            Vector2 querySize = bounds.size;
            float angle = playerBodyCollider.transform.eulerAngles.z;

            ContactFilter2D contactFilter = new ContactFilter2D
            {
                useLayerMask = true,
                useTriggers = false
            };
            contactFilter.SetLayerMask(enemyBodyLayer);

            int count = Physics2D.OverlapBox(
                queryCenter,
                querySize,
                angle,
                contactFilter,
                overlapResults);

            for (int i = 0; i < count; i++)
            {
                Collider2D hit = overlapResults[i];

                if (hit == null ||
                    hit == playerBodyCollider ||
                    hit.isTrigger ||
                    hit.GetComponentInParent<Hurtbox2D>() != null)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private void Log(string message)
        {
            if (logCollisionDebug)
            {
                Debug.Log($"[PlayerEnemyCollision] {message}", this);
            }
        }
    }
}
