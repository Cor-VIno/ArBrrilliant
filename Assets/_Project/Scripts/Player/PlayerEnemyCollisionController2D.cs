using UnityEngine;

namespace JingHongLu.Player
{
    public sealed class PlayerEnemyCollisionController2D : MonoBehaviour
    {
        [SerializeField] private Collider2D playerBodyCollider;
        [SerializeField] private Rigidbody2D playerRigidbody;
        [SerializeField] private PlayerDashController2D dashController;
        [SerializeField] private LayerMask enemyBodyLayer = 1 << 8;
        [SerializeField] private bool useLayerIgnore = true;
        [SerializeField] private float safePointStep = 0.1f;
        [SerializeField] private int maxSafePointSteps = 12;
        [SerializeField] private float enemyBodyMass = 10000f;
        [SerializeField] private bool logCollisionDebug;

        private readonly Collider2D[] overlapResults = new Collider2D[16];
        private bool isIgnoringEnemyBodyCollision;
        private bool waitingForSeparation;
        private bool previousLayerCollisionIgnored;
        private Vector2 lastDashDirection = Vector2.right;
        private int playerLayer = -1;
        private int enemyLayer = -1;
        private static bool layerCollisionConfigured;
        private static bool enemyBodiesConfigured;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureControllersInScene()
        {
            PlayerDashController2D[] dashControllers =
                FindObjectsByType<PlayerDashController2D>(
                    FindObjectsInactive.Exclude);

            foreach (PlayerDashController2D dash in dashControllers)
            {
                if (dash == null ||
                    dash.GetComponent<PlayerEnemyCollisionController2D>() != null)
                {
                    continue;
                }

                dash.gameObject.AddComponent<PlayerEnemyCollisionController2D>();
            }
        }

        private void Awake()
        {
            ResolveReferences();
            ResolveLayers();
        }

        private void OnEnable()
        {
            ResolveReferences();
            ResolveLayers();

            if (dashController != null)
            {
                dashController.OnDashStarted += HandleDashStarted;
                dashController.OnDashFinished += HandleDashFinished;
            }
        }

        private void OnDisable()
        {
            if (dashController != null)
            {
                dashController.OnDashStarted -= HandleDashStarted;
                dashController.OnDashFinished -= HandleDashFinished;
            }

            RestoreEnemyBodyCollision();
            waitingForSeparation = false;
        }

        private void OnDestroy()
        {
            RestoreEnemyBodyCollision();
        }

        private void LateUpdate()
        {
            if (!waitingForSeparation)
            {
                return;
            }

            if (IsOverlappingEnemyAt(GetCurrentRootPosition()))
            {
                return;
            }

            waitingForSeparation = false;
            RestoreEnemyBodyCollision();
            Log("Restored collision after player separated from enemy body.");
        }

        private void HandleDashStarted(Vector2 direction)
        {
            lastDashDirection = direction.sqrMagnitude > 0.0001f
                ? new Vector2(Mathf.Sign(direction.x == 0f ? 1f : direction.x), 0f)
                : Vector2.right;

            waitingForSeparation = false;
            IgnoreEnemyBodyCollision();
        }

        private void HandleDashFinished()
        {
            if (!isIgnoringEnemyBodyCollision)
            {
                return;
            }

            if (!IsOverlappingEnemyAt(GetCurrentRootPosition()))
            {
                RestoreEnemyBodyCollision();
                return;
            }

            if (TryMoveToNearestSafePoint())
            {
                RestoreEnemyBodyCollision();
                return;
            }

            waitingForSeparation = true;
            Log("Safe point search failed. Keeping enemy body collision ignored until separation.");
        }

        private void IgnoreEnemyBodyCollision()
        {
            if (isIgnoringEnemyBodyCollision)
            {
                return;
            }

            ResolveLayers();

            if (useLayerIgnore && playerLayer >= 0 && enemyLayer >= 0)
            {
                previousLayerCollisionIgnored =
                    Physics2D.GetIgnoreLayerCollision(playerLayer, enemyLayer);
                Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
                isIgnoringEnemyBodyCollision = true;
                Log("PlayerBody vs EnemyBody collision ignored for dash.");
            }
        }

