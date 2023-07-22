using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] private EnemyController _enemyController;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private float _moveSpeed = 1f;

    private bool _canMove = true;

    void Update()
    {
        MovePhysics();
    }

    void MovePhysics()
    {
        if (_enemyController != null && _canMove)
        {
            if (_enemyController.TargetSurvivor != null && _enemyController.IsTargetSurvivor)
            {
                // move towards target survivor
                var dir = (_enemyController.TargetSurvivor.transform.position - transform.position).normalized;
                _rb.velocity = new Vector2(dir.x * _moveSpeed, dir.y * _moveSpeed);
                return;
            }

            if (_enemyController.IsTargetCrystal) 
            {
                var dir = (_enemyController.Crystal.transform.position - transform.position).normalized;
                _rb.velocity = new Vector2(dir.x * _moveSpeed, dir.y * _moveSpeed);
            }
        }
        else
        {
            _rb.velocity = Vector2.zero;
        }
    }

    public void SetMove(bool canMove)
    {
        _canMove = canMove;
    }
}
