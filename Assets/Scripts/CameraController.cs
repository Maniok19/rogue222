using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target; 
    public float smoothSpeed = 5f;

    [Header("Deadzone (Screen Margins)")]
    [Range(0.01f, 0.45f)] 
    public float horizontalMargin = 0.25f; 
    [Range(0.01f, 0.45f)] 
    public float verticalMargin = 0.25f;   

    [Header("Zoom Settings")]
    public float minZoom = 3f;             
    public float maxZoom = 15f;            
    public float zoomSpeed = 5f;           
    public float zoomSensitivity = 0.01f;  

    [Header("Input System Actions")]
    [Tooltip("Link this to your project's Zoom/Scroll action.")]
    public InputActionProperty zoomAction; // Best practice: Serialized Action Property

    private Camera cam;
    private float targetZoom;

    void OnEnable()
    {
        // Best practice: Enable the input action when the script is active
        zoomAction.action?.Enable();
    }

    void OnDisable()
    {
        // Disable it when inactive to free up memory
        zoomAction.action?.Disable();
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            targetZoom = cam.orthographicSize;
        }
    }

    void LateUpdate()
    {
        if (target == null || cam == null) return;

        HandleCameraFollow();
        HandleCameraZoom();
    }

    void HandleCameraFollow()
    {
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        float leftBoundary = transform.position.x - (camWidth * 0.5f) + (camWidth * horizontalMargin);
        float rightBoundary = transform.position.x + (camWidth * 0.5f) - (camWidth * horizontalMargin);
        float bottomBoundary = transform.position.y - (camHeight * 0.5f) + (camHeight * verticalMargin);
        float topBoundary = transform.position.y + (camHeight * 0.5f) - (camHeight * verticalMargin);

        Vector3 targetCamPos = transform.position;

        if (target.position.x < leftBoundary)
        {
            targetCamPos.x += (target.position.x - leftBoundary);
        }
        else if (target.position.x > rightBoundary)
        {
            targetCamPos.x += (target.position.x - rightBoundary);
        }

        if (target.position.y < bottomBoundary)
        {
            targetCamPos.y += (target.position.y - bottomBoundary);
        }
        else if (target.position.y > topBoundary)
        {
            targetCamPos.y += (target.position.y - topBoundary);
        }

        targetCamPos.z = transform.position.z;

        transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothSpeed * Time.deltaTime);
    }

    void HandleCameraZoom()
    {
        float scrollInput = 0f;

        // Read value directly from the bound action asset
        if (zoomAction.action != null)
        {
            // Scroll actions generally output a Vector2. We read the Y value for scroll direction.
            scrollInput = zoomAction.action.ReadValue<Vector2>().y;
        }

        if (Mathf.Abs(scrollInput) > 0.1f)
        {
            targetZoom -= scrollInput * zoomSensitivity;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, zoomSpeed * Time.deltaTime);
    }
}