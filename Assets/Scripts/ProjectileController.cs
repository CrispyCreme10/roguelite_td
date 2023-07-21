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
        transform.Translate(_moveDirection * _moveSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if (_DamageController != null)
        {
            _DamageController.DealDamage(other);
        }        

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
