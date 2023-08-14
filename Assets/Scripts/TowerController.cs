using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TowerController : MonoBehaviour {
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject healthbarCanvas;
    [SerializeField] private SpriteRenderer cannonRenderer;
    [SerializeField] private CasterController casterController;
    [SerializeField] private DamageController damageController;
    [SerializeField] private SpriteRenderer activationRangeRenderer;
    [SerializeField] private Collider2D rangeCollider;
    [SerializeField] private Transform cannonPivot;
    [SerializeField] private float pivotSpeed = 10f;

    private bool _canFire = true;
    private Vector3 _projectileDirection = Vector3.zero;
    private bool _isSelected;
    private Camera _mainCamera;

    [SerializeField] private List<DamageController> targets;

    public List<DamageController> Targets => targets;

    private void Start() {
        targets = new List<DamageController>();
    }

    private void Update() {
        if (_isSelected) {
            var screenPoint = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            screenPoint.z = 0f;
            transform.position = screenPoint;
        }

        if (!casterController.enabled) return;
        if (_canFire && _projectileDirection == Vector3.zero) {
            // set projectile direction
            var currentTarget = GetCurrentTarget();
            if (currentTarget != null) {
                _projectileDirection = currentTarget.transform.position - transform.position;
                _canFire = false;
            }
        }

        if (_projectileDirection == Vector3.zero) return;
        if (cannonPivot.transform.up != _projectileDirection.normalized) {
            cannonPivot.transform.up = Vector3.MoveTowards(cannonPivot.transform.up, _projectileDirection.normalized,
                Time.deltaTime * pivotSpeed);
        } else {
            Debug.Log("CAST");
            casterController.CastProjectile(ProjectileSlot.Primary, cannonPivot.transform.up);
            _canFire = true;
            _projectileDirection = Vector3.zero;
        }
    }

    private void FixedUpdate() {
        
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

    private IEnumerator RotateTowardsTargetAndFire(Transform currentTargetTransform) {
        var dir = currentTargetTransform.position - transform.position;
        // set cannon rotation (Target Lock-On)
        while (cannonPivot.transform.up != dir) {
            cannonPivot.transform.up = Vector3.MoveTowards(cannonPivot.transform.up, dir, 
                Time.deltaTime * pivotSpeed);
        }

        Debug.Log(dir);
        Debug.Log(cannonPivot.transform.up);
        
        // cast projectile
        casterController.CastProjectile(ProjectileSlot.Primary, cannonPivot.transform.up);
        
        _canFire = true;
        yield return null;
    }

    public void AddTarget(DamageController d) {
        targets.Add(d);
    }

    public void RemoveTarget(DamageController d) {
        targets.Remove(d);
    }

    private DamageController GetCurrentTarget() {
        return targets.FirstOrDefault();
    }

    public void SetSelected(Camera mainCamera) {
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

    public void SetPlaced() {
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

    public void OnSurvivorMove(Vector2 velocity) {
        // rb.velocity = velocity;
    }
}