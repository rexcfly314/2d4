using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance
    {
        get;
        private set;
    }

    [Header("Player")]
    [SerializeField]
    private PlayerUnit player;

    private readonly HashSet<EnemyUnit> enemies =
        new HashSet<EnemyUnit>();

    public PlayerUnit Player => player;

    public int EnemyCount =>
        enemies.Count;

    private void Awake()
    {
        InitializeSingleton();
        ValidatePlayer();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        if (Instance == this)
        {
            return;
        }

        Debug.LogWarning(
            "場景中存在重複的 GameManager，將移除重複物件。",
            this);

        Destroy(gameObject);
    }

    private void ValidatePlayer()
    {
        if (player != null)
        {
            return;
        }

        Debug.LogError(
            "GameManager 尚未設定 PlayerUnit。",
            this);
    }

    public PlayerUnit GetPlayer()
    {
        return player;
    }

    public void RegisterEnemy(
        EnemyUnit enemy)
    {
        if (enemy == null)
        {
            return;
        }

        enemies.Add(enemy);
    }

    public void UnregisterEnemy(
        EnemyUnit enemy)
    {
        if (enemy == null)
        {
            return;
        }

        enemies.Remove(enemy);
    }
}