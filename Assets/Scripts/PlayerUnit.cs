using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerStatus))]
public sealed class PlayerUnit : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private PlayerStatus status;

    private bool deathNotified;

    public event Action<PlayerUnit> Died;
    public event Action<float, float> HealthChanged;

    public Transform Transform =>
        transform;

    public float CurrentHealth =>
        status != null
            ? status.CurrentHealth
            : 0f;

    public float MaxHealth =>
        status != null
            ? status.MaxHealth
            : 0f;

    public float HealthNormalized =>
        status != null
            ? status.HealthNormalized
            : 0f;

    public bool IsDead =>
        status != null &&
        status.IsDead;

    private void Awake()
    {
        if (status == null)
        {
            status =
                GetComponent<PlayerStatus>();
        }

        if (status == null)
        {
            Debug.LogError(
                "PlayerUnit 初始化失敗，找不到 PlayerStatus。",
                this);

            enabled = false;
        }
    }

    public float TakeDamage(
        float damage)
    {
        if (status == null ||
            IsDead)
        {
            return 0f;
        }

        float actualDamage =
            status.TakeDamage(damage);

        if (actualDamage > 0f)
        {
            NotifyHealthChanged();
        }

        TryNotifyDeath();

        return actualDamage;
    }

    public float Heal(
        float amount)
    {
        if (status == null)
        {
            return 0f;
        }

        float actualHeal = status.Heal(amount);
        if (actualHeal > 0f)
        {
            NotifyHealthChanged();
        }

        return actualHeal;
    }

    public void SetMaxHealth(
        float value,
        bool restoreHealth = false)
    {
        if (status == null)
        {
            return;
        }

        float previousHealth = CurrentHealth;
        float previousMaxHealth = MaxHealth;

        status.SetMaxHealth(
            value,
            restoreHealth);

        if (previousHealth != CurrentHealth || previousMaxHealth != MaxHealth)
        {
            NotifyHealthChanged();
        }

        if (restoreHealth &&
            !status.IsDead)
        {
            deathNotified = false;
        }
    }

    public void RestoreFullHealth()
    {
        if (status == null)
        {
            return;
        }

        float previousHealth = CurrentHealth;
        status.RestoreFullHealth();
        deathNotified = false;

        if (previousHealth != CurrentHealth)
        {
            NotifyHealthChanged();
        }
    }

    private void NotifyHealthChanged()
    {
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    private void TryNotifyDeath()
    {
        if (status == null ||
            !status.IsDead ||
            deathNotified)
        {
            return;
        }

        deathNotified = true;

        Debug.Log(
            "PlayerUnit 已進入死亡狀態。",
            this);

        Action<PlayerUnit> handler =
            Died;

        if (handler != null)
        {
            handler(this);
        }
    }
}
