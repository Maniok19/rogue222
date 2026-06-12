using UnityEngine;
using UnityEngine.InputSystem; // Uses your project's active Input System

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Drag your Player GameObject here
    public float smoothSpeed = 5f;

    [Header("Deadzone (Screen Margins)")]
    [Range(0.01f, 0.45f)] 
    public float horizontalMargin = 0.25f; // 25% screen margin from left/right edges
    [Range(0.01f, 0.45f)] 
    public float verticalMargin = 0.25f;   // 25% screen margin from top/bottom edges

    [Header("Zoom Settings")]
    public float minZoom = 3f;             // How close you can zoom in
    public float maxZoom = 15f;            // How far you can zoom out
    public float zoomSpeed = 5f;           // How smoothly the camera zooms
    public float zoomSensitivity = 0.01f;  // How fast the zoom reacts to the scroll wheel

    private Camera cam;
    private float targetZoom;

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
        // 1. Calculate the current visible bounds of the camera
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        // 2. Define the deadzone box boundaries relative to the camera's current position
        float leftBoundary = transform.position.x - (camWidth * 0.5f) + (camWidth * horizontalMargin);
        float rightBoundary = transform.position.x + (camWidth * 0.5f) - (camWidth * horizontalMargin);
        float bottomBoundary = transform.position.y - (camHeight * 0.5f) + (camHeight * verticalMargin);
        float topBoundary = transform.position.y + (camHeight * 0.5f) - (camHeight * verticalMargin);

        Vector3 targetCamPos = transform.position;

        // 3. Check if the player has pushed past the horizontal boundaries
        if (target.position.x < leftBoundary)
        {
            targetCamPos.x += (target.position.x - leftBoundary);
        }
        else if (target.position.x > rightBoundary)
        {
            targetCamPos.x += (target.position.x - rightBoundary);
        }

        // 4. Check if the player has pushed past the vertical boundaries
        if (target.position.y < bottomBoundary)
        {
            targetCamPos.y += (target.position.y - bottomBoundary);
        }
        else if (target.position.y > topBoundary)
        {
            targetCamPos.y += (target.position.y - topBoundary);
        }

        // Keep the camera's Z axis position locked (usually -10)
        targetCamPos.z = transform.position.z;

        // 5. Smoothly interpolate (lerp) to the new target position
        transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothSpeed * Time.deltaTime);
    }

    void HandleCameraZoom()
    {
        float scrollInput = 0f;

        // Read scroll wheel input from the New Input System
        if (Mouse.current != null)
        {
            scrollInput = Mouse.current.scroll.ReadValue().y;
        }

        if (Mathf.Abs(scrollInput) > 0.1f)
        {
            // Adjust the target zoom based on scroll wheel input direction
            targetZoom -= scrollInput * zoomSensitivity;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // Smoothly transition the camera's Orthographic Size to the target zoom
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, zoomSpeed * Time.deltaTime);
    }
}