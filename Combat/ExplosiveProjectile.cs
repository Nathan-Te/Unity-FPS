using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ExplosiveProjectile : MonoBehaviour
{
    [Header("Type de Détonation")]
    [Tooltip("Cocher pour une Roquette (Explose au choc). Décocher pour une Grenade (Rebondit).")]
    public bool explodeOnContact = true;
    [Tooltip("Temps de la mèche (Grenade) ou Sécurité anti-fuite (Roquette)")]
    public float lifeTime = 5f;

    [Header("Paramètres Explosion")]
    public float damage = 50f;
    public float explosionRadius = 5f;
    public float explosionForce = 700f;

    [Header("VFX/SFX")]
    public GameObject explosionVFX;
    public AudioClip explosionSound;

    private bool _hasExploded = false;

    void Start()
    {
        // On lance le compte à rebours dès le départ.
        // - Si c'est une Grenade : C'est le moment où elle va péter.
        // - Si c'est une Roquette : C'est une sécurité si elle ne touche rien (pour pas qu'elle vole à l'infini).
        Invoke(nameof(Explode), lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Si on est en mode "Roquette", on explose au moindre contact solide
        if (explodeOnContact)
        {
            Explode();
        }
        // Sinon (Mode Grenade), on ne fait rien ici. 
        // Le Rigidbody va naturellement faire rebondir l'objet sur le mur/sol.
    }

    void Explode()
    {
        if (_hasExploded) return;
        _hasExploded = true;

        // 1. VFX & SFX
        if (explosionVFX) Instantiate(explosionVFX, transform.position, Quaternion.identity);
        if (explosionSound) AudioSource.PlayClipAtPoint(explosionSound, transform.position);

        // 2. Zone de dégâts (OverlapSphere)
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            // Dégâts
            IDamageable target = nearbyObject.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            // Physique (Explosion)
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        // 3. Destruction de l'objet lui-même
        Destroy(gameObject);
    }
}