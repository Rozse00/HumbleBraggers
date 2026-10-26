
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementScript : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 65f;

    private CharacterController controller;
    private float verticalVelocity;
    private float yaw;
    private float pitch = 15f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Start()
    {
        yaw = transform.eulerAngles.y;
        LockCursor(true);
    }

    private void Update()
    {
        if (Keyboard.current == null ||
            Mouse.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            LockCursor(false);

        if (Mouse.current.leftButton.wasPressedThisFrame)
            LockCursor(true);

        if (cameraTransform == null)
            return;

        HandleCamera();
        HandleMovement();
    }

    private void HandleCamera()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        yaw += mouseDelta.x * mouseSensitivity;
        pitch -= mouseDelta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraTransform.rotation =
            Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandleMovement()
    {
        Keyboard keyboard = Keyboard.current;

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed) horizontal -= 1f;
        if (keyboard.dKey.isPressed) horizontal += 1f;
        if (keyboard.sKey.isPressed) vertical -= 1f;
        if (keyboard.wKey.isPressed) vertical += 1f;

        Vector3 movement =
            transform.right * horizontal +
            transform.forward * vertical;

        movement = Vector3.ClampMagnitude(movement, 1f);

        float speed = keyboard.leftShiftKey.isPressed
            ? sprintSpeed
            : moveSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (controller.isGrounded &&
            keyboard.spaceKey.wasPressedThisFrame)
        {
            verticalVelocity =
                Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        controller.Move(
            (movement * speed +
             Vector3.up * verticalVelocity) *
            Time.deltaTime
        );
    }

    private void LockCursor(bool locked)
    {
        Cursor.lockState = locked
            ? CursorLockMode.Locked
            : CursorLockMode.None;

        Cursor.visible = !locked;
    }
}
