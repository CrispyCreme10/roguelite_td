using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    [SerializeField] private int StartingHealth;

    int _health;

    void Awake()
    {
        _health = StartingHealth;
    }

    public void IncreaseHealth(int amount)
    {
        _health += amount;
        if (_health > StartingHealth)
        {
            _health = StartingHealth;
        }
    }

    public void DecreaseHealth(int amount)
    {
        _health -= amount;
        if (_health <= 0)
        {
            // On Death
        }
    }
}