        private void RestoreEnemyBodyCollision()
        {
            if (!isIgnoringEnemyBodyCollision)
            {
                return;
            }

            if (useLayerIgnore && playerLayer >= 0 && enemyLayer >= 0)
            {
                Physics2D.IgnoreLayerCollision(
                    playerLayer,
                    enemyLayer,
                    previousLayerCollisionIgnored);
            }

            isIgnoringEnemyBodyCollision = false;
            Log("PlayerBody vs EnemyBody collision restored.");
        }

        private bool TryMoveToNearestSafePoint()
        {
            Vector2 direction = lastDashDirection.sqrMagnitude > 0.0001f
                ? lastDashDirection.normalized
                : Vector2.right;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector2.right;
            }

            Vector2 currentPosition = GetCurrentRootPosition();
            float step = Mathf.Max(0.01f, safePointStep);
            int steps = Mathf.Max(1, maxSafePointSteps);

            for (int i = 1; i <= steps; i++)
            {
                Vector2 candidate = currentPosition + direction * (step * i);

                if (IsOverlappingEnemyAt(candidate))
                {
                    continue;
                }

                MovePlayerTo(candidate);
                Log($"Moved player to dash safe point. Offset={step * i:0.###}");
                return true;
            }

            return false;
        }

        private bool IsOverlappingEnemyAt(Vector2 rootPosition)
        {
            if (playerBodyCollider == null)
            {
                return false;
            }

            Bounds bounds = playerBodyCollider.bounds;
            Vector2 currentRoot = GetCurrentRootPosition();
            Vector2 centerOffset = (Vector2)bounds.center - currentRoot;
            Vector2 checkCenter = rootPosition + centerOffset;
            Vector2 checkSize = bounds.size;

            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(enemyBodyLayer);
            contactFilter.useTriggers = false;

            int hitCount = Physics2D.OverlapBox(
                checkCenter,
                checkSize,
                0f,
                contactFilter,
                overlapResults);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = overlapResults[i];

                if (hit == null ||
                    hit == playerBodyCollider ||
                    hit.isTrigger)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private Vector2 GetCurrentRootPosition()
        {
            return playerRigidbody != null
                ? playerRigidbody.position
                : (Vector2)transform.position;
        }

        private void MovePlayerTo(Vector2 position)
        {
            if (playerRigidbody != null)
            {
                playerRigidbody.position = position;
                playerRigidbody.linearVelocity = new Vector2(
                    0f,
                    playerRigidbody.linearVelocity.y);
                return;
            }

            transform.position = new Vector3(
                position.x,
                position.y,
                transform.position.z);
        }

        private void ResolveReferences()
        {
            if (playerBodyCollider == null)
            {
                TryGetComponent(out playerBodyCollider);
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
            if (playerBodyCollider != null)
            {
                playerLayer = playerBodyCollider.gameObject.layer;
            }
            else
            {
                playerLayer = gameObject.layer;
            }

            enemyLayer = LayerMaskToSingleLayer(enemyBodyLayer);

            if (enemyLayer < 0)
            {
                enemyLayer = LayerMask.NameToLayer("Enemy");
            }

            if (enemyBodyLayer.value == 0 && enemyLayer >= 0)
            {
                enemyBodyLayer = 1 << enemyLayer;
            }

            ConfigureLayerCollisionRules();
        }

        private void ConfigureLayerCollisionRules()
        {
            if (layerCollisionConfigured || playerLayer < 0 || enemyLayer < 0)
            {
                return;
            }

            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
            Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
            ConfigureEnemyBodyMass(enemyLayer);
            layerCollisionConfigured = true;
        }

        private void ConfigureEnemyBodyMass(int targetEnemyLayer)
        {
            if (enemyBodiesConfigured)
            {
                return;
            }

            Rigidbody2D[] bodies = FindObjectsByType<Rigidbody2D>(
                FindObjectsInactive.Exclude);

            foreach (Rigidbody2D body in bodies)
            {
                if (body == null || body.gameObject.layer != targetEnemyLayer)
                {
                    continue;
                }

                body.mass = Mathf.Max(body.mass, Mathf.Max(1f, enemyBodyMass));
            }

            enemyBodiesConfigured = true;
        }

        private static int LayerMaskToSingleLayer(LayerMask mask)
        {
            int value = mask.value;

            if (value == 0 || (value & (value - 1)) != 0)
            {
                return -1;
            }

            int layer = 0;
            while (value > 1)
            {
                value >>= 1;
                layer++;
            }

            return layer;
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
