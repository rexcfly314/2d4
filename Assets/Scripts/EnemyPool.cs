using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyPool : MonoBehaviour
{
    [Header("敵人 Prefab")]
    [SerializeField] private EnemyUnit enemyPrefab;

    [Header("池子容量")]
    [SerializeField, Min(0)] private int initialSize = 10;
    [SerializeField, Min(1)] private int maximumSize = 60;

    [Header("課堂測試")]
    [SerializeField] private Vector2 testSpawnPosition = new Vector2(3f, 0f);

    private readonly Queue<EnemyUnit> available = new Queue<EnemyUnit>();
    private readonly HashSet<EnemyUnit> active = new HashSet<EnemyUnit>();
    private Transform inactiveContainer;
    private int createdCount;

    public int ActiveCount => active.Count;
    public int AvailableCount => available.Count;

    private void Awake()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPool 尚未指定 Enemy Prefab。", this);
            enabled = false;
            return;
        }

        maximumSize = Mathf.Max(1, maximumSize);
        var container = new GameObject("InactiveEnemies");
        inactiveContainer = container.transform;
        inactiveContainer.SetParent(transform, false);
        container.SetActive(false);

        int count = Mathf.Min(initialSize, maximumSize);
        for (int i = 0; i < count; i++)
        {
            EnemyUnit enemy = CreateEnemy();
            if (enemy != null)
            {
                available.Enqueue(enemy);
            }
        }
    }

    public EnemyUnit Spawn(Vector2 position)
    {
        if (!enabled)
        {
            return null;
        }

        EnemyUnit enemy = available.Count > 0 ? available.Dequeue() : CreateEnemy();
        if (enemy == null)
        {
            return null;
        }

        enemy.RestoreFullHealth();
        enemy.transform.SetParent(transform);
        enemy.transform.position = new Vector3(position.x, position.y, transform.position.z);
        enemy.transform.rotation = Quaternion.identity;

        Rigidbody2D body = enemy.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        active.Add(enemy);
        enemy.gameObject.SetActive(true);
        return enemy;
    }

    public bool Release(EnemyUnit enemy)
    {
        if (enemy == null || !active.Remove(enemy))
        {
            return false;
        }

        Rigidbody2D body = enemy.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        enemy.gameObject.SetActive(false);
        enemy.transform.SetParent(inactiveContainer);
        available.Enqueue(enemy);
        return true;
    }

    [ContextMenu("測試生成一隻敵人")]
    private void SpawnForTest()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("請在 Play Mode 中使用測試生成。", this);
            return;
        }

        Spawn(testSpawnPosition);
    }

    private EnemyUnit CreateEnemy()
    {
        if (createdCount >= maximumSize || inactiveContainer == null)
        {
            return null;
        }

        EnemyUnit enemy = Instantiate(enemyPrefab, inactiveContainer);
        enemy.gameObject.SetActive(false);
        PooledEnemy pooledEnemy = enemy.GetComponent<PooledEnemy>();
        if (pooledEnemy == null)
        {
            pooledEnemy = enemy.gameObject.AddComponent<PooledEnemy>();
        }

        pooledEnemy.SetOwner(this);
        createdCount++;
        return enemy;
    }
}
