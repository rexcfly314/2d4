using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyUnit))]
public sealed class PooledEnemy : MonoBehaviour
{
    private EnemyPool ownerPool;
    private EnemyUnit enemyUnit;

    private void Awake()
    {
        enemyUnit = GetComponent<EnemyUnit>();
    }

    public void SetOwner(EnemyPool pool)
    {
        ownerPool = pool;
    }

    public bool TryReturnToPool()
    {
        if (ownerPool == null || enemyUnit == null)
        {
            return false;
        }

        return ownerPool.Release(enemyUnit);
    }
}
