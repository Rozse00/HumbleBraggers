
using UnityEngine;
using UnityEngine.Events;

public class LurkerAI : MonoBehaviour
{
    public enum LurkerState
    {
        Hiding,
        Observed,
        Attacking,
        Cooldown
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera playerCamera;

    [Header("Visibility")]
    [SerializeField, Range(1f, 180f)]
    private float fieldOfView = 90f;

    [SerializeField] private float maxViewDistance = 30f;

    [Tooltip("Layers that can block camera visibility.")]
    [SerializeField] private LayerMask visibilityMask = ~0;

    [Header("Teleport Repositioning")]
    [SerializeField] private float minTeleportDistance = 3f;
    [SerializeField] private float maxTeleportDistance = 8f;
    [SerializeField] private float repositionInterval = 2f;

    [Tooltip("Layers that count as solid obstacles for teleporting.")]
    [SerializeField] private LayerMask obstacleMask = ~0;

    [Tooltip("Layers used by floors and other walkable surfaces.")]
    [SerializeField] private LayerMask groundMask = ~0;

    [SerializeField] private float groundCheckHeight = 3f;
    [SerializeField] private float groundCheckDistance = 8f;

    [Tooltip("Vertical offset from the ground to the Lurker pivot.")]
    [SerializeField] private float standingOffset = 0f;

    [SerializeField] private float clearanceRadius = 0.4f;
    [SerializeField] private float clearanceHeight = 1.8f;
    [SerializeField] private int positionAttempts = 20;

    [Header("AI Timers")]
    [SerializeField] private float dispelTime = 3f;
    [SerializeField] private float unnoticedTime = 7f;
    [SerializeField] private float cooldownDuration = 5f;

    [Header("Orb and Attack")]
    [SerializeField] private float orbEnergyDrain = 5f;

    [Tooltip("Connect to your player damage method.")]
    [SerializeField] private UnityEvent onAttack;

    [Tooltip("Connect to an Orb method that accepts a float.")]
    [SerializeField] private UnityEvent<float> onDrainOrbEnergy;

    [Tooltip("Optional sound or visual effect when dispelled.")]
    [SerializeField] private UnityEvent onDispel;

    public LurkerState CurrentState { get; private set; }
        = LurkerState.Hiding;

    public bool InLineOfSight { get; private set; }

    private Renderer[] lurkerRenderers;
    private Collider[] lurkerColliders;

    private float observedTimer;
    private float unnoticedTimer;
    private float cooldownTimer;
    private float repositionTimer;

    private bool hasAttacked;
    private bool isTemporarilyHidden;

    private void Awake()
    {
        lurkerRenderers =
            GetComponentsInChildren<Renderer>(true);

        lurkerColliders =
            GetComponentsInChildren<Collider>(true);

        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        if (player == null || playerCamera == null)
            return;

        switch (CurrentState)
        {
            case LurkerState.Hiding:
                UpdateHiding();
                break;

            case LurkerState.Observed:
                UpdateObserved();
                break;

            case LurkerState.Attacking:
                UpdateAttacking();
                break;

            case LurkerState.Cooldown:
                UpdateCooldown();
                break;
        }
    }

    // SENSOR
    private bool CanPlayerSeeLurker()
    {
        Vector3 target = GetVisibilityPoint();

        Vector3 viewport =
            playerCamera.WorldToViewportPoint(target);

        if (viewport.z <= 0f ||
            viewport.x < 0f || viewport.x > 1f ||
            viewport.y < 0f || viewport.y > 1f)
        {
            return false;
        }

        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        if (distance > maxViewDistance)
            return false;

        float angle = Vector3.Angle(
            playerCamera.transform.forward,
            direction
        );

        if (angle > fieldOfView * 0.5f)
            return false;

        if (Physics.Raycast(
            origin,
            direction.normalized,
            out RaycastHit hit,
            distance + 0.1f,
            visibilityMask,
            QueryTriggerInteraction.Ignore))
        {
            Transform hitTransform = hit.transform;


            return hitTransform == transform ||
                   hitTransform.IsChildOf(transform);
        }


        return true;
    }

    private Vector3 GetVisibilityPoint()
    {
        return transform.position +
               Vector3.up * (clearanceHeight * 0.5f);
    }

    // GOAL
    private void UpdateHiding()
    {
        InLineOfSight = CanPlayerSeeLurker();

        if (InLineOfSight)
        {
            CurrentState = LurkerState.Observed;
            observedTimer = 0f;
            unnoticedTimer = 0f;
            return;
        }

        unnoticedTimer += Time.deltaTime;
        repositionTimer += Time.deltaTime;

        if (repositionTimer >= repositionInterval)
        {
            repositionTimer = 0f;
            TryReposition();
        }

        if (unnoticedTimer >= unnoticedTime)
        {
            CurrentState = LurkerState.Attacking;
        }
    }

