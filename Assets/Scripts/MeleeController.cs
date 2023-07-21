using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MeleeController : MonoBehaviour
{
    public Action<bool> OnCanDealDamageChange;

    [SerializeField] private EnemyController _enemyController;
    [SerializeField] private DamageController _damageController;
    [SerializeField] private MovementController _movementController;
    [SerializeField] private Collider2D _meleeRange;
    [SerializeField] private float _attackSpeed = 1f;

    bool _isTargetInRange;
    Collider2D _target;
    bool _canDealDamage = true;
    float _timeRemaining;

    void OnEnable()
    {
        _damageController.OnDamageDeal += OnDamageDeal;
    }

    void OnDisabled()
    {
        _damageController.OnDamageDeal -= OnDamageDeal;
    }

    void Update()
    {
        if (!_isTargetInRange) return;
        
        if (_canDealDamage)
        {
            if (_damageController != null)
            {
                Attack();
            }
        }
        else
        {
            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining <= 0)
            {
                UpdateCanDealDamage(true);
                ResetAttackCooldown();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("Collision Enter");
        _target = other.collider;
        _isTargetInRange = true;
        ResetAttackCooldown();
        _movementController.SetMove(false);
    }

    void OnCollisionExit2D(Collision2D other)
    {
        Debug.Log("Collision Exit");
        _target = null;
        _isTargetInRange = false;
        _movementController.SetMove(true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger Enter");
        _target = other;
        _isTargetInRange = true;
        ResetAttackCooldown();
        _movementController.SetMove(false);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Trigger Exit");
        _target = null;
        _isTargetInRange = false;
        _movementController.SetMove(true);
    }

    void OnDamageDeal(int damageAmount)
    {
        UpdateCanDealDamage(false);
    }

    void UpdateCanDealDamage(bool value)
    {
        _canDealDamage = value;
        OnCanDealDamageChange?.Invoke(_canDealDamage);
    }

    void Attack()
    {
        _damageController.DealDamage(_target);
        UpdateCanDealDamage(false);
    }

    void ResetAttackCooldown()
    {
        _timeRemaining = _attackSpeed;
    }
}
