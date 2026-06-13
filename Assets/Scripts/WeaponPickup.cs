using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public WeaponData weaponData;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        Debug.Log("weapon start");
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && weaponData != null)
        {
            spriteRenderer.sprite = weaponData.weaponSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ensure your Player GameObject has the "Player" tag assigned
        Debug.Log("weapon collider triggered");
        if (other.CompareTag("Player"))
        {
            PlayerAttack playerAttack = other.GetComponent<PlayerAttack>();
            if (playerAttack != null)
            {
                playerAttack.EquipWeapon(weaponData);
                Destroy(gameObject); // Remove the item from the ground
            }
        }
    }
}