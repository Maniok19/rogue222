using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnData
    {
        public GameObject enemyPrefab;
        public Transform spawnPoint;
    }

    [Header("Spawn Configuration")]
    [SerializeField] private SpawnData[] enemiesToSpawn;
    [SerializeField] private GameObject spawnVFXPrefab; // Optionnel : effet de fumée / invocation

    public void SpawnEnemies()
    {
        foreach (var spawn in enemiesToSpawn)
        {
            if (spawn.enemyPrefab == null || spawn.spawnPoint == null) continue;

            // Optionnel : Jouer un effet visuel au point de spawn
            if (spawnVFXPrefab != null)
            {
                Instantiate(spawnVFXPrefab, spawn.spawnPoint.position, Quaternion.identity);
            }

            // Instanciation de l'ennemi
            Instantiate(spawn.enemyPrefab, spawn.spawnPoint.position, Quaternion.identity);
        }
    }
}