using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Coroutine activeShake;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // No Start() method needed anymore!

    public void Shake(float duration, float magnitude)
    {
        if (activeShake != null)
        {
            StopCoroutine(activeShake);
        }
        activeShake = StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;
        Vector3 lastOffset = Vector3.zero;

        while (elapsed < duration)
        {
            // 1. Remove the previous frame's offset first to restore 
            // the camera to wherever your follow script has moved it.
            transform.localPosition -= lastOffset;

            // 2. Generate a new random offset
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            Vector3 newOffset = new Vector3(x, y, 0f);

            // 3. Apply the new offset
            transform.localPosition += newOffset;
            
            // 4. Store this offset to subtract it on the next frame
            lastOffset = newOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 5. Clean up the final offset to return the camera exactly back to normal
        transform.localPosition -= lastOffset;
        activeShake = null;
    }
}