    // Three continuous seconds of visibility dispels the Lurker.
    private void UpdateObserved()
    {
        InLineOfSight = CanPlayerSeeLurker();

        if (!InLineOfSight)
        {
            observedTimer = 0f;
            unnoticedTimer = 0f;
            repositionTimer = 0f;

            CurrentState = LurkerState.Hiding;
            return;
        }

        observedTimer += Time.deltaTime;

        if (observedTimer >= dispelTime)
            Dispel();
    }

    // Attack once, then enter cooldown.

    private void UpdateAttacking()
    {
        if (hasAttacked)
            return;

        hasAttacked = true;

        onAttack?.Invoke();
        onDrainOrbEnergy?.Invoke(orbEnergyDrain);

        HideLurker();
        BeginCooldown();
    }


    private void Dispel()
    {
        onDispel?.Invoke();

        HideLurker();

        observedTimer = 0f;
        unnoticedTimer = 0f;
        repositionTimer = 0f;
        hasAttacked = false;

        BeginCooldown();
    }

    private void BeginCooldown()
    {
        cooldownTimer = cooldownDuration;
        CurrentState = LurkerState.Cooldown;
    }

    private void UpdateCooldown()
    {
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer > 0f)
            return;

        if (!TryFindPosition(out Vector3 destination))
        {
            return;
        }

        transform.position = destination;

        ShowLurker();

        observedTimer = 0f;
        unnoticedTimer = 0f;
        repositionTimer = 0f;
        hasAttacked = false;

        CurrentState = LurkerState.Hiding;
    }

    // ACTUATOR
    private void TryReposition()
    {
        if (!TryFindPosition(out Vector3 destination))
            return;

        transform.position = destination;
    }

    private bool TryFindPosition(out Vector3 destination)
    {
        destination = transform.position;

        for (int i = 0; i < positionAttempts; i++)
        {
            Vector2 circle = Random.insideUnitCircle.normalized;

            if (circle.sqrMagnitude < 0.01f)
                circle = Vector2.right;

            float distance = Random.Range(
                minTeleportDistance,
                maxTeleportDistance
            );

            Vector3 candidate = player.position +
                new Vector3(circle.x, 0f, circle.y) * distance;

            Vector3 rayOrigin =
                candidate + Vector3.up * groundCheckHeight;

            if (!Physics.Raycast(
                rayOrigin,
                Vector3.down,
                out RaycastHit groundHit,
                groundCheckDistance,
                groundMask,
                QueryTriggerInteraction.Ignore))
            {
                continue;
            }

            candidate = groundHit.point +
                        Vector3.up * standingOffset;

            if (IsPositionVisible(candidate))
                continue;

            Vector3 capsuleBottom =
                candidate + Vector3.up * clearanceRadius;

            Vector3 capsuleTop =
                candidate + Vector3.up *
                Mathf.Max(clearanceRadius,
                    clearanceHeight - clearanceRadius);

            if (Physics.CheckCapsule(
                capsuleBottom,
                capsuleTop,
                clearanceRadius,
                obstacleMask,
                QueryTriggerInteraction.Ignore))
            {
                continue;
            }

            destination = candidate;
            return true;
        }

        return false;
    }

    private bool IsPositionVisible(Vector3 position)
    {
        Vector3 target =
            position + Vector3.up * (clearanceHeight * 0.5f);

        Vector3 viewport =
            playerCamera.WorldToViewportPoint(target);

        if (viewport.z <= 0f ||
            viewport.x < 0f || viewport.x > 1f ||
            viewport.y < 0f || viewport.y > 1f)
        {
            return false;
        }

        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        if (distance > maxViewDistance)
            return false;

        if (Vector3.Angle(
            playerCamera.transform.forward,
            direction) > fieldOfView * 0.5f)
        {
            return false;
        }

        return !Physics.Raycast(
            origin,
            direction.normalized,
            distance,
            visibilityMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private void HideLurker()
    {
        SetLurkerEnabled(false);
        isTemporarilyHidden = true;
    }

    private void ShowLurker()
    {
        SetLurkerEnabled(true);
        isTemporarilyHidden = false;
    }

    private void SetLurkerEnabled(bool enabledState)
    {
        foreach (Renderer rend in lurkerRenderers)
        {
            if (rend != null)
                rend.enabled = enabledState;
        }

        foreach (Collider col in lurkerColliders)
        {
            if (col != null)
                col.enabled = enabledState;
        }
    }

    public void ForceDispel()
    {
        if (CurrentState == LurkerState.Cooldown)
            return;

        Dispel();
    }
}
