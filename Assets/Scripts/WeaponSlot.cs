using UnityEngine;

[DisallowMultipleComponent]
public sealed class WeaponSlot : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField]
    private Projectile projectileTemplate;

    [SerializeField]
    private Transform firePoint;

    [SerializeField, Min(0.1f)]
    private float projectileSpeed = 10f;

    [Header("Targeting")]
    [SerializeField, Min(0f)]
    private float attackRange = 10f;

    [SerializeField]
    private string enemyTag = "Enemy";

    [Header("Attack")]
    [SerializeField, Min(0.01f)]
    private float attackInterval = 0.5f;

    private float nextAttackTime;

    private void Awake()
    {
        ValidateConfiguration();
    }

    private void Update()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        EnemyUnit target = FindNearestTarget();

        if (target == null)
        {
            return;
        }

        Fire(target);

        nextAttackTime =
            Time.time + attackInterval;
    }

    private EnemyUnit FindNearestTarget()
    {
        Collider2D[] colliders =
            Physics2D.OverlapCircleAll(
                transform.position,
                attackRange);

        EnemyUnit nearestTarget = null;
        float nearestDistanceSqr =
            float.MaxValue;

        foreach (Collider2D targetCollider in colliders)
        {
            EnemyUnit enemyUnit =
                targetCollider.GetComponentInParent<EnemyUnit>();

            if (enemyUnit == null)
            {
                continue;
            }

            if (enemyUnit.IsDead)
            {
                continue;
            }

            if (!enemyUnit.CompareTag(enemyTag))
            {
                continue;
            }

            Vector2 offset =
                (Vector2)enemyUnit.Transform.position -
                (Vector2)transform.position;

            float distanceSqr =
                offset.sqrMagnitude;

            if (distanceSqr >= nearestDistanceSqr)
            {
                continue;
            }

            nearestDistanceSqr =
                distanceSqr;

            nearestTarget =
                enemyUnit;
        }

        return nearestTarget;
    }

    private void Fire(
        EnemyUnit target)
    {
        if (projectileTemplate == null)
        {
            Debug.LogError(
                "WeaponSlot 無法射擊，尚未設定 Projectile Template。",
                this);

            return;
        }

        Vector2 spawnPosition =
            GetSpawnPosition();

        Vector2 direction =
            (Vector2)target.Transform.position -
            spawnPosition;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        direction.Normalize();

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x) *
            Mathf.Rad2Deg;

        Quaternion rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle);

        Projectile projectile =
            Instantiate(
                projectileTemplate,
                spawnPosition,
                rotation);

        projectile.gameObject.SetActive(true);

        if (!projectile.TryGetComponent(
                out Rigidbody2D projectileRigidbody))
        {
            Debug.LogError(
                $"投射物 {projectile.name} 沒有 Rigidbody2D。",
                projectile);

            Destroy(projectile.gameObject);
            return;
        }

        projectileRigidbody.linearVelocity =
            direction * projectileSpeed;
    }

    private Vector2 GetSpawnPosition()
    {
        if (firePoint != null)
        {
            return firePoint.position;
        }

        return projectileTemplate.transform.position;
    }

    private void ValidateConfiguration()
    {
        if (projectileTemplate == null)
        {
            Debug.LogWarning(
                "WeaponSlot 尚未設定 Projectile Template。",
                this);

            return;
        }

        if (!projectileTemplate.transform.IsChildOf(transform))
        {
            Debug.LogWarning(
                "Projectile Template 應掛載在 WeaponSlot 物件底下。",
                this);
        }

        if (!projectileTemplate.TryGetComponent<Rigidbody2D>(out _))
        {
            Debug.LogError(
                "Projectile Template 缺少 Rigidbody2D。",
                projectileTemplate);
        }

        if (!projectileTemplate.TryGetComponent<Collider2D>(out _))
        {
            Debug.LogError(
                "Projectile Template 缺少 Collider2D。",
                projectileTemplate);
        }

        projectileTemplate.gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange);
    }
#endif
}