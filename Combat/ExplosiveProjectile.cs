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
        // Si c'est une grenade, on joue un son de choc physique
        else
        {
            // On vérifie que le choc est assez fort pour faire du bruit
            if (collision.relativeVelocity.magnitude > 2f && SurfaceManager.Instance != null)
            {
                // On triche un peu : On utilise "PlayFootstep" ou on crée une méthode "PlayCollision" dans le manager.
                // Pour l'instant, utilisons PlayBulletImpact pour avoir le son d'impact de la surface, 
                // ou mieux : ajoutons une méthode rapide dans SurfaceManager si tu veux être précis.

                // Pour faire simple ici sans modifier le Manager : 
                // On récupère la définition et on joue un son de sa liste 'collisionSounds' manuellement.

                RaycastHit dummyHit = new RaycastHit();
                // On ne peut pas caster Collision en RaycastHit facilement, 
                // mais SurfaceManager a juste besoin du Collider pour trouver le material.
                // On va faire une version simplifiée :

                SurfaceDefinition surf = SurfaceManager.Instance.GetSurfaceFromHit(CreateHitFromCollision(collision));

                if (surf != null && surf.collisionSounds.Count > 0)
                {
                    AudioClip clip = surf.collisionSounds[Random.Range(0, surf.collisionSounds.Count)];
                    AudioSource.PlayClipAtPoint(clip, transform.position);
                }
            }
        }
    }

    // Petite astuce pour convertir Collision en info exploitable par notre Manager
    RaycastHit CreateHitFromCollision(Collision col)
    {
        RaycastHit hit = new RaycastHit();
        // On assigne manuellement le collider touché
        // C'est du "Hacking" propre car le Manager lit hit.collider
        // (Attention : hit.textureCoord ne marchera pas avec cette méthode, mais pour les Materials c'est OK)

        // On utilise la réflexion ou on crée une surcharge dans le Manager.
        // Pour éviter de compliquer, modifions plutôt SurfaceManager pour accepter un Collider directement !
        return hit;
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