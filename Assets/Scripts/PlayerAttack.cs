using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [System.Serializable]
    public struct WeaponOffset
    {
        public Vector3 positionOffset;     // Shift the weapon to align with the hand
        public float rotationOffset;        // Angled resting rotation in hand
        public int sortingOrderOffset;      // e.g., -1 to put behind player (back view), +1 for front
    }

    [Header("Weapon Setup")]
    public WeaponData equippedWeapon;
    public LayerMask enemyLayers;

    [Header("Visual Hand Offsets")]
    public WeaponOffset frontOffset;        // Offsets when facing Down
    public WeaponOffset backOffset;         // Offsets when facing Up
    public WeaponOffset sideOffset;         // Offsets when facing Side (automatically mirrors via scale)

    [Header("Visual Elements")]
    public Transform weaponAnchor;       
    public SpriteRenderer weaponSpriteRenderer; 
    public float swingAngle = 90f;       
    public float swingDuration = 0.15f;   

    [Header("Slash FX")]
    public GameObject slashVFXPrefab;       // Drag your "SlashVisual" prefab here
    public float slashDistance = 0.6f;      // How far in front of the player the slash spawns

    [Header("Input Action")]
    public InputActionProperty attackAction;

    private PlayerController playerController;
    private float nextAttackTime;
    private bool isAttacking;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        
        // Hide the weapon visual initially if no weapon is equipped
        if (weaponAnchor != null)
            weaponAnchor.gameObject.SetActive(false);
    }

    private void OnEnable() => attackAction.action?.Enable();
    private void OnDisable() => attackAction.action?.Disable();

    private void Update()
    {
        if (equippedWeapon == null) return;

        if (!isAttacking && attackAction.action != null && attackAction.action.WasPressedThisFrame())
        {
            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(PerformSwingAttack());
                nextAttackTime = Time.time + equippedWeapon.attackCooldown;
            }
        }
    }

    private void LateUpdate()
    {
        // Continuously update the hand positioning while not swinging
        if (equippedWeapon == null || isAttacking) return;
        UpdateWeaponRestingPosition();
    }

    public void EquipWeapon(WeaponData newWeapon)
    {
        equippedWeapon = newWeapon;
        if (weaponSpriteRenderer != null)
        {
            weaponSpriteRenderer.sprite = newWeapon.weaponSprite;
        }
        if (weaponAnchor != null)
        {
            weaponAnchor.gameObject.SetActive(true);
        }
    }

    // Helper method to get the correct offset config based on direction
    private WeaponOffset GetActiveOffset(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > 0.1f)
        {
            return sideOffset;
        }
        else if (dir.y > 0.1f)
        {
            return backOffset;
        }
        else
        {
            return frontOffset;
        }
    }

    private void UpdateWeaponRestingPosition()
    {
        if (weaponAnchor == null || playerController == null) return;

        Vector2 dir = playerController.LastMoveDirection;
        WeaponOffset activeOffset = GetActiveOffset(dir);

        // Apply resting offsets
        weaponAnchor.localPosition = activeOffset.positionOffset;
        weaponAnchor.localRotation = Quaternion.Euler(0, 0, activeOffset.rotationOffset);

        // Apply sorting order relative to player
        if (weaponSpriteRenderer != null)
        {
            SpriteRenderer playerSR = GetComponent<SpriteRenderer>();
            if (playerSR != null)
            {
                weaponSpriteRenderer.sortingOrder = playerSR.sortingOrder + activeOffset.sortingOrderOffset;
            }
        }
    }

    private IEnumerator PerformSwingAttack()
    {
        isAttacking = true;
        Vector2 attackDir = playerController.LastMoveDirection;

        // 1. Get the current hand offset profile
        WeaponOffset activeOffset = GetActiveOffset(attackDir);

        // 2. Snap the anchor position to the correct hand position immediately
        weaponAnchor.localPosition = activeOffset.positionOffset;

        // 3. Calculate target angle relative to the player's scale (to handle horizontal flipping)
        float scaleSign = Mathf.Sign(transform.localScale.x);
        Vector2 localAttackDir = new Vector2(attackDir.x * scaleSign, attackDir.y);
        float localTargetAngle = Mathf.Atan2(localAttackDir.y, localAttackDir.x) * Mathf.Rad2Deg;

        // 4. Center the swing rotation around the hand's custom resting angle [3]
        float baseSwingAngle = localTargetAngle + activeOffset.rotationOffset;

        // Define start and end local rotations based on the hand-aligned base angle
        Quaternion startRotation = Quaternion.Euler(0, 0, baseSwingAngle + (swingAngle / 2f));
        Quaternion endRotation = Quaternion.Euler(0, 0, baseSwingAngle - (swingAngle / 2f));

        weaponAnchor.gameObject.SetActive(true);
        weaponAnchor.localRotation = startRotation;

        // Render weapon depth layer during swing
        if (weaponSpriteRenderer != null)
        {
            SpriteRenderer playerSR = GetComponent<SpriteRenderer>();
            if (playerSR != null)
            {
                weaponSpriteRenderer.sortingOrder = (attackDir.y > 0.1f) ? playerSR.sortingOrder - 1 : playerSR.sortingOrder + 1;
            }
        }

        // Instantiate Slash Effect in front of the player (Slash remains in absolute world space)
        if (slashVFXPrefab != null)
        {
            float worldTargetAngle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;
            Vector3 slashPos = transform.position + (Vector3)(attackDir * slashDistance);
            Instantiate(slashVFXPrefab, slashPos, Quaternion.Euler(0, 0, worldTargetAngle));
        }

        // Perform the smooth rotation swing
        float elapsed = 0f;
        while (elapsed < swingDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / swingDuration;
            weaponAnchor.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        DetectHits(attackDir);

        isAttacking = false;
        UpdateWeaponRestingPosition(); // Instantly snap back to standard hand orientation
    }

    private void DetectHits(Vector2 attackDir)
    {
        // Physics checks remain in absolute world coordinates
        float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;
        Vector3 rotatedOffset = Quaternion.Euler(0, 0, angle) * (Vector3)equippedWeapon.hitboxOffset;
        Vector2 hitboxCenter = (Vector2)transform.position + (Vector2)rotatedOffset;

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(
            hitboxCenter, 
            equippedWeapon.hitboxSize, 
            angle, 
            enemyLayers
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(equippedWeapon.damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        PlayerController controller = playerController;
        if (controller == null)
        {
            controller = GetComponent<PlayerController>();
        }

        if (equippedWeapon == null) return;

        Vector2 attackDir = Vector2.down; 
        if (controller != null)
        {
            attackDir = Application.isPlaying ? controller.LastMoveDirection : Vector2.down;
        }

        float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;
        Vector3 rotatedOffset = Quaternion.Euler(0, 0, angle) * (Vector3)equippedWeapon.hitboxOffset;
        Vector3 hitboxCenter = transform.position + rotatedOffset;

        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(hitboxCenter, Quaternion.Euler(0, 0, angle), Vector3.one);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(equippedWeapon.hitboxSize.x, equippedWeapon.hitboxSize.y, 0.1f));

        Gizmos.matrix = originalMatrix;
    }
}