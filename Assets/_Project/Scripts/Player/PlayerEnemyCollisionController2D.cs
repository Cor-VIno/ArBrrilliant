using JingHongLu.Combat;
using System.Collections.Generic;
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
        private readonly List<Collider2D> ignoredEnemyBodyColliders = new();
        private readonly List<ColliderPair> ignoredEnemyBodyPairs = new();
        private bool isIgnoringEnemyBodyCollision;
        private bool waitingForSeparation;
        private int playerLayer = -1;
        private int enemyLayer = -1;
        private Vector2 lastDashDirection = Vector2.right;

        private readonly struct ColliderPair
        {
            public ColliderPair(Collider2D first, Collider2D second)
            {
                First = first;
                Second = second;
            }

            public Collider2D First { get; }
            public Collider2D Second { get; }
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (!TryResolveAndValidateConfiguration())
            {
                enabled = false;
                return;
            }

            IgnoreEnemyEnemyBodyCollisions();
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

            IgnoreCurrentEnemyBodyColliders();

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

            RestoreAllIgnoredCollisions();
        }

        private void OnDestroy()
        {
            RestoreAllIgnoredCollisions();
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

        private bool TryResolveAndValidateConfiguration()
        {
            if (playerBodyCollider == null)
            {
                Debug.LogError("[PlayerEnemyCollision] Player body collider is missing.", this);
                return false;
            }

            if (playerRigidbody == null)
            {
                Debug.LogError("[PlayerEnemyCollision] Player Rigidbody2D is missing.", this);
                return false;
            }

            if (dashController == null)
            {
                Debug.LogError("[PlayerEnemyCollision] PlayerDashController2D is missing.", this);
                return false;
            }

            playerLayer = playerBodyCollider.gameObject.layer;

            if (!IsValidLayerIndex(playerLayer))
            {
                Debug.LogError(
                    $"[PlayerEnemyCollision] Player layer index is invalid: {playerLayer}.",
                    this);
                return false;
            }

            if (!TryResolveSingleLayer(enemyBodyLayer, out enemyLayer))
            {
                return false;
            }

            if (playerLayer == enemyLayer)
            {
                Debug.LogError(
                    $"[PlayerEnemyCollision] Player and enemy body layers must be different. Layer={playerLayer}.",
                    this);
                return false;
            }

            return true;
        }

        private bool TryResolveSingleLayer(LayerMask mask, out int layer)
        {
            int value = mask.value;
            layer = -1;

            if (value == 0)
            {
                Debug.LogError("[PlayerEnemyCollision] Enemy body layer mask is empty.", this);
                return false;
            }

            if ((value & (value - 1)) != 0)
            {
                Debug.LogError(
                    "[PlayerEnemyCollision] Enemy body layer mask must contain exactly one layer.",
                    this);
                return false;
            }

            for (int i = 0; i < 32; i++)
            {
                if ((value & (1 << i)) == 0)
                {
                    continue;
                }

                layer = i;
                break;
            }

            if (!IsValidLayerIndex(layer))
            {
                Debug.LogError(
                    $"[PlayerEnemyCollision] Enemy body layer index is invalid: {layer}.",
                    this);
                return false;
            }

            return true;
        }

        private static bool IsValidLayerIndex(int layer)
        {
            return layer >= 0 && layer < 32;
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
            if (!useLayerIgnore || !HasValidLayerIndices())
            {
                return;
            }

            IgnoreCurrentEnemyBodyColliders();
            isIgnoringEnemyBodyCollision = true;
            Log($"Player/Enemy body collision ignored for dash. Count={ignoredEnemyBodyColliders.Count}.");
        }

        private void RestorePlayerEnemyCollision()
        {
            if (!useLayerIgnore || !HasValidLayerIndices())
            {
                isIgnoringEnemyBodyCollision = false;
                return;
            }

            RestorePlayerEnemyBodyCollisions();
            isIgnoringEnemyBodyCollision = false;
            Log(
                $"Player/Enemy body collision restored. playerLayer={playerLayer} enemyLayer={enemyLayer} ignore=false.");
        }

        private void IgnoreCurrentEnemyBodyColliders()
        {
            if (playerBodyCollider == null)
            {
                return;
            }

            Collider2D[] colliders = FindObjectsByType<Collider2D>();

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D enemyBody = colliders[i];

                if (!IsEnemyBodyCollider(enemyBody) ||
                    ignoredEnemyBodyColliders.Contains(enemyBody))
                {
                    continue;
                }

                Physics2D.IgnoreCollision(playerBodyCollider, enemyBody, true);
                ignoredEnemyBodyColliders.Add(enemyBody);
            }
        }

        private void RestorePlayerEnemyBodyCollisions()
        {
            if (playerBodyCollider == null)
            {
                ignoredEnemyBodyColliders.Clear();
                return;
            }

            for (int i = 0; i < ignoredEnemyBodyColliders.Count; i++)
            {
                Collider2D enemyBody = ignoredEnemyBodyColliders[i];

                if (enemyBody == null)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(playerBodyCollider, enemyBody, false);
            }

            ignoredEnemyBodyColliders.Clear();
        }

        private void IgnoreEnemyEnemyBodyCollisions()
        {
            RestoreEnemyEnemyBodyCollisions();

            Collider2D[] colliders = FindObjectsByType<Collider2D>();
            List<Collider2D> enemyBodies = new();

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D collider = colliders[i];

                if (IsEnemyBodyCollider(collider))
                {
                    enemyBodies.Add(collider);
                }
            }

            for (int i = 0; i < enemyBodies.Count; i++)
            {
                for (int j = i + 1; j < enemyBodies.Count; j++)
                {
                    Collider2D first = enemyBodies[i];
                    Collider2D second = enemyBodies[j];

                    if (first == null || second == null)
                    {
                        continue;
                    }

                    Physics2D.IgnoreCollision(first, second, true);
                    ignoredEnemyBodyPairs.Add(new ColliderPair(first, second));
                }
            }

            Log($"Enemy/Enemy body collision ignored. PairCount={ignoredEnemyBodyPairs.Count}.");
        }

        private void RestoreEnemyEnemyBodyCollisions()
        {
            for (int i = 0; i < ignoredEnemyBodyPairs.Count; i++)
            {
                ColliderPair pair = ignoredEnemyBodyPairs[i];

                if (pair.First == null || pair.Second == null)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(pair.First, pair.Second, false);
            }

            ignoredEnemyBodyPairs.Clear();
        }

        private void RestoreAllIgnoredCollisions()
        {
            RestorePlayerEnemyBodyCollisions();
            RestoreEnemyEnemyBodyCollisions();
            isIgnoringEnemyBodyCollision = false;
            waitingForSeparation = false;
        }

        private bool HasValidLayerIndices()
        {
            return IsValidLayerIndex(playerLayer) && IsValidLayerIndex(enemyLayer);
        }

        private bool IsEnemyBodyCollider(Collider2D collider)
        {
            return collider != null &&
                collider != playerBodyCollider &&
                collider.gameObject.layer == enemyLayer &&
                !collider.isTrigger &&
                collider.GetComponentInParent<Hurtbox2D>() == null;
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

                if (!IsEnemyBodyCollider(hit))
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
