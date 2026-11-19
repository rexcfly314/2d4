using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(HealthBarView))]
public sealed class PlayerHealthPresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerUnit player;
    [SerializeField] private HealthBarView healthBarView;

    private PlayerUnit subscribedPlayer;

    private void Awake()
    {
        if (healthBarView == null)
        {
            healthBarView = GetComponent<HealthBarView>();
        }
    }

    private void OnEnable()
    {
        TryBind();
    }

    private void Start()
    {
        TryBind();

        if (subscribedPlayer == null)
        {
            Debug.LogError("玩家血量 UI 找不到 PlayerUnit，請在 Inspector 指定。", this);
        }
    }

    private void OnDisable()
    {
        if (subscribedPlayer != null)
        {
            subscribedPlayer.HealthChanged -= HandleHealthChanged;
            subscribedPlayer = null;
        }
    }

    private void TryBind()
    {
        if (healthBarView == null)
        {
            return;
        }

        if (player == null && GameManager.Instance != null)
        {
            player = GameManager.Instance.GetPlayer();
        }

        if (player == null)
        {
            return;
        }

        if (subscribedPlayer != player)
        {
            if (subscribedPlayer != null)
            {
                subscribedPlayer.HealthChanged -= HandleHealthChanged;
            }

            subscribedPlayer = player;
            subscribedPlayer.HealthChanged += HandleHealthChanged;
        }

        HandleHealthChanged(subscribedPlayer.CurrentHealth, subscribedPlayer.MaxHealth);
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        healthBarView.SetHealth(currentHealth, maxHealth);
    }
}
