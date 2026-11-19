using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyUnit))]
public sealed class EnemyHealthPresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthBarView healthBarView;

    private EnemyUnit enemyUnit;

    private void Awake()
    {
        enemyUnit = GetComponent<EnemyUnit>();

        if (healthBarView == null)
        {
            healthBarView = GetComponentInChildren<HealthBarView>(true);
        }

        if (healthBarView == null)
        {
            Debug.LogError("敵人血量 UI 找不到 HealthBarView。", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (enemyUnit == null || healthBarView == null)
        {
            return;
        }

        enemyUnit.HealthChanged += HandleHealthChanged;
        HandleHealthChanged(enemyUnit.CurrentHealth, enemyUnit.MaxHealth);
    }

    private void OnDisable()
    {
        if (enemyUnit != null)
        {
            enemyUnit.HealthChanged -= HandleHealthChanged;
        }
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        healthBarView.SetHealth(currentHealth, maxHealth);
    }
}
