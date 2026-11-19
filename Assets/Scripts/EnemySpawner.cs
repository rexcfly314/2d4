using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemySpawner : MonoBehaviour
{
    [Header("依賴")]
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private InfiniteGroundController groundController;

    [Header("生成規則")]
    [SerializeField, Min(0.05f)] private float spawnInterval = 1.5f;
    [SerializeField, Min(1)] private int maximumActiveEnemies = 25;
    [SerializeField, Min(0f)] private float edgeInset = 0.5f;

    private float nextSpawnTime;

    private void OnEnable()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }

    private void LateUpdate()
    {
        if (Time.time < nextSpawnTime || enemyPool == null || groundController == null)
        {
            return;
        }

        nextSpawnTime = Time.time + spawnInterval;
        if (enemyPool.ActiveCount >= maximumActiveEnemies ||
            !groundController.TryGetActiveGroundBounds(out Bounds bounds))
        {
            return;
        }

        Vector2 position = GetRandomEdgePosition(bounds);
        enemyPool.Spawn(position);
    }

    private Vector2 GetRandomEdgePosition(Bounds bounds)
    {
        float insetX = Mathf.Min(edgeInset, bounds.extents.x);
        float insetY = Mathf.Min(edgeInset, bounds.extents.y);
        float left = bounds.min.x + insetX;
        float right = bounds.max.x - insetX;
        float bottom = bounds.min.y + insetY;
        float top = bounds.max.y - insetY;

        switch (Random.Range(0, 4))
        {
            case 0:
                return new Vector2(left, Random.Range(bottom, top));
            case 1:
                return new Vector2(right, Random.Range(bottom, top));
            case 2:
                return new Vector2(Random.Range(left, right), top);
            default:
                return new Vector2(Random.Range(left, right), bottom);
        }
    }
}
