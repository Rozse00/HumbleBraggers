using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    #region Variables
    [Header("Actions")]
    [SerializeField] private InputActionReference moveAction;      // Value, Vector2
    [SerializeField] private InputActionReference lookAction;      // Value, Vector2
    [SerializeField] private InputActionReference interactAction;  // Button
    [SerializeField] private InputActionReference sprintAction;    // Button (optional)

    [Header("Movement & Look")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float sens = 0.1f; 
    [SerializeField] private float maxPitch = 85f;
    [SerializeField] private Transform cameraTransfrom;
    [SerializeField] private bool lockCursor = true;

    [Header("Interactions")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactMask;

    private CharacterController controller;
    private IInteractable currentTarget;
    private float pitch;
    #endregion

    #region Monobehavior Methods
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransfrom == null && Camera.main != null)
        {
            cameraTransfrom = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        EnableAction(moveAction);
        EnableAction(lookAction);
        EnableAction(interactAction);
        EnableAction(sprintAction);

        SetCursorLocked(lockCursor);
    }

    private void OnDisable()
    {
        DisableAction(moveAction);
        DisableAction(lookAction);
        DisableAction(interactAction);
        DisableAction(sprintAction);

        SetCursorLocked(false);
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
        UpdateInteractionTarget();

        if (interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            TryInteract();
        }
    }
    #endregion

    #region Movement & Look
    private void HandleLook() {
        if (lookAction == null || cameraTransfrom == null)
        {
            return; 
        }

        Vector2 look = lookAction.action.ReadValue<Vector2>();

        float yaw = look.x * sens;
        float pitchDelta = -look.y * sens;
        transform.Rotate(0f, yaw, 0f);

        pitch = Mathf.Clamp(pitch + pitchDelta, -maxPitch, maxPitch);
        cameraTransfrom.localRotation = Quaternion.Euler(pitch, 0f, 0f); 
    }

    private void HandleMovement()
    {
        Vector2 input = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        bool sprinting = sprintAction != null && sprintAction.action.IsPressed();
        float speed = sprinting ? sprintSpeed : walkSpeed;

        Vector3 move = (transform.right * input.x + transform.forward * input.y);
        move = Vector3.ClampMagnitude(move, 1f) * speed;

        controller.SimpleMove(move); 
    }
    #endregion

    #region Interactions
    private void UpdateInteractionTarget() 
    {
        IInteractable interactable = null;

        if (cameraTransfrom != null){
            Ray ray = new Ray(cameraTransfrom.position, cameraTransfrom.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask, QueryTriggerInteraction.Collide))
            {
                interactable = hit.collider.GetComponentInParent<IInteractable>(); 
                if (interactable != null && !interactable.CanInteract(gameObject))
                {
                    interactable = null; 
                }
            }
        }

        currentTarget = interactable;

    }
    

   private void TryInteract()
   {
        if (currentTarget != null && currentTarget.CanInteract(gameObject))
        {
            currentTarget.Interact(gameObject); 
        }
   }
    #endregion

    #region Helpers
    private static void EnableAction(InputActionReference reference)
    {
        if (reference != null)
        {
            reference.action.Enable(); 
        }
    }
    
    private static void DisableAction(InputActionReference reference)
    {
        if (reference != null)
        {
            reference.action.Disable(); 
        }
    }

    private static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked; 
    }
    #endregion
}
