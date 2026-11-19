using UnityEngine;

[DisallowMultipleComponent]
public sealed class DeadModule : MonoBehaviour
{
    private PlayerUnit playerUnit;
    private EnemyUnit enemyUnit;
    private PooledEnemy pooledEnemy;
    private bool isTriggered;

    private void Awake()
    {
        playerUnit = GetComponent<PlayerUnit>();
        enemyUnit = GetComponent<EnemyUnit>();
        pooledEnemy = GetComponent<PooledEnemy>();

        if (playerUnit == null && enemyUnit == null)
        {
            Debug.LogError("DeadModule 找不到 PlayerUnit 或 EnemyUnit。", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (pooledEnemy == null)
        {
            pooledEnemy = GetComponent<PooledEnemy>();
        }

        isTriggered = false;
        if (playerUnit != null)
        {
            playerUnit.Died += HandlePlayerDeath;
        }

        if (enemyUnit != null)
        {
            enemyUnit.Died += HandleEnemyDeath;
        }
    }

    private void OnDisable()
    {
        if (playerUnit != null)
        {
            playerUnit.Died -= HandlePlayerDeath;
        }

        if (enemyUnit != null)
        {
            enemyUnit.Died -= HandleEnemyDeath;
        }
    }

    public void TriggerDeath()
    {
        if (isTriggered)
        {
            return;
        }

        isTriggered = true;
        if (enemyUnit != null && pooledEnemy != null && pooledEnemy.TryReturnToPool())
        {
            Debug.Log($"{gameObject.name} 死亡後已回收到敵人池。", this);
            return;
        }

        Debug.Log($"{gameObject.name} 觸發死亡，將刪除物件。", this);
        Destroy(gameObject);
    }

    private void HandlePlayerDeath(PlayerUnit unit)
    {
        TriggerDeath();
    }

    private void HandleEnemyDeath(EnemyUnit unit)
    {
        TriggerDeath();
    }
}
