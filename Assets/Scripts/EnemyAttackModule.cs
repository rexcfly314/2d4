using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class EnemyAttackModule : MonoBehaviour
{
    [Header("攻擊")]
    [SerializeField, Min(0f)] private float contactDamage = 10f;
    [SerializeField, Min(0.01f)] private float attackInterval = 1f;

    private float nextAttackTime;

    private void OnEnable()
    {
        nextAttackTime = Time.time;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryAttack(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryAttack(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryAttack(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryAttack(other);
    }

    private void TryAttack(Collider2D targetCollider)
    {
        if (targetCollider == null || Time.time < nextAttackTime)
        {
            return;
        }

        PlayerUnit target = targetCollider.GetComponentInParent<PlayerUnit>();
        if (target == null || target.IsDead)
        {
            return;
        }

        float actualDamage = target.TakeDamage(contactDamage);
        if (actualDamage <= 0f)
        {
            return;
        }

        nextAttackTime = Time.time + attackInterval;
        Debug.Log(
            $"敵人對玩家造成 {actualDamage} 點傷害，玩家剩餘生命：{target.CurrentHealth}/{target.MaxHealth}",
            this);
    }
}
