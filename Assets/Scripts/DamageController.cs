using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DamageController : MonoBehaviour
{
    public Action<int> OnDamageDeal;
    public Action<int> OnDamageReceived;

    [SerializeField] private bool damageDealer;
    [SerializeField] private bool damageReceiver;
    [SerializeField] private int damageAmount;
    [SerializeField] private DamageTag damageTag;
    [SerializeField] private List<DamageTag> receivedDamageTagsIgnored;

    public bool IsDamageReceiver => damageReceiver;
    public DamageTag DamageTag => damageTag;

    public void DealDamage(Collider2D other) 
    {
        var otherDamageController = other.GetComponent<DamageController>();
        if (damageDealer && ValidateOtherDamageController(otherDamageController))
        {
            otherDamageController.ReceivedDamage(damageAmount);
            OnDamageDeal?.Invoke(damageAmount);
        }
    }

    public void DealDamage(GameObject other)
    {
        var otherDamageController = other.GetComponent<DamageController>();
        if (damageDealer && ValidateOtherDamageController(otherDamageController))
        {
            Debug.Log("Deal Damage: " + other.name);
            otherDamageController.ReceivedDamage(damageAmount);
            OnDamageDeal?.Invoke(damageAmount);
        }
    }

    public bool CanBeAttackedByTag(DamageTag damageTag)
    {
        return !receivedDamageTagsIgnored.Contains(damageTag);
    }

    private bool ValidateOtherDamageController(DamageController otherDamageController)
    {
        return otherDamageController != null && 
            otherDamageController.IsDamageReceiver && 
            otherDamageController.CanBeAttackedByTag(damageTag);
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