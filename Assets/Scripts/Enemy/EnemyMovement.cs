using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f;

    private EnemyController _enemyController;
    private Transform BodyTransform => transform.GetChild(1);

    private void Awake() {
        _enemyController = GetComponent<EnemyController>();
    }
    
    private void Update()
    {
        Move();
    }

    private void Move() {
        if (_enemyController == null) return;
        var targetPosition = _enemyController.GetCurrentTarget().transform.position;
        var dir = (targetPosition - transform.position).normalized;
        
        // rotate body
        BodyTransform.transform.up = dir;
        
        transform.position += new Vector3(dir.x * moveSpeed * Time.deltaTime,
            dir.y * moveSpeed * Time.deltaTime, 0f);
    }
}
