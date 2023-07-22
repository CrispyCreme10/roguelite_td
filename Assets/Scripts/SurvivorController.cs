using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivorController : MonoBehaviour
{

    [SerializeField] private Camera sceneCamera;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float moveSpeed = 5f;

    private CasterController _casterController;
    private Vector2 _moveDirection;
    private Vector2 _mousePosition;
    void Awake()
    {
        _casterController = GetComponent<CasterController>();
    }

    void Update()
    {
        ProcessInputs();
        
        if (Input.GetButtonDown("Fire1") && _casterController != null)
        {
            var projectileDirection = Input.mousePosition;
            var dir = sceneCamera.ScreenToWorldPoint(projectileDirection) - transform.position;
            dir.z = 0f;
            _casterController.CastProjectile(dir.normalized);
        }
    }

    private void FixedUpdate() 
    {
        Move();
    }

    private void ProcessInputs()
    {
        var moveX = Input.GetAxisRaw("Horizontal");
        var moveY = Input.GetAxisRaw("Vertical");

        _moveDirection = new Vector2(moveX, moveY).normalized;
        _mousePosition = sceneCamera.ScreenToWorldPoint(Input.mousePosition);
    }

    void Move()
    {
        rb.velocity = new Vector2(_moveDirection.y * moveSpeed, _moveDirection.y * moveSpeed);
        
        // rotate player to follow mouse
        Vector2 aimDir = _mousePosition - rb.position;
        float aimAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = aimAngle;
    }
}
