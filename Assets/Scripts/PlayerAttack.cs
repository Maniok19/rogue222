using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [System.Serializable]
    public struct WeaponOffset
    {
        public Transform handAnchor;        
        public float rotationOffset;        
        public int sortingOrderOffset;      
    }

    [Header("Weapon Slots")]
    private WeaponData[] weaponSlots = new WeaponData[2]; // Index 0: Arme principale, Index 1: Arme secondaire
    private int currentWeaponIndex = 0;

    // Propriété publique pour récupérer l'arme actuellement active
    public WeaponData equippedWeapon => weaponSlots[currentWeaponIndex];

    [Header("Physics & Layers")]
    public LayerMask enemyLayers;

    [Header("Visual Hand Offsets")]
    public WeaponOffset frontOffset;        
    public WeaponOffset backOffset;         
    public WeaponOffset sideOffset;         

    [Header("Visual Elements")]
    public Transform weaponAnchor;       
    public SpriteRenderer weaponSpriteRenderer; 
    public float swingAngle = 90f;       
    public float swingDuration = 0.15f;   

    [Header("Slash FX (Melee Only)")]
    public GameObject slashVFXPrefab;       
    public float slashDistance = 0.6f;      

    [Header("Input Actions")]
    public InputActionProperty attackAction;
    public InputActionProperty switchWeaponAction; // <-- Action pour changer d'arme (ex: touche 'Q' ou molette)

    private PlayerController playerController;
    private float nextAttackTime;
    private bool isAttacking;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        
        if (weaponAnchor != null)
            weaponAnchor.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        attackAction.action?.Enable();
        switchWeaponAction.action?.Enable();
    }

    private void OnDisable()
    {
        attackAction.action?.Disable();
        switchWeaponAction.action?.Disable();
    }

    private void Update()
    {
        // Changement d'arme si l'input est pressé et qu'on n'est pas en train d'attaquer
        if (!isAttacking && switchWeaponAction.action != null && switchWeaponAction.action.WasPressedThisFrame())
        {
            SwitchWeapon();
        }

        if (equippedWeapon == null) return;

        // Attaque
        if (!isAttacking && attackAction.action != null && attackAction.action.WasPressedThisFrame())
        {
            if (Time.time >= nextAttackTime)
            {
                if (equippedWeapon.weaponType == WeaponType.Melee)
                {
                    StartCoroutine(PerformSwingAttack());
                }
                else if (equippedWeapon.weaponType == WeaponType.Ranged)
                {
                    StartCoroutine(PerformRangedAttack());
                }

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
        // Logique de ramassage intelligente :
        // Si l'emplacement 1 est vide, on l'y met. Sinon, si l'emplacement 2 est vide, on l'y met.
        // Sinon, on remplace l'arme actuellement tenue.
        if (weaponSlots[0] == null)
        {
            weaponSlots[0] = newWeapon;
            currentWeaponIndex = 0;
        }
        else if (weaponSlots[1] == null)
        {
            weaponSlots[1] = newWeapon;
            currentWeaponIndex = 1;
        }
        else
        {
            weaponSlots[currentWeaponIndex] = newWeapon;
        }

        UpdateWeaponVisuals();
    }

    private void SwitchWeapon()
    {
        // On ne change d'arme que si on possède au moins une autre arme
        int nextIndex = (currentWeaponIndex + 1) % 2;
        if (weaponSlots[nextIndex] != null)
        {
            currentWeaponIndex = nextIndex;
            UpdateWeaponVisuals();
            Debug.Log($"Arme équipée : {equippedWeapon.weaponName}");
        }
    }

    private void UpdateWeaponVisuals()
    {
        if (equippedWeapon != null)
        {
            if (weaponSpriteRenderer != null)
            {
                weaponSpriteRenderer.sprite = equippedWeapon.weaponSprite;
            }
            if (weaponAnchor != null)
            {
                weaponAnchor.gameObject.SetActive(true);
            }
            UpdateWeaponRestingPosition();
        }
        else
        {
            if (weaponAnchor != null)
                weaponAnchor.gameObject.SetActive(false);
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
        if (weaponAnchor == null || playerController == null || equippedWeapon == null) return;

        Vector2 dir = playerController.SnappedMoveDirection;
        WeaponOffset activeOffset = GetActiveOffset(dir);

        if (activeOffset.handAnchor != null)
        {
            weaponAnchor.localPosition = activeOffset.handAnchor.localPosition;
        }

        weaponAnchor.localRotation = Quaternion.Euler(0, 0, activeOffset.rotationOffset);

        if (weaponSpriteRenderer != null)
        {
            SpriteRenderer playerSR = GetComponent<SpriteRenderer>();
            if (playerSR != null)
            {
                weaponSpriteRenderer.sortingOrder = playerSR.sortingOrder + activeOffset.sortingOrderOffset;
            }
        }
    }

    // --- ATTAQUE DE MÊLÉE ---
    private IEnumerator PerformSwingAttack()
    {
        isAttacking = true;
        Vector2 attackDir = playerController.SnappedMoveDirection;
        WeaponOffset activeOffset = GetActiveOffset(attackDir);

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

    // --- ATTAQUE À DISTANCE ---
    private IEnumerator PerformRangedAttack()
    {
        isAttacking = true;
        Vector2 attackDir = playerController.SnappedMoveDirection;
        WeaponOffset activeOffset = GetActiveOffset(attackDir);

        // Aligne l'arme dans la main
        if (activeOffset.handAnchor != null)
        {
            weaponAnchor.localPosition = activeOffset.handAnchor.localPosition;
        }

        // Oriente l'arme visuellement vers la cible
        float scaleSign = Mathf.Sign(playerController.rigTransform != null ? playerController.rigTransform.localScale.x : transform.localScale.x);
        Vector2 localAttackDir = new Vector2(attackDir.x * scaleSign, attackDir.y);
        float localTargetAngle = Mathf.Atan2(localAttackDir.y, localAttackDir.x) * Mathf.Rad2Deg;
        weaponAnchor.localRotation = Quaternion.Euler(0, 0, localTargetAngle + activeOffset.rotationOffset);

        // Apparition du projectile
        if (equippedWeapon.projectilePrefab != null)
        {
            // Fait apparaître le projectile légèrement devant le joueur
            Vector3 spawnPos = weaponAnchor.position + (Vector3)(attackDir * 0.4f);
            GameObject projGO = Instantiate(equippedWeapon.projectilePrefab, spawnPos, Quaternion.identity);
            
            Projectile projectileInstance = projGO.GetComponent<Projectile>();
            if (projectileInstance != null)
            {
                projectileInstance.Setup(
                    attackDir, 
                    equippedWeapon.damage, 
                    equippedWeapon.knockbackForce, 
                    equippedWeapon.knockbackDuration, 
                    equippedWeapon.projectileSpeed
                );
            }
        }

        // Temps d'animation de tir de l'arme (recul visuel)
        yield return new WaitForSeconds(0.12f);

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

        if (equippedWeapon == null || equippedWeapon.weaponType == WeaponType.Ranged) return;

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