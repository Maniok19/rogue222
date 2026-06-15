using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [System.Serializable]
    public struct WeaponOffset
    {
        public Transform handAnchor;        // <-- REMPLACÉ : On glisse ici l'objet Hand_Front, Hand_Back ou Hand_Side
        public float rotationOffset;        // Rotation de repos dans la main
        public int sortingOrderOffset;      // e.g., -1 pour mettre derrière le joueur (dos), +1 pour devant
    }

    [Header("Weapon Setup")]
    public WeaponData equippedWeapon;
    public LayerMask enemyLayers;

    [Header("Visual Hand Offsets")]
    public WeaponOffset frontOffset;        // Config pour la vue de Face (Bas)
    public WeaponOffset backOffset;         // Config pour la vue de Dos (Haut)
    public WeaponOffset sideOffset;         // Config pour la vue de Profil (Côté)

    [Header("Visual Elements")]
    public Transform weaponAnchor;       
    public SpriteRenderer weaponSpriteRenderer; 
    public float swingAngle = 90f;       
    public float swingDuration = 0.15f;   

    [Header("Slash FX")]
    public GameObject slashVFXPrefab;       
    public float slashDistance = 0.6f;      

    [Header("Input Action")]
    public InputActionProperty attackAction;

    private PlayerController playerController;
    private float nextAttackTime;
    private bool isAttacking;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        
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

        Vector2 dir = playerController.SnappedMoveDirection;
        WeaponOffset activeOffset = GetActiveOffset(dir);

        // Aligne automatiquement l'arme sur la position locale de l'ancre de la main active
        if (activeOffset.handAnchor != null)
        {
            weaponAnchor.localPosition = activeOffset.handAnchor.localPosition;
        }

        weaponAnchor.localRotation = Quaternion.Euler(0, 0, activeOffset.rotationOffset);

        // Applique l'ordre de tri (sorting order)
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
        Vector2 attackDir = playerController.SnappedMoveDirection;
        WeaponOffset activeOffset = GetActiveOffset(attackDir);

        // Snap immédiat sur la bonne main au début de l'attaque
        if (activeOffset.handAnchor != null)
        {
            weaponAnchor.localPosition = activeOffset.handAnchor.localPosition;
        }

        float scaleSign = Mathf.Sign(playerController.rigTransform != null ? playerController.rigTransform.localScale.x : transform.localScale.x);
        Vector2 localAttackDir = new Vector2(attackDir.x * scaleSign, attackDir.y);
        float localTargetAngle = Mathf.Atan2(localAttackDir.y, localAttackDir.x) * Mathf.Rad2Deg;

        float baseSwingAngle = localTargetAngle + activeOffset.rotationOffset;

        Quaternion startRotation = Quaternion.Euler(0, 0, baseSwingAngle + (swingAngle / 2f));
        Quaternion endRotation = Quaternion.Euler(0, 0, baseSwingAngle - (swingAngle / 2f));

        weaponAnchor.gameObject.SetActive(true);
        weaponAnchor.localRotation = startRotation;

        if (weaponSpriteRenderer != null)
        {
            SpriteRenderer playerSR = GetComponent<SpriteRenderer>();
            if (playerSR != null)
            {
                weaponSpriteRenderer.sortingOrder = (attackDir.y > 0.1f) ? playerSR.sortingOrder - 1 : playerSR.sortingOrder + 1;
            }
        }

        if (slashVFXPrefab != null)
        {
            float worldTargetAngle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;
            Vector3 slashPos = transform.position + (Vector3)(attackDir * slashDistance);
            Instantiate(slashVFXPrefab, slashPos, Quaternion.Euler(0, 0, worldTargetAngle));
        }

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
        UpdateWeaponRestingPosition(); 
    }

    private void DetectHits(Vector2 attackDir)
    {
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

            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                enemyAI.ApplyKnockback(knockbackDir, equippedWeapon.knockbackForce, equippedWeapon.knockbackDuration);
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
            attackDir = Application.isPlaying ? controller.SnappedMoveDirection : Vector2.down;
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