using System.Collections;
using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    
    [Header("Follow Script settings")]
    [Tooltip("If left empty, this will automatically detect your CameraController component on Awake.")]
    [SerializeField] private MonoBehaviour cameraFollowScript; 

    private float originalSize;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            originalSize = targetCamera.orthographicSize;
        }

        // Automatically find your follow/zoom script
        if (cameraFollowScript == null)
        {
            cameraFollowScript = GetComponent<CameraController>();
        }
    }

    public IEnumerator ChangeZoomAndPosition(float targetSize, Vector3 targetPos, float duration)
    {
        if (targetCamera == null) yield break;

        // Disables CameraController entirely while zooming and waiting
        SetFollowActive(false);

        float startSize = targetCamera.orthographicSize;
        Vector3 startPos = targetCamera.transform.position;
        targetPos.z = startPos.z;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t); // SmoothStep Easing

            targetCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, smoothT);
            targetCamera.transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            yield return null;
        }

        targetCamera.orthographicSize = targetSize;
        targetCamera.transform.position = targetPos;
    }

    public IEnumerator ResetZoomAndPosition(Transform playerTransform, float duration)
    {
        if (targetCamera == null || playerTransform == null) yield break;

        float startSize = targetCamera.orthographicSize;
        Vector3 startPos = targetCamera.transform.position;
        
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t); // SmoothStep Easing

            // --- DYNAMIC POSITION TRACKING ---
            // We read the player's position every frame instead of caching it once.
            // This ensures the camera tracks the player smoothly even if they are moving.
            Vector3 currentTargetPos = playerTransform.position;
            currentTargetPos.z = startPos.z;

            targetCamera.orthographicSize = Mathf.Lerp(startSize, originalSize, smoothT);
            targetCamera.transform.position = Vector3.Lerp(startPos, currentTargetPos, smoothT);
            yield return null;
        }

        // Final snap to the player's actual position when ending
        if (playerTransform != null)
        {
            Vector3 finalTargetPos = playerTransform.position;
            finalTargetPos.z = startPos.z;
            targetCamera.transform.position = finalTargetPos;
        }

        targetCamera.orthographicSize = originalSize;

        // Re-enable the player follow and zoom script now that we are done
        SetFollowActive(true);
    }

    private void SetFollowActive(bool active)
    {
        if (cameraFollowScript != null)
        {
            cameraFollowScript.enabled = active;
        }
    }
}