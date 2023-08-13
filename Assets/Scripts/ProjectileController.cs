using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private DamageController damageController;
    [SerializeField] private float moveSpeed = 1f;

    private Vector3 _moveDirection;
    private CasterController _casterController;

    void OnEnable()
    {
        damageController.OnDamageDeal += OnDamageDeal;
    }

    void OnDisable()
    {
        damageController.OnDamageDeal -= OnDamageDeal;
    }

    void Update()
    {
        transform.Translate(Time.deltaTime * moveSpeed * _moveDirection);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("ON COLLISION: " + collision.gameObject.name);
        if (damageController != null)
        {
            damageController.DealDamage(collision.collider);
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
        _moveDirection = dir.normalized;
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
