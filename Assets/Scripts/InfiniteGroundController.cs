using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class InfiniteGroundController : MonoBehaviour
{
    [Header("地板來源")]
    [SerializeField] private GameObject groundTilePrefab;
    [SerializeField] private Transform playerTransform;

    [Header("地板排列")]
    [SerializeField] private Vector2 tileWorldSize = new Vector2(10f, 10f);
    [SerializeField, Min(1)] private int visibleRadius = 2;

    private readonly Dictionary<Vector2Int, GameObject> activeTiles = new();
    private readonly Queue<GameObject> inactiveTiles = new();
    private readonly List<Vector2Int> keysToRemove = new();

    private GroundTileObstacleSpawner obstacleSpawner;
    private Vector2Int currentCell;
    private bool initialized;

    private void Awake()
    {
        // 使用同一個地板管理器上的障礙物生成元件。
        obstacleSpawner = GetComponent<GroundTileObstacleSpawner>();
    }

    private void Start()
    {
        if (groundTilePrefab == null)
        {
            Debug.LogError("InfiniteGroundController 尚未指定 Ground Tile Prefab。", this);
            enabled = false;
            return;
        }

        if (groundTilePrefab.TryGetComponent(out InfiniteGroundController _))
        {
            Debug.LogError(
                "Ground Tile Prefab 不應包含 InfiniteGroundController，請指定只有地板外觀的 Prefab。",
                this);
            enabled = false;
            return;
        }

        if (playerTransform == null && GameManager.Instance != null)
        {
            PlayerUnit player = GameManager.Instance.GetPlayer();
            if (player != null)
            {
                playerTransform = player.Transform;
            }
        }

        if (playerTransform == null)
        {
            Debug.LogError("InfiniteGroundController 尚未指定 Player Transform。", this);
            enabled = false;
            return;
        }

        tileWorldSize.x = Mathf.Max(0.01f, tileWorldSize.x);
        tileWorldSize.y = Mathf.Max(0.01f, tileWorldSize.y);

        if (obstacleSpawner == null)
        {
            Debug.LogWarning(
                "目前地板管理器沒有 GroundTileObstacleSpawner，地板可生成但不會產生障礙物。",
                this);
        }

        RefreshGround(true);
    }

    private void Update()
    {
        RefreshGround(false);
    }

    public bool TryGetActiveGroundBounds(out Bounds bounds)
    {
        if (!initialized)
        {
            bounds = default;
            return false;
        }

        float width = (visibleRadius * 2 + 1) * tileWorldSize.x;
        float height = (visibleRadius * 2 + 1) * tileWorldSize.y;
        Vector3 center = new Vector3(
            transform.position.x + currentCell.x * tileWorldSize.x,
            transform.position.y + currentCell.y * tileWorldSize.y,
            transform.position.z);

        bounds = new Bounds(center, new Vector3(width, height, 0f));
        return true;
    }

    private void RefreshGround(bool force)
    {
        if (playerTransform == null)
        {
            return;
        }

        Vector3 offset = playerTransform.position - transform.position;
        Vector2Int playerCell = new Vector2Int(
            Mathf.FloorToInt((offset.x + tileWorldSize.x * 0.5f) / tileWorldSize.x),
            Mathf.FloorToInt((offset.y + tileWorldSize.y * 0.5f) / tileWorldSize.y));

        if (!force && initialized && currentCell == playerCell)
        {
            return;
        }

        currentCell = playerCell;
        initialized = true;
        keysToRemove.Clear();

        foreach (KeyValuePair<Vector2Int, GameObject> entry in activeTiles)
        {
            if (Mathf.Abs(entry.Key.x - currentCell.x) > visibleRadius ||
                Mathf.Abs(entry.Key.y - currentCell.y) > visibleRadius)
            {
                keysToRemove.Add(entry.Key);
            }
        }

        foreach (Vector2Int key in keysToRemove)
        {
            GameObject tile = activeTiles[key];
            activeTiles.Remove(key);
            tile.SetActive(false);
            inactiveTiles.Enqueue(tile);
        }

        for (int y = -visibleRadius; y <= visibleRadius; y++)
        {
            for (int x = -visibleRadius; x <= visibleRadius; x++)
            {
                Vector2Int cell = currentCell + new Vector2Int(x, y);
                if (activeTiles.ContainsKey(cell))
                {
                    continue;
                }

                GameObject tile;
                if (inactiveTiles.Count > 0)
                {
                    tile = inactiveTiles.Dequeue();
                }
                else
                {
                    tile = Instantiate(groundTilePrefab, transform);
                }

                // 地板與障礙物配置完成後才啟用，避免在舊位置觸發碰撞。
                tile.SetActive(false);
                tile.transform.position = new Vector3(
                    transform.position.x + cell.x * tileWorldSize.x,
                    transform.position.y + cell.y * tileWorldSize.y,
                    transform.position.z);

                if (obstacleSpawner != null)
                {
                    obstacleSpawner.RefreshObstacles(
                        tile.transform, tileWorldSize, playerTransform.position);
                }

                tile.SetActive(true);
                activeTiles.Add(cell, tile);
            }
        }
    }
}
