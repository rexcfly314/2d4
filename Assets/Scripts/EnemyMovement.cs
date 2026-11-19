using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyUnit))]
public sealed class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float moveSpeed = 3f;

    [SerializeField, Min(0f)]
    private float stopDistance = 0.5f;

    [Header("Rotation")]
    [SerializeField]
    private bool rotateTowardTarget = false;

    [SerializeField, Min(0f)]
    private float rotationSpeed = 360f;

    private Rigidbody2D enemyRigidbody;
    private EnemyUnit enemyUnit;
    private PlayerUnit playerUnit;
    private bool isRegistered;

    private void Awake()
    {
        enemyRigidbody =
            GetComponent<Rigidbody2D>();

        enemyUnit =
            GetComponent<EnemyUnit>();
    }

    private void OnEnable()
    {
        ResolvePlayer();
        RegisterToGameManager();
    }

    private void Start()
    {
        ResolvePlayer();
        RegisterToGameManager();
    }

    private void OnDisable()
    {
        UnregisterFromGameManager();
    }

    private void FixedUpdate()
    {
        if (!isRegistered)
        {
            RegisterToGameManager();
        }

        if (playerUnit == null)
        {
            ResolvePlayer();

            if (playerUnit == null)
            {
                StopMovement();
                return;
            }
        }

        if (playerUnit.IsDead)
        {
            StopMovement();
            return;
        }

        MoveTowardPlayer();
    }

    private void ResolvePlayer()
    {
        if (GameManager.Instance == null)
        {
            playerUnit = null;
            return;
        }

        playerUnit =
            GameManager.Instance.GetPlayer();
    }

    private void RegisterToGameManager()
    {
        if (isRegistered)
        {
            return;
        }

        if (GameManager.Instance == null ||
            enemyUnit == null)
        {
            return;
        }

        GameManager.Instance.RegisterEnemy(
            enemyUnit);

        isRegistered = true;
    }

    private void UnregisterFromGameManager()
    {
        if (!isRegistered)
        {
            return;
        }

        if (GameManager.Instance != null &&
            enemyUnit != null)
        {
            GameManager.Instance.UnregisterEnemy(
                enemyUnit);
        }

        isRegistered = false;
    }

    private void MoveTowardPlayer()
    {
        Vector2 direction =
            (Vector2)playerUnit.Transform.position -
            enemyRigidbody.position;

        if (direction.sqrMagnitude <=
            stopDistance * stopDistance)
        {
            StopMovement();
            return;
        }

        Vector2 normalizedDirection =
            direction.normalized;

        enemyRigidbody.linearVelocity =
            normalizedDirection * moveSpeed;

        if (rotateTowardTarget)
        {
            RotateTowardDirection(
                normalizedDirection);
        }
    }

    private void RotateTowardDirection(
        Vector2 direction)
    {
        float targetAngle =
            Mathf.Atan2(
                direction.y,
                direction.x) *
            Mathf.Rad2Deg;

        float newAngle =
            Mathf.MoveTowardsAngle(
                enemyRigidbody.rotation,
                targetAngle,
                rotationSpeed *
                Time.fixedDeltaTime);

        enemyRigidbody.MoveRotation(
            newAngle);
    }

    private void StopMovement()
    {
        enemyRigidbody.linearVelocity =
            Vector2.zero;
    }
}