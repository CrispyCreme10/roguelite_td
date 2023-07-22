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
    [SerializeField] private Transform _crystalTransform;
    [SerializeField] private CircleCollider2D _crystalCollider;
    [SerializeField] private float _meleeRange = 1f;
    [SerializeField] private float _attackSpeed = 1f;

    bool _isTargetInRange;
    Collider2D _target;
    bool _canDealDamage = true;
    float _timeRemaining;

    private void Start()
    {
        ResetAttackCooldown();
    }

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
        if (!IsTargetInRange()) 
        {
            _movementController.SetMove(true);
            return;
        }

        // Debug.Log("In Range");
        _movementController.SetMove(false);
        
        if (_canDealDamage)
        {
            if (_damageController != null)
            {
                Attack();
                UpdateCanDealDamage(false);
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

    bool IsTargetInRange()
    {
        return Vector2.Distance(_enemyController.GetCurrentTarget().transform.position, transform.position) <= _meleeRange + _crystalCollider.radius;
    }

    // void OnCollisionEnter2D(Collision2D other)
    // {
    //     Debug.Log("Collision Enter");
    //     _target = other.collider;
    //     _isTargetInRange = true;
    //     ResetAttackCooldown();
    //     _movementController.SetMove(false);
    // }

    // void OnCollisionExit2D(Collision2D other)
    // {
    //     Debug.Log("Collision Exit");
    //     _target = null;
    //     _isTargetInRange = false;
    //     _movementController.SetMove(true);
    // }

    // void OnTriggerEnter2D(Collider2D other)
    // {
    //     Debug.Log("Trigger Enter");
    //     _target = other;
    //     _isTargetInRange = true;
    //     ResetAttackCooldown();
    //     _movementController.SetMove(false);
    // }

    // void OnTriggerExit2D(Collider2D other)
    // {
    //     Debug.Log("Trigger Exit");
    //     _target = null;
    //     _isTargetInRange = false;
    //     _movementController.SetMove(true);
    // }

    void OnDamageDeal(int damageAmount)
    {
        // UpdateCanDealDamage(false);
    }

    void UpdateCanDealDamage(bool value)
    {
        _canDealDamage = value;
        OnCanDealDamageChange?.Invoke(_canDealDamage);
    }

    void Attack()
    {
        Debug.Log("ATTACK");
        _damageController.DealDamage(_enemyController.GetCurrentTarget());
    }

    void ResetAttackCooldown()
    {
        _timeRemaining = _attackSpeed;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _meleeRange);
    }
}
