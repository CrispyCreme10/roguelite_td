using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class SurvivorController : MonoBehaviour {
    private Action<Vector2> OnSurvivorMove;
    private Action<bool> OnCursorTowerRangeChange;

    [Header("Refs")] [SerializeField] private Camera sceneCamera;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform playerTowersContainer;
    [SerializeField] private Transform towerSpawnPoint;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private SpriteRenderer towerRangeRenderer;
    [SerializeField] private TowerController tower1;

    [Header("Fields")] [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float towerRange = 5f;
    [SerializeField] private float towerRangeMask = 1f;

    private PlayerInputActions _playerInputActions;
    private CasterController _casterController;
    private Vector2 _moveDirection;
    private Vector2 _mousePosition;
    private TowerController _selectedTower;
    private bool? _isMouseInTowerRange = null;

    private bool IsTowerSelected => _selectedTower != null;
    private float TowerRangeRadius => towerRange / 2;
    private float TowerRangeMaskRadius => towerRangeMask / 2;

    private void Awake() {
        _casterController = GetComponent<CasterController>();
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Player.Enable();
    }

    private void OnEnable() {
        _playerInputActions.Player.PlaceTurret.performed += PlaceTower;
        _playerInputActions.Player.DeselectTurret.performed += DeselectTower;
        _playerInputActions.Player.SelectTurret1.performed += SelectTurret1;
        _playerInputActions.Player.PrimaryFire.performed += PrimaryFire;
        _playerInputActions.Player.SecondaryFire.performed += SecondaryFire;
    }

    private void OnDisable() {
        _playerInputActions.Player.PlaceTurret.performed -= PlaceTower;
        _playerInputActions.Player.DeselectTurret.performed -= DeselectTower;
        _playerInputActions.Player.SelectTurret1.performed -= SelectTurret1;
        _playerInputActions.Player.PrimaryFire.performed -= PrimaryFire;
        _playerInputActions.Player.SecondaryFire.performed -= SecondaryFire;
    }

    private void Update() {
        if (IsTowerSelected) {
            HandleMouseTowerRange();
        }
    }

    private void FixedUpdate() {
        Move();
        Look();
    }

    private void Move() {
        Vector2 moveDir = _playerInputActions.Player.Move.ReadValue<Vector2>();
        Vector2 velocity = Time.fixedDeltaTime * moveSpeed * new Vector2(moveDir.x, moveDir.y);
        rb.velocity = velocity;
        OnSurvivorMove?.Invoke(velocity);
    }

    private void Look() {
        Vector2 aimDir;
        if (_playerInputActions.Player.Look.activeControl?.path.Contains("Mouse") ?? false) {
            Vector2 lookPos = _playerInputActions.Player.Look.ReadValue<Vector2>();
            Vector2 mousePos = sceneCamera.ScreenToWorldPoint(lookPos);
            aimDir = mousePos - rb.position;
        } else {
            aimDir = _playerInputActions.Player.Look.ReadValue<Vector2>();
        }

        var aimAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = aimAngle + 180f;
    }

    private void HandleMouseTowerRange() {
        var mousePos = sceneCamera.ScreenToWorldPoint(Input.mousePosition);
        var mouseDistanceFromSurvivor = Vector2.Distance(mousePos, transform.position);
        var newValue = mouseDistanceFromSurvivor < TowerRangeRadius 
                       && mouseDistanceFromSurvivor > TowerRangeMaskRadius;
        if (newValue == _isMouseInTowerRange) return;
        _isMouseInTowerRange = newValue;
        OnCursorTowerRangeChange?.Invoke(newValue);
    }

    private void ShowTowerRange() {
        towerRangeRenderer.enabled = true;
    }

    private void HideTowerRange() {
        towerRangeRenderer.enabled = false;
    }

    // PLAYER ACTIONS
    private void PrimaryFire(InputAction.CallbackContext context) {
        if (_casterController == null || IsTowerSelected) return;
        var lookPos = _playerInputActions.Player.Look.ReadValue<Vector2>();
        var dir = sceneCamera.ScreenToWorldPoint(lookPos) - projectileSpawnPoint.position;
        dir.z = 0f;
        _casterController.CastProjectile(ProjectileSlot.Primary, dir.normalized);
    }

    private void SecondaryFire(InputAction.CallbackContext context) {
        if (_casterController == null || IsTowerSelected) return;

        var lookPos = _playerInputActions.Player.Look.ReadValue<Vector2>();
        var dir = sceneCamera.ScreenToWorldPoint(lookPos) - projectileSpawnPoint.position;
        dir.z = 0f;
        _casterController.CastProjectile(ProjectileSlot.Secondary, dir.normalized);
    }

    private void PlaceTower(InputAction.CallbackContext context) {
        if (!IsTowerSelected || _isMouseInTowerRange.HasValue && !_isMouseInTowerRange.Value) return;

        HideTowerRange();
        _selectedTower.SetPlaced();
        OnSurvivorMove -= _selectedTower.OnSurvivorMove;
        OnCursorTowerRangeChange -= _selectedTower.OnHandleInRange;
        _selectedTower = null;
    }

    private void DeselectTower(InputAction.CallbackContext context) {
        if (!IsTowerSelected) return;

        Destroy(_selectedTower.gameObject);
        _selectedTower = null;
    }

    private void SelectTurret1(InputAction.CallbackContext context) {
        if (IsTowerSelected) return;

        ShowTowerRange();
        _selectedTower = Instantiate(tower1, tower1.transform.position, Quaternion.identity, playerTowersContainer);
        _selectedTower.SetSelected(sceneCamera);
        OnSurvivorMove += _selectedTower.OnSurvivorMove;
        OnCursorTowerRangeChange += _selectedTower.OnHandleInRange;
    }
}