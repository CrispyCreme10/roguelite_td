using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivorController : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 5f;

    private bool _isInboundsTop = true;
    private bool _isInboundsRight = true;
    private bool _isInboundsBottom = true;
    private bool _isInboundsLeft = true;
    private CasterController _casterController;

    void Awake()
    {
        _casterController = GetComponent<CasterController>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && _casterController != null)
        {
            var projectileDirection = Input.mousePosition;
            var dir = Camera.main.ScreenToWorldPoint(projectileDirection) - transform.position;
            dir.z = 0f;
            _casterController.CastProjectile(dir.normalized);
        }
    }

    void FixedUpdate() 
    {
        Move();
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Boundary") && other.name == "Top")
        {
            _isInboundsTop = false;
        }
        if (other.CompareTag("Boundary") && other.name == "Right")
        {
            _isInboundsRight = false;
        }
        if (other.CompareTag("Boundary") && other.name == "Bottom")
        {
            _isInboundsBottom = false;
        }
        if (other.CompareTag("Boundary") && other.name == "Left")
        {
            _isInboundsLeft = false;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Boundary") && other.name == "Top")
        {
            _isInboundsTop = true;
        }
        if (other.CompareTag("Boundary") && other.name == "Right")
        {
            _isInboundsRight = true;
        }
        if (other.CompareTag("Boundary") && other.name == "Bottom")
        {
            _isInboundsBottom = true;
        }
        if (other.CompareTag("Boundary") && other.name == "Left")
        {
            _isInboundsLeft = true;
        }
    }

    void Move()
    {
        var moveX = Input.GetAxisRaw("Horizontal");
        var moveY = Input.GetAxisRaw("Vertical");
        if (moveY > 0 && _isInboundsTop)
        {
            transform.position += new Vector3(0f, moveY * moveSpeed * Time.deltaTime, 0f);
        }
        if (moveY < 0 && _isInboundsBottom)
        {
            transform.position += new Vector3(0f, moveY * moveSpeed * Time.deltaTime, 0f);
        }
        if (moveX > 0 && _isInboundsRight)
        {
            transform.position += new Vector3(moveX * moveSpeed * Time.deltaTime, 0f, 0f);
        }
        if (moveX < 0 && _isInboundsLeft)
        {
            transform.position += new Vector3(moveX * moveSpeed * Time.deltaTime, 0f, 0f);
        }
    }
}
