using UnityEngine;

// On définit les deux types d'armes possibles
public enum WeaponType { Melee, Ranged }

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public Sprite weaponSprite;
    public WeaponType weaponType; // Mêlée ou Distance
    public float damage = 10f;
    public float attackCooldown = 0.5f;

    [Header("Melee Hitbox Settings")]
    public Vector2 hitboxSize = new Vector2(1.2f, 0.8f);   
    public Vector2 hitboxOffset = new Vector2(0.8f, 0f);    

    [Header("Ranged Settings")]
    public GameObject projectilePrefab; // Le prefab de la flèche, balle, etc.
    public float projectileSpeed = 12f;

    [Header("Knockback Settings")]
    public float knockbackForce = 15f;      
    public float knockbackDuration = 0.15f;  
}