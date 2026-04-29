using UnityEngine;

/// <summary>
/// PlayerController handles all player movement, input, and ability triggers.
/// Attach this script to the Player capsule GameObject.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CombatSystem))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;          // How fast the player walks
    public float turnSmoothTime = 0.1f;   // How smoothly the player rotates toward movement direction

    [Header("References")]
    public Transform cameraTransform;     // Drag the Main Camera here in the Inspector

    // Private references
    private CharacterController _characterController;
    private CombatSystem _combatSystem;
    private float _turnSmoothVelocity;    // Used internally by SmoothDampAngle
    private bool _isDead = false;

    // Gravity
    private float _verticalVelocity = 0f;
    private float _gravity = -9.81f;

    void Start()
    {
        // Get required components from this GameObject
        _characterController = GetComponent<CharacterController>();
        _combatSystem = GetComponent<CombatSystem>();

        // Lock and hide the mouse cursor for camera control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Don't process input if the player is dead
        if (_isDead) return;

        HandleMovement();
        HandleAbilityInput();
    }

    /// <summary>
    /// Reads WASD input and moves the player relative to the camera direction.
    /// </summary>
    void HandleMovement()
    {
        // Read raw axis input (-1 to 1)
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D keys
        float vertical = Input.GetAxisRaw("Vertical");     // W/S keys

        // Build a direction vector from input
        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        // Only move and rotate if there is input
        if (inputDir.magnitude >= 0.1f)
        {
            // Calculate the target angle based on camera direction + input direction
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg
                                 + cameraTransform.eulerAngles.y;

            // Smoothly rotate the player toward the target angle
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref _turnSmoothVelocity,
                turnSmoothTime
            );
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Move in the direction the player is facing
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            _characterController.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
        }

        // Apply gravity so the player stays grounded
        if (_characterController.isGrounded)
        {
            _verticalVelocity = -1f; // Small downward force to keep grounded
        }
        else
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }

        _characterController.Move(new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
    }

    /// <summary>
    /// Listens for mouse button clicks and triggers abilities via CombatSystem.
    /// </summary>
    void HandleAbilityInput()
    {
        // Left mouse button = basic punch attack
        if (Input.GetMouseButtonDown(0))
        {
            _combatSystem.PerformPunch();
        }

        // Right mouse button = special energy blast
        if (Input.GetMouseButtonDown(1))
        {
            _combatSystem.PerformEnergyBlast();
        }
    }

    /// <summary>
    /// Called by CombatSystem or UIManager when the player's health reaches zero.
    /// </summary>
    public void Die()
    {
        _isDead = true;
        Debug.Log("Player has died!");
        // UIManager will detect this and show the Game Over screen
    }

    /// <summary>
    /// Returns whether the player is currently dead.
    /// </summary>
    public bool IsDead() => _isDead;
}
