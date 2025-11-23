using UnityEngine;

public class ProjectileWeapon : WeaponBase
{
    [Header("Spécifique Projectile")]
    public Transform muzzle;
    public GameObject projectilePrefab;
    public float launchForce = 20f;

    protected override void ExecuteFireLogic()
    {
        if (projectilePrefab != null && muzzle != null)
        {
            // 1. Créer la grenade
            GameObject proj = Instantiate(projectilePrefab, muzzle.position, muzzle.rotation);

            // 2. Récupération Sécurisée du Joueur
            // Si _playerController est vide (le bug actuel), on le cherche manuellement dans toute la scène
            if (_playerController == null)
            {
                _playerController = FindAnyObjectByType<HeavyFPSController>();
            }

            // 3. Ignorer la collision (seulement si on a trouvé le joueur)
            if (_playerController != null)
            {
                Collider projCollider = proj.GetComponent<Collider>();
                Collider playerCollider = _playerController.GetComponent<Collider>();

                if (projCollider != null && playerCollider != null)
                {
                    Physics.IgnoreCollision(projCollider, playerCollider);
                }
            }

            // 4. Donner de la vitesse
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = muzzle.forward * launchForce;
            }
        }
    }
}