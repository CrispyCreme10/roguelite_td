using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{

    [SerializeField] private float MoveSpeed = 1f;

    private Vector3 _moveDirection;

    void Update()
    {
        transform.Translate(_moveDirection * MoveSpeed * Time.deltaTime);
    }

    public void SetMoveTowardsDir(Vector3 dir)
    {
        _moveDirection = dir;
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Boundary"))
        {
            Destroy(gameObject);
        }
    }
}
