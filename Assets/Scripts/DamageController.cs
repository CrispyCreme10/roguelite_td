using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageController : MonoBehaviour
{
    public Action<int> OnDamageDeal;
    public Action<int> OnDamageReceived;

    [SerializeField] private bool _damageDealer;
    [SerializeField] private bool _damageReceiver;
    [SerializeField] private int _damageAmount;
    [SerializeField] private DamageTag _damageTag;
    [SerializeField] private List<DamageTag> _receivedDamageTagsIgnored;

    public bool IsDamageReceiver => _damageReceiver;

    public void DealDamage(Collider2D other) 
    {
        var otherDamageController = other.GetComponent<DamageController>();
        if (_damageDealer && ValidateOtherDamageController(otherDamageController))
        {
            otherDamageController.ReceivedDamage(_damageAmount);
            OnDamageDeal?.Invoke(_damageAmount);
        }
    }

    public bool CanBeAttackedByTag(DamageTag damageTag)
    {
        return !_receivedDamageTagsIgnored.Contains(damageTag);
    }

    private bool ValidateOtherDamageController(DamageController otherDamageController)
    {
        return otherDamageController != null && 
            otherDamageController.IsDamageReceiver && 
            otherDamageController.CanBeAttackedByTag(_damageTag);
    }

    public void ReceivedDamage(int amount)
    {
        OnDamageReceived?.Invoke(amount);
    }
}

public enum DamageTag
{
    Survivor,
    Enemy,
    Crystal
}