using System.Collections.Generic;
using UnityEngine;

public class CharmInventory : MonoBehaviour
{
    // Singleton pour y accéder facilement depuis les projectiles
    public static CharmInventory Instance { get; private set; }

    [Header("Charm Bag Settings")]
    [SerializeField] private int maxCapacity = 3;
    [SerializeField] private List<CharmData> equippedCharms = new List<CharmData>();

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

        if (equippedCharms == null)
        {
            equippedCharms = new List<CharmData>();
            Debug.LogWarning("equippedCharms était null et a été réinitialisé. Vérifiez votre configuration dans l'inspecteur.");
        }
    }

    // Tente d'équiper un charme
    public bool EquipCharm(CharmData charm)
    {
        if (equippedCharms.Contains(charm))
        {
            Debug.Log($"{charm.charmName} est déjà équipé !");
            return false;
        }

        if (equippedCharms.Count >= maxCapacity)
        {
            Debug.Log("La besace est pleine ! Impossible d'équiper.");
            return false;
        }

        equippedCharms.Add(charm);
        Debug.Log($"Charme équipé : {charm.charmName}");
        return true;
    }

    // Permet de déséquiper un charme
    public void UnequipCharm(CharmData charm)
    {
        if (equippedCharms.Contains(charm))
        {
            equippedCharms.Remove(charm);
            Debug.Log($"Charme retiré : {charm.charmName}");
        }
    }

    // Permet à n'importe quel script (ex: Projectile) de savoir si un charme est actif
    public bool HasCharm(CharmType type)
    {
        foreach (var charm in equippedCharms)
        {
            if (charm != null && charm.charmType == type)
            {
                return true;
            }
        }
        return false;
    }

    public List<CharmData> GetEquippedCharms() => equippedCharms;
}