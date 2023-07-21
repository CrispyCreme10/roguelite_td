using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivorController : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 5f;

    private bool _isInboundsY = true;
    private bool _isInboundsX = true;

    void Update() 
    {
        Move();
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Boundary"))
        {
            Debug.Log("SUCCESS"); 
        }
    }

    void Move()
    {
        var moveX = Input.GetAxisRaw("Horizontal");
        var moveY = Input.GetAxisRaw("Vertical");
        if (moveX != 0)
        {
            transform.position += new Vector3(moveX * moveSpeed * Time.deltaTime, 0f, 0f);
        }
        
        if (moveY != 0)
        {
            transform.position += new Vector3(0f, moveY * moveSpeed * Time.deltaTime, 0f);
        }
    }
}
