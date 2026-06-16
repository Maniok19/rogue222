using System.Collections;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private CameraZoomController cameraController;

    [Header("Room Sequence Settings")]
    [SerializeField] private float zoomOutSize = 8.0f;       // Taille de la caméra lors du dézoom
    [SerializeField] private float zoomDuration = 0.8f;       // Vitesse du zoom/dézoom
    [SerializeField] private float waitTimeBeforeSpawn = 0.5f; // Pause dramatique avant le spawn
    [SerializeField] private float waitTimeAfterSpawn = 0.5f;  // Pause après l'apparition des monstres

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                hasTriggered = true; // Empêche de déclencher la pièce plusieurs fois
                StartCoroutine(PlayRoomEntrySequence(player));
            }
        }
    }

    private IEnumerator PlayRoomEntrySequence(PlayerController player)
    {
        // 1. Geler le joueur
        player.SetInputActive(false);

        // 2. Dézoomer la caméra
        if (cameraController != null)
        {
            yield return cameraController.ChangeZoom(zoomOutSize, zoomDuration);
        }

        // 3. Petite attente dramatique
        yield return new WaitForSeconds(waitTimeBeforeSpawn);

        // 4. Faire apparaître les monstres
        if (enemySpawner != null)
        {
            enemySpawner.SpawnEnemies();
        }

        // 5. Attente après l'apparition des monstres
        yield return new WaitForSeconds(waitTimeAfterSpawn);

        // 6. Rezoomer la caméra à sa taille initiale
        if (cameraController != null)
        {
            yield return cameraController.ResetZoom(zoomDuration);
        }

        // 7. Libérer le joueur
        player.SetInputActive(true);
    }
}