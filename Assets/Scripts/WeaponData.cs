using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Melee Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public Sprite weaponSprite;
    public float damage = 10f;
    public float attackCooldown = 0.5f;

    [Header("Hitbox Settings")]
    public Vector2 hitboxSize = new Vector2(1.2f, 0.8f);   
    public Vector2 hitboxOffset = new Vector2(0.8f, 0f);    

    [Header("Knockback Settings")]
    public float knockbackForce = 15f;      // How hard the enemy is pushed back
    public float knockbackDuration = 0.15f;  // How long the enemy slides backward [1]
}