using UnityEngine;

public class CharmPickup : MonoBehaviour
{
    public CharmData charmData;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && charmData != null)
        {
            spriteRenderer.sprite = charmData.charmSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CharmInventory inventory = other.GetComponent<CharmInventory>();
            if (inventory != null)
            {
                // Si la besace a de la place, on l'équipe et on détruit l'objet au sol
                if (inventory.EquipCharm(charmData))
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}