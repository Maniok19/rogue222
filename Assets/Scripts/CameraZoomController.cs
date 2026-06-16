using System.Collections;
using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
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
    }

    // Coroutine pour effectuer un zoom ou dézoom fluide
    public IEnumerator ChangeZoom(float targetSize, float duration)
    {
        if (targetCamera == null) yield break;

        float startSize = targetCamera.orthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            targetCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, elapsed / duration);
            yield return null;
        }

        targetCamera.orthographicSize = targetSize;
    }

    // Permet de revenir facilement au zoom initial du jeu
    public IEnumerator ResetZoom(float duration)
    {
        yield return ChangeZoom(originalSize, duration);
    }
}