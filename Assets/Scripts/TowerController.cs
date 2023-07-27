using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TowerController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject healthbarCanvas;
    [SerializeField] private SpriteRenderer cannonRenderer;

    private bool _isSelected;
    private Transform _playerTowersContainer;
    
    public void SetSelected(Transform playerTowersContainer)
    {
        var color1 = spriteRenderer.color;
        color1.a = 0.3f;
        var color2 = cannonRenderer.color;
        color2.a = 0.3f;
        spriteRenderer.color = color1;
        healthbarCanvas.SetActive(false);
        cannonRenderer.color = color2;
        _isSelected = true;
        _playerTowersContainer = playerTowersContainer;
    }

    public void SetPlaced()
    {
        // undo selection values
        var color1 = spriteRenderer.color;
        color1.a = 1f;
        var color2 = cannonRenderer.color;
        color2.a = 1f;
        spriteRenderer.color = color1;
        healthbarCanvas.SetActive(true);
        cannonRenderer.color = color2;
        _isSelected = false;
        
        // set parent to scene
        transform.SetParent(_playerTowersContainer);
    }

    private void Update()
    {
        
    }
}
