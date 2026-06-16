using System.Collections;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private CameraZoomController cameraController;
    [SerializeField] private Transform cameraTargetPoint; 

    [Header("Room Sequence Settings")]
    [SerializeField] private float zoomOutSize = 8.0f;       
    [SerializeField] private float zoomDuration = 1.0f;       
    [SerializeField] private float waitTimeBeforeSpawn = 0.6f; 
    [SerializeField] private float waitTimeAfterSpawn = 0.8f; // Now this will be respected!

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                hasTriggered = true; 
                StartCoroutine(PlayRoomEntrySequence(player));
            }
        }
    }

    private IEnumerator PlayRoomEntrySequence(PlayerController player)
    {
        // 1. Freeze the player instantly
        player.SetInputActive(false);

        // 2. Unzoom and move camera to the center smoothly
        if (cameraController != null && cameraTargetPoint != null)
        {
            yield return cameraController.ChangeZoomAndPosition(zoomOutSize, cameraTargetPoint.position, zoomDuration);
        }

        // 3. Keep camera fixed and unzoomed before spawn for a brief dramatic pause
        yield return new WaitForSeconds(waitTimeBeforeSpawn);

        // 4. Spawn the mobs!
        if (enemySpawner != null)
        {
            enemySpawner.SpawnEnemies();
        }

        // 5. Trigger Camera Shake on spawn (if available)
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.35f, 0.15f);
        }

        // 8. Release the player at the very end of the sequence
        player.SetInputActive(true); 
        // 6. Keep camera fixed and unzoomed AFTER spawn so player can register the threat
        yield return new WaitForSeconds(waitTimeAfterSpawn);

        // 7. Zoom back in smoothly towards the player
        if (cameraController != null)
        {
            yield return cameraController.ResetZoomAndPosition(player.transform, zoomDuration);
        }


    }
}