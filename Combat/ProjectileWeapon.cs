using UnityEngine;

public class ProjectileWeapon : WeaponBase
{
    [Header("Spécifique Projectile")]
    public Transform muzzle; // On le garde juste pour la référence, même si on spawn depuis la cam
    public GameObject projectilePrefab;
    public float launchForce = 20f;

    [Tooltip("Distance devant la caméra pour faire apparaître le projectile (évite le clipping)")]
    public float spawnForwardOffset = 0.5f;

    protected override void ExecuteFireLogic()
    {
        if (projectilePrefab != null)
        {
            // ---------------------------------------------------------
            // 1. POSITONNEMENT SUR LA CAMÉRA (Style Doom/Quake)
            // ---------------------------------------------------------

            Camera cam = Camera.main;

            // Position : Au centre de la caméra + un petit décalage devant pour ne pas l'avoir dans l'oeil
            Vector3 spawnPos = cam.transform.position + (cam.transform.forward * spawnForwardOffset);

            // Rotation : Celle de la caméra (regarde tout droit)
            Quaternion spawnRot = cam.transform.rotation;

            // ---------------------------------------------------------
            // 2. CRÉATION
            // ---------------------------------------------------------

            GameObject proj = Instantiate(projectilePrefab, spawnPos, spawnRot);

            // --- SÉCURITÉ COLLISION ---
            if (_playerController == null) _playerController = FindAnyObjectByType<HeavyFPSController>();

            if (_playerController != null)
            {
                Collider projCollider = proj.GetComponent<Collider>();
                Collider playerCollider = _playerController.GetComponent<Collider>(); // Collider Racine (Capsule)

                // On ignore la capsule principale
                if (projCollider != null && playerCollider != null)
                {
                    Physics.IgnoreCollision(projCollider, playerCollider);
                }

                // On ignore aussi les enfants (Bras, Arme, etc.) pour éviter que la grenade ne tape le fusil
                Collider[] allPlayerColliders = _playerController.GetComponentsInChildren<Collider>();
                foreach (var col in allPlayerColliders)
                {
                    if (projCollider != null) Physics.IgnoreCollision(projCollider, col);
                }
            }
            // --------------------------

            // 3. VITESSE (Tout droit devant la caméra)
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // La direction est simplement le "Forward" de la caméra
                rb.linearVelocity = cam.transform.forward * launchForce;
            }
        }
    }
}