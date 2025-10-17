using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(ActionPointComponentBehaviour))]
[RequireComponent(typeof(EnergyComponentBehaviour))]
[RequireComponent(typeof(ExplorationPlayerIdentifier))]
public class Movement : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("The InputReader ScriptableObject that provides player input events.")]
    [SerializeField] private InputReader inputReader;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float gravityValue = -9.81f;
    private CharacterController _characterController;
    private Transform _mainCameraTransform;

    private Vector2 _moveInput;
    private Vector3 _playerVelocity;
    private bool _isGrounded;
    private CombatTrigger _currentCombatTrigger;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _mainCameraTransform = Camera.main.transform;

        if (inputReader == null)
        {
            GameLog.LogError($"InputReader is not assigned in the inspector on {gameObject.name}!", this);
        }
    }

    private void OnEnable()
    {
        if (inputReader != null)
        {
            inputReader.MoveEvent += OnMove;
            inputReader.InteractEvent += OnInteract;
        }
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.MoveEvent -= OnMove;
            inputReader.InteractEvent -= OnInteract;
        }
    }

    private void OnMove(Vector2 moveInput)
    {
        _moveInput = moveInput;
    }

    private void OnInteract()
    {
        if (_currentCombatTrigger != null)
        {
            _currentCombatTrigger.StartCombatInteraction();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CombatTrigger>(out var combatTrigger))
        {
            _currentCombatTrigger = combatTrigger;
            ServiceLocator.Get<IGameplayUIService>()?.ShowActionButton(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<CombatTrigger>(out var combatTrigger) && combatTrigger == _currentCombatTrigger)
        {
            _currentCombatTrigger = null;
            ServiceLocator.Get<IGameplayUIService>()?.ShowActionButton(false);
        }
    }

    private void Update()
    {
        _isGrounded = _characterController.isGrounded;

        if (_isGrounded && _playerVelocity.y < 0)
        {
            _playerVelocity.y = -2f;
        }

        Vector3 forward = _mainCameraTransform.forward;
        Vector3 right = _mainCameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        Vector3 desiredMoveDirection = (forward * _moveInput.y + right * _moveInput.x).normalized;

        _playerVelocity.y += gravityValue * Time.deltaTime;

        _characterController.Move((desiredMoveDirection * moveSpeed + _playerVelocity) * Time.deltaTime);

        if (desiredMoveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(desiredMoveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }
}