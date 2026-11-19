using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public sealed class Projectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField, Min(0f)]
    private float damage = 10f;

    [SerializeField, Min(0.01f)]
    private float lifetime = 5f;

    [Header("Hit")]
    [SerializeField]
    private bool destroyOnHit = true;

    private Rigidbody2D projectileRigidbody;
    private float remainingLifetime;
    private bool isTerminated;

    private void Awake()
    {
        projectileRigidbody =
            GetComponent<Rigidbody2D>();

        if (projectileRigidbody == null)
        {
            Debug.LogError(
                "Projectile 初始化失敗，找不到 Rigidbody2D。",
                this);

            enabled = false;
        }
    }

    private void OnEnable()
    {
        remainingLifetime = lifetime;
        isTerminated = false;
    }

    private void Update()
    {
        if (isTerminated)
        {
            return;
        }

        remainingLifetime -= Time.deltaTime;

        if (remainingLifetime > 0f)
        {
            return;
        }

        Terminate();
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        TryHit(other);
    }

    private void OnCollisionEnter2D(
        Collision2D collision)
    {
        TryHit(collision.collider);
    }

    private void TryHit(
        Collider2D targetCollider)
    {
        if (isTerminated)
        {
            return;
        }

        if (targetCollider == null)
        {
            return;
        }

        EnemyUnit enemyUnit =
            targetCollider.GetComponentInParent<EnemyUnit>();

        if (enemyUnit == null ||
            enemyUnit.IsDead)
        {
            return;
        }

        float actualDamage =
            enemyUnit.TakeDamage(damage);

        if (actualDamage <= 0f)
        {
            return;
        }

        Debug.Log(
            $"投射物對 {enemyUnit.name} 造成 {actualDamage} 點傷害。",
            this);

        if (destroyOnHit)
        {
            Terminate();
        }
    }

    private void Terminate()
    {
        if (isTerminated)
        {
            return;
        }

        isTerminated = true;

        if (projectileRigidbody != null)
        {
            projectileRigidbody.linearVelocity =
                Vector2.zero;

            projectileRigidbody.angularVelocity = 0f;
        }

        Destroy(gameObject);
    }
}