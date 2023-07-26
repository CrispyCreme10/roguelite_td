using UnityEngine;
using UnityEngine.InputSystem;

public class SurvivorController : MonoBehaviour
{

    [SerializeField] private Camera sceneCamera;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float moveSpeed = 5f;

    private PlayerInputActions _playerInputActions;
    private CasterController _casterController;
    private Vector2 _moveDirection;
    private Vector2 _mousePosition;
    void Awake()
    {
        _casterController = GetComponent<CasterController>();
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Player.Enable();
    }

    private void OnEnable()
    {
        _playerInputActions.Player.SelectTurret1.performed += SelectTurret1;
        _playerInputActions.Player.PrimaryFire.performed += PrimaryFire;
        _playerInputActions.Player.SecondaryFire.performed += SecondaryFire;
    }

    private void OnDisable()
    {
        _playerInputActions.Player.SelectTurret1.performed -= SelectTurret1;
        _playerInputActions.Player.PrimaryFire.performed -= PrimaryFire;
        _playerInputActions.Player.SecondaryFire.performed -= SecondaryFire;
    }

    private void FixedUpdate() 
    {
        Move();
        Look();
    }

    void Move()
    {
        Vector2 moveDir = _playerInputActions.Player.Move.ReadValue<Vector2>();
        rb.velocity = Time.fixedDeltaTime * moveSpeed * new Vector2(moveDir.x, moveDir.y);
    }

    private void Look()
    {
        Vector2 aimDir;
        if (_playerInputActions.Player.Look.activeControl?.path.Contains("Mouse") ?? false)
        {
            Vector2 lookPos = _playerInputActions.Player.Look.ReadValue<Vector2>();
            Vector2 mousePos = sceneCamera.ScreenToWorldPoint(lookPos);
            aimDir = mousePos - rb.position;
        }
        else
        {
            aimDir = _playerInputActions.Player.Look.ReadValue<Vector2>();
        }
        
        float aimAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = aimAngle;
    }

    private void PrimaryFire(InputAction.CallbackContext context)
    {
        if (_casterController != null)
        {
            var lookPos = _playerInputActions.Player.Look.ReadValue<Vector2>();
            var dir = sceneCamera.ScreenToWorldPoint(lookPos) - transform.position;
            dir.z = 0f;
            _casterController.CastProjectile(ProjectileSlot.Primary, dir.normalized);
        }
    }
    
    private void SecondaryFire(InputAction.CallbackContext context)
    {
        if (_casterController != null)
        {
            var lookPos = _playerInputActions.Player.Look.ReadValue<Vector2>();
            var dir = sceneCamera.ScreenToWorldPoint(lookPos) - transform.position;
            dir.z = 0f;
            _casterController.CastProjectile(ProjectileSlot.Secondary, dir.normalized);
        }
    }
    
    private void SelectTurret1(InputAction.CallbackContext context)
    {
        
    }
}
