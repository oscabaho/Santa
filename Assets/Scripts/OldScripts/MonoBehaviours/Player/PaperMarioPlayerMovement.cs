using UnityEngine;
using ProyectSecret.MonoBehaviours.Player;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputController), typeof(PlayerCameraController))]
public class PaperMarioPlayerMovement : MonoBehaviour
{
    // Evento para notificar a otros componentes sobre el cambio de cámara
    public event System.Action<bool> OnCameraInvertedChanged;

    private PlayerPointSwitcher pointSwitcher;
    private PlayerInputController _input; // Referencia al nuevo InputController
    private PlayerCameraController _cameraController; // Referencia al controlador de cámara

    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 5f;

    // Referencias a componentes
    private Rigidbody rb;

    // Propiedades públicas de estado para que otros componentes puedan leerlas
    public bool IsGrounded { get; private set; } = true;
    public Vector3 CurrentVelocity { get; private set; }
    public bool IsCameraInverted { get; private set; } = false;

    // Backing field privado para el estado de movimiento hacia abajo
    private bool _isMovingDown;
    /// <summary>
    /// Indica si el jugador está presionando input de movimiento hacia abajo.
    /// Útil para el PlayerCameraController.
    /// </summary>
    public bool IsMovingDown {
        get => _isMovingDown;
        private set => _isMovingDown = value;
    }

    public void SetCameraInverted(bool inverted)
    {
        IsCameraInverted = inverted;
        OnCameraInvertedChanged?.Invoke(inverted);        
        pointSwitcher?.UpdateActivePoints(IsCameraInverted);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pointSwitcher = GetComponent<PlayerPointSwitcher>();
        _input = GetComponent<PlayerInputController>();
        _cameraController = GetComponent<PlayerCameraController>();
    }

    void Update()
    {
        if (_input != null)
        {
            // Esta lógica sigue siendo útil para el PlayerCameraController
            IsMovingDown = _input.MoveInput.y < -0.1f;
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleGroundCheck();
    }

    private void HandleMovement()
    {
        Camera currentCam = _cameraController?.GetActiveCamera();

        if (_input != null && currentCam != null)
        {
            Vector2 input = _input.MoveInput;
            
            // Si la cámara está invertida, el input de movimiento también se invierte.
            if (IsCameraInverted)
            {
                input.x = -input.x;
                input.y = -input.y;
            }

            Vector3 camForward = currentCam.transform.forward;
            Vector3 camRight = currentCam.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = (camForward * input.y + camRight * input.x).normalized;
            Vector3 targetVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);
            rb.linearVelocity = targetVelocity;
            CurrentVelocity = rb.linearVelocity;

            UpdatePointSwitcherRotation(moveDir);
        }
    }

    private void HandleGroundCheck()
    {
        // Usamos un SphereCast para una detección de suelo más robusta que OnCollisionEnter.
        // Esto maneja mejor los bordes y las pendientes.
        float sphereRadius = 0.3f;
        Vector3 spherePosition = transform.position + Vector3.up * (sphereRadius - 0.05f);
        IsGrounded = Physics.CheckSphere(spherePosition, sphereRadius, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
    }

    private void UpdatePointSwitcherRotation(Vector3 moveDir)
    {
        if (pointSwitcher != null && moveDir.sqrMagnitude > 0.01f)
        {
            Transform weaponPoint = IsCameraInverted ? pointSwitcher.WeaponPoint1 : pointSwitcher.WeaponPoint;
            Transform hitboxPoint = IsCameraInverted ? pointSwitcher.HitBoxPoint1 : pointSwitcher.HitBoxPoint;

            if (weaponPoint != null) weaponPoint.forward = moveDir;
            if (hitboxPoint != null) hitboxPoint.forward = moveDir;
        }
    }
}
