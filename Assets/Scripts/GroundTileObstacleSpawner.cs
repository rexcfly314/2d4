using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GroundTileObstacleSpawner : MonoBehaviour
{
    [Header("障礙物 Prefab")]
    [SerializeField] private GameObject obstaclePrefab;

    [Header("每塊地板的障礙物數量")]
    [SerializeField, Min(0)] private int obstacleCount = 3;

    [Header("生成位置")]
    [SerializeField, Min(0f)] private float edgePadding = 1f;
    [SerializeField, Min(0f)] private float minimumSpacing = 1.5f;
    [SerializeField, Min(0f)] private float playerSafeRadius = 2.5f;
    [SerializeField, Min(1)] private int placementAttempts = 25;

    private readonly Dictionary<Transform, List<GameObject>> obstaclesByTile = new();
    private readonly List<Vector2> occupiedWorldPositions = new();
    private bool hasReportedMissingPrefab;

    public void RefreshObstacles(
        Transform tileTransform,
        Vector2 tileWorldSize,
        Vector2 playerWorldPosition)
    {
        if (!isActiveAndEnabled || tileTransform == null)
        {
            return;
        }

        if (obstaclePrefab == null)
        {
            if (!hasReportedMissingPrefab)
            {
                Debug.LogError("障礙物生成失敗，請在地板管理器上指定 Obstacle Prefab。", this);
                hasReportedMissingPrefab = true;
            }

            return;
        }

        if (!obstaclesByTile.TryGetValue(tileTransform, out List<GameObject> obstacles))
        {
            obstacles = new List<GameObject>();
            obstaclesByTile.Add(tileTransform, obstacles);
        }

        // 每個地板物件只建立一次障礙物，重用地板時重新配置位置。
        while (obstacles.Count < obstacleCount)
        {
            GameObject obstacle = Instantiate(obstaclePrefab, tileTransform);
            obstacle.SetActive(false);
            obstacles.Add(obstacle);
        }

        occupiedWorldPositions.Clear();
        for (int i = 0; i < obstacles.Count; i++)
        {
            GameObject obstacle = obstacles[i];
            if (obstacle == null)
            {
                continue;
            }

            if (i >= obstacleCount ||
                !TryFindPosition(tileTransform.position, tileWorldSize,
                    playerWorldPosition, out Vector2 position))
            {
                obstacle.SetActive(false);
                continue;
            }

            Vector3 oldPosition = obstacle.transform.position;
            obstacle.transform.position = new Vector3(
                position.x, position.y, oldPosition.z);
            occupiedWorldPositions.Add(position);
            obstacle.SetActive(true);
        }
    }

    private bool TryFindPosition(
        Vector3 tileCenter,
        Vector2 tileWorldSize,
        Vector2 playerWorldPosition,
        out Vector2 position)
    {
        float halfWidth = Mathf.Max(0f, tileWorldSize.x * 0.5f - edgePadding);
        float halfHeight = Mathf.Max(0f, tileWorldSize.y * 0.5f - edgePadding);
        float minimumSpacingSqr = minimumSpacing * minimumSpacing;
        float playerSafeRadiusSqr = playerSafeRadius * playerSafeRadius;

        for (int attempt = 0; attempt < placementAttempts; attempt++)
        {
            Vector2 candidate = new Vector2(
                tileCenter.x + Random.Range(-halfWidth, halfWidth),
                tileCenter.y + Random.Range(-halfHeight, halfHeight));

            if ((candidate - playerWorldPosition).sqrMagnitude < playerSafeRadiusSqr)
            {
                continue;
            }

            bool overlaps = false;
            foreach (Vector2 occupied in occupiedWorldPositions)
            {
                if ((candidate - occupied).sqrMagnitude < minimumSpacingSqr)
                {
                    overlaps = true;
                    break;
                }
            }

            if (overlaps)
            {
                continue;
            }

            position = candidate;
            return true;
        }

        position = default;
        return false;
    }
}
