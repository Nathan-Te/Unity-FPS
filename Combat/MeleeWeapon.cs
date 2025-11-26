using UnityEngine;
using System.Collections;

public class MeleeWeapon : WeaponBase
{
    [Header("Paramètres Mêlée")]
    public float attackRange = 2.5f;   // Portée (courte)
    public float attackRadius = 0.5f;  // Rayon de la "bûche" (Hitbox large)
    public float damage = 25f;
    public float impactForce = 15f;
    public LayerMask hitLayers;

    [Header("Game Feel (Juice)")]
    [Tooltip("Temps de gel à l'impact (ex: 0.1s). 0 = Désactivé.")]
    public float hitStopDuration = 0.1f;

    protected override void ExecuteFireLogic()
    {
        // 1. Origine du coup (Depuis les yeux)
        Transform eyes = Camera.main.transform;
        RaycastHit hit;

        // 2. SphereCast : On lance une sphère vers l'avant (plus facile de toucher qu'un Raycast)
        // C'est comme donner un coup de poing avec une grosse hitbox
        if (Physics.SphereCast(eyes.position, attackRadius, eyes.forward, out hit, attackRange, hitLayers))
        {
            // --- IMPACT ---
            Debug.Log($"Coup porté sur : {hit.collider.name}");

            // A. Dégâts
            IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            // B. Physique (Poussée)
            Rigidbody rb = hit.rigidbody;
            if (rb != null)
            {
                // On pousse dans la direction du regard
                rb.AddForceAtPosition(eyes.forward * impactForce, hit.point, ForceMode.Impulse);
            }

            // C. Surface FX (Son + Particules)
            if (SurfaceManager.Instance != null)
            {
                // On utilise le système d'impact existant (bruit de choc, poussière...)
                // Tu pourras créer des "SurfaceDefinition" spécifiques pour le métal frappé par un couteau plus tard
                SurfaceManager.Instance.PlayBulletImpact(hit.point, hit.normal, hit);
            }

            // D. Hit Stop (Le secret du "Heavy Feel")
            if (hitStopDuration > 0)
            {
                StartCoroutine(HitStopRoutine());
            }
        }
    }

    IEnumerator HitStopRoutine()
    {
        // On gèle le temps
        Time.timeScale = 0.05f; // Presque à l'arrêt

        // On attend (en temps réel, car Time.timeScale affecte WaitForSeconds)
        yield return new WaitForSecondsRealtime(hitStopDuration);

        // On reprend
        Time.timeScale = 1f;
    }

    // Debug visuel de la portée
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (Camera.main != null)
        {
            Transform eyes = Camera.main.transform;
            Gizmos.DrawWireSphere(eyes.position + eyes.forward * attackRange, attackRadius);
        }
    }
}