using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

public class HealthController : MonoBehaviour
{
    public Action<int, float> OnHealthChange; // current health, % of total health
    public Action OnDeath;

    [SerializeField] private DamageController DamageController;
    [SerializeField] private int StartingHealth;

    [ReadOnly]
    [SerializeField]
    int _health;

    void Awake()
    {
        _health = StartingHealth;
    }

    void OnEnable()
    {
        DamageController.OnDamageReceived += OnDamageReceived;
    }

    void OnDisable()
    {
        DamageController.OnDamageReceived -= OnDamageReceived;
    }

    public void IncreaseHealth(int amount)
    {
        _health += amount;
        if (_health > StartingHealth)
        {
            _health = StartingHealth;
        }

        DispatchOnHealthChange();
    }

    public void DecreaseHealth(int amount)
    {
        _health -= amount;
        if (_health <= 0)
        {
            OnDeath?.Invoke();
            Destroy(gameObject);
        }

        DispatchOnHealthChange();
    }

    void DispatchOnHealthChange()
    {
        OnHealthChange?.Invoke(_health, _health / (float)StartingHealth);
    }

    void OnDamageReceived(int damageAmount)
    {
        DecreaseHealth(damageAmount);
    }
}
