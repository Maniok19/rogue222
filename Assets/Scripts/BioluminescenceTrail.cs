using System.Collections;
using UnityEngine;

public class BioluminescenceTrail : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private float damagePerTick = 2f;    // Dégâts par "tick"
    [SerializeField] private float tickInterval = 0.5f;   // Intervalle entre chaque "tick" de dégâts
    [SerializeField] private float lifetime = 2.5f;       // Durée de vie de la flaque

    private Collider2D myCollider;

private void Start()
{
    myCollider = GetComponent<Collider2D>();
    
    // --- SÉCURITÉ INFINITE LOOP ---
    // Si l'intervalle est réglé à 0 ou moins par accident dans l'inspecteur,
    // on le force à 0.1s pour empêcher Unity de geler et de crasher.
    if (tickInterval <= 0f)
    {
        tickInterval = 0.1f;
    }

    // Détruit la flaque proprement après quelques secondes
    Destroy(gameObject, lifetime);
}

private IEnumerator DamageOverTimeRoutine(IDamageable target)
{
    yield return new WaitForSeconds(tickInterval); 

    Component targetComponent = target as Component;

    while (target != null && targetComponent != null && gameObject != null && myCollider != null) 
    {
        if (myCollider.OverlapPoint(targetComponent.transform.position))
        {
            target.TakeDamage(damagePerTick);
        }
        yield return new WaitForSeconds(tickInterval);
    }
}

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // Lance la coroutine de dégâts sur la durée pour cet ennemi
            StartCoroutine(DamageOverTimeRoutine(damageable));
        }
    }
}
