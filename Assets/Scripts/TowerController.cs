using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TowerController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject healthbarCanvas;
    [SerializeField] private SpriteRenderer cannonRenderer;
    [SerializeField] private CasterController casterController;
    [SerializeField] private DamageController damageController;
    [SerializeField] private SpriteRenderer activationRangeRenderer;

    private bool _isSelected;
    private Transform _playerTowersContainer;
    private Transform _towerSpawnPoint;
    
    [SerializeField]
    private List<DamageController> targets;

    public List<DamageController> Targets => targets;
    
    private void Start()
    {
        targets = new List<DamageController>();
    }

    private void Update()
    {
        if (_isSelected)
        {
            transform.position = _towerSpawnPoint.position;
        }
    }

    private void FixedUpdate()
    {
        if (!casterController.enabled) return;

        var currentTarget = GetCurrentTarget();
        if (currentTarget != null)
        {
            Vector2 dir = currentTarget.transform.position - transform.position;
            casterController.CastProjectile(ProjectileSlot.Primary, dir);
        }
    }

    public void AddTarget(DamageController d)
    {
        targets.Add(d);
    }

    public void RemoveTarget(DamageController d)
    {
        targets.Remove(d);
    }

    private DamageController GetCurrentTarget()
    {
        return targets.FirstOrDefault();
    }

    public void SetSelected(Transform playerTowersContainer, Transform towerSpawnPoint)
    {
        // set refs
        _isSelected = true;
        _playerTowersContainer = playerTowersContainer;
        _towerSpawnPoint = towerSpawnPoint;
        casterController.enabled = false;
        
        // update colors
        var color1 = spriteRenderer.color;
        color1.a = 0.3f;
        spriteRenderer.color = color1;
        var color2 = cannonRenderer.color;
        color2.a = 0.3f;
        cannonRenderer.color = color2;
        activationRangeRenderer.enabled = true;
        
        // deactivate healthbar
        healthbarCanvas.SetActive(false);
    }

    public void SetPlaced()
    {
        // set refs
        _isSelected = false;
        casterController.enabled = true;
        
        // update colors
        var color1 = spriteRenderer.color;
        color1.a = 1f;
        spriteRenderer.color = color1;
        var color2 = cannonRenderer.color;
        color2.a = 1f;
        cannonRenderer.color = color2;
        activationRangeRenderer.enabled = false;
        
        // extras
        healthbarCanvas.SetActive(true);
    }

    public void OnSurvivorMove(Vector2 velocity)
    {
        // rb.velocity = velocity;
    }
}
