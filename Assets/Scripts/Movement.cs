using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour
{
        private ActionMap actions;
    private CharacterController characterController;
        [SerializeField] private Transform mainCameraTransform;

    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;
    public float gravityValue = -9.81f;
    public float jumpHeight = 1.2f;

    private Vector2 moveInput;
    private Vector3 playerVelocity;
    private bool isGrounded;

    private void Awake()
    {
                actions = new ActionMap();
        characterController = GetComponent<CharacterController>();
                if (mainCameraTransform == null)
        {
            mainCameraTransform = Camera.main.transform;
        }

        actions.Player.Move.performed += OnMove;
        actions.Player.Move.canceled += OnMove;
        actions.Player.Jump.performed += OnJump;
    }

    private void OnEnable()
    {
        actions.Player.Enable();
    }

        private void OnDisable()
    {
        actions.Player.Disable();
    }

    private void OnDestroy()
    {
        actions.Dispose();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
        }
    }

    private void Update()
    {
        isGrounded = characterController.isGrounded;

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        Vector3 forward = mainCameraTransform.forward;
        Vector3 right = mainCameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        Vector3 desiredMoveDirection = (forward * moveInput.y + right * moveInput.x).normalized;

        playerVelocity.y += gravityValue * Time.deltaTime;

        characterController.Move((desiredMoveDirection * moveSpeed + playerVelocity) * Time.deltaTime);

        if (desiredMoveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(desiredMoveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
