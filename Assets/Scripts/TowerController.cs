using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TowerController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject healthbarCanvas;
    [SerializeField] private SpriteRenderer cannonRenderer;
    [SerializeField] private CasterController casterController;
    [SerializeField] private DamageController damageController;
    [SerializeField] private SpriteRenderer activationRangeRenderer;
    [SerializeField] private Collider2D rangeCollider;
    [SerializeField] private Transform cannonPivot;
    private bool _isSelected;
    private Camera _mainCamera;
    
    [SerializeField]
    private List<DamageController> targets;

    public List<DamageController> Targets => targets;

    private void Start()
    {
        targets = new List<DamageController>();
    }

    private void Update() {
        if (!_isSelected) return;
        var screenPoint = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        screenPoint.z = 0f;
        transform.position = screenPoint;
    }

    private void FixedUpdate()
    {
        if (!casterController.enabled) return;

        CastProjectile();
    }

    private void OnMouseEnter() {
        // highlight turret
    }

    private void OnMouseExit() {
        // unhighlight turret
    }

    private void OnMouseDown() {
        // show turret overlay menu
        // show turret range
    }

    private void CastProjectile() {
        var currentTarget = GetCurrentTarget();
        if (currentTarget == null) return;
        Vector2 dir = currentTarget.transform.position - transform.position;
        // set cannon rotation
        
        
        // cast
        casterController.CastProjectile(ProjectileSlot.Primary, dir);
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

    public void SetSelected(Camera mainCamera)
    {
        // set refs
        _isSelected = true;
        casterController.enabled = false;
        _mainCamera = mainCamera;
        
        // update colors
        var color1 = spriteRenderer.color;
        color1.a = 0.3f;
        spriteRenderer.color = color1;
        var color2 = cannonRenderer.color;
        color2.a = 0.3f;
        cannonRenderer.color = color2;
        activationRangeRenderer.enabled = true;
        
        // deactivate collider
        rangeCollider.enabled = false;
        
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
        
        // activate collider
        rangeCollider.enabled = true;
        
        // extras
        healthbarCanvas.SetActive(true);
    }

    public void OnHandleInRange(bool isMouseInSurvivorsTowerRange) {
        if (isMouseInSurvivorsTowerRange)
            SetTowerRangeGreen();
        else
            SetTowerRangeRed();
    }
    
    private void SetTowerRangeGreen() {
        activationRangeRenderer.color = new Color(0, 255, 0, 15f / 255);
    }

    private void SetTowerRangeRed() {
        activationRangeRenderer.color = new Color(255, 0, 0, 15f / 255);
    }

    public void OnSurvivorMove(Vector2 velocity)
    {
        // rb.velocity = velocity;
    }
}
