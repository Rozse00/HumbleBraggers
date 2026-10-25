using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Camera controlledCamera;

    [Header("Camera Position")]
    [SerializeField]
    private Vector3 offset =
        new Vector3(0f, 0f, -4f);

    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 7f;
    [SerializeField] private float followSmoothness = 15f;

    private float currentZoom = 4f;

    private void Awake()
    {
        if (controlledCamera == null)
            controlledCamera = GetComponentInChildren<Camera>();

        currentZoom = Mathf.Clamp(
            offset.magnitude, minZoom, maxZoom);
    }

    private void LateUpdate()
    {
        if (target == null || controlledCamera == null)
            return;

        if (Mouse.current != null)
        {
            currentZoom -=
                Mouse.current.scroll.ReadValue().y
                * 0.01f * zoomSpeed;
        }

        currentZoom = Mathf.Clamp(
            currentZoom, minZoom, maxZoom);

        Vector3 cameraOffset =
            new Vector3(0f, 0f, -currentZoom);

        Vector3 desiredPosition =
            transform.position +
            transform.rotation * cameraOffset;

        controlledCamera.transform.position =
            Vector3.Lerp(
                controlledCamera.transform.position,
                desiredPosition,
                followSmoothness * Time.deltaTime);

        controlledCamera.transform.LookAt(
            transform.position);
    }
}
