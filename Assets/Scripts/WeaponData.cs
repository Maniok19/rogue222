using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Melee Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public Sprite weaponSprite;
    public float damage = 10f;
    public float attackCooldown = 0.5f;

    [Header("Hitbox Settings")]
    public Vector2 hitboxSize = new Vector2(1.2f, 0.8f);   // Width and height of the box
    public Vector2 hitboxOffset = new Vector2(0.8f, 0f);    // How far in front of the player it spawns
}