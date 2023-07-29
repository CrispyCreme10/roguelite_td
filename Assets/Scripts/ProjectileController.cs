using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private DamageController _DamageController;
    [SerializeField] private float _moveSpeed = 1f;

    private Vector3 _moveDirection;
    private CasterController _casterController;

    void OnEnable()
    {
        _DamageController.OnDamageDeal += OnDamageDeal;
    }

    void OnDisable()
    {
        _DamageController.OnDamageDeal -= OnDamageDeal;
    }

    void Update()
    {
        transform.Translate(Time.deltaTime * _moveSpeed * _moveDirection);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("ON COLLISION: " + collision.gameObject.name);
        if (_DamageController != null)
        {
            _DamageController.DealDamage(collision.collider);
        }     
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        Debug.Log("ON TRIGGER: " + other.gameObject.name);
        if (other.CompareTag("Boundary"))
        {
            Destroy(gameObject);
        }
    }

    public void SetMoveTowardsDir(Vector3 dir)
    {
        _moveDirection = dir;
    }

    public void SetCasterController(CasterController casterController)
    {
        _casterController = casterController;
    }

    void OnDamageDeal(int damageAmount)
    {
        Destroy(gameObject);
    }
}
