using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageController : MonoBehaviour
{
    public Action<int> OnApplyDamage;

    [SerializeField] private bool DamageDealer;
    [SerializeField] private bool DamageReceiver;
    [SerializeField] private int DamageAmount;

    public bool IsDamageReceiver => DamageReceiver;

    void OnTriggerEnter2D(Collider2D other) 
    {
        var otherDamageController = other.GetComponent<DamageController>();
        if (DamageDealer && otherDamageController != null && otherDamageController.IsDamageReceiver)
        {
            otherDamageController.ApplyDamage(DamageAmount);
        }
    }

    public void ApplyDamage(int amount)
    {
        OnApplyDamage?.Invoke(amount);
    }
}
