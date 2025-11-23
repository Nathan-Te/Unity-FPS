using UnityEngine;

public class ProjectileWeapon : WeaponBase
{
    [Header("Spécifique Projectile")]
    public Transform muzzle;
    public GameObject projectilePrefab;
    public float launchForce = 20f;

    [Header("Visée")]
    [Tooltip("Qu'est-ce que le viseur peut toucher ? (Décoche Player et Triggers)")]
    public LayerMask aimingMask = ~0; // Tout par défaut

    protected override void ExecuteFireLogic()
    {
        if (projectilePrefab != null && muzzle != null)
        {
            // ---------------------------------------------------------
            // 1. CALCUL DE LA CIBLE (Convergence Caméra -> Monde)
            // ---------------------------------------------------------

            Camera cam = Camera.main;
            Vector3 targetPoint;

            // On tire un rayon depuis le centre exact de l'écran (0.5, 0.5)
            // C'est là que se trouve ton Crosshair
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            // On ignore le layer du joueur pour ne pas se viser soi-même
            // Astuce : Si aimingMask est mal réglé, le tir partira dans les pieds.
            if (Physics.Raycast(ray, out hit, 1000f, aimingMask))
            {
                targetPoint = hit.point;
            }
            else
            {
                // Si on vise le ciel, on vise un point très loin devant la caméra
                targetPoint = ray.GetPoint(1000f);
            }

            // La direction réelle = Du Muzzle VERS le Point Visé
            Vector3 fireDirection = (targetPoint - muzzle.position).normalized;

            // ---------------------------------------------------------
            // 2. CRÉATION DU PROJECTILE
            // ---------------------------------------------------------

            // On oriente le projectile pour qu'il regarde sa destination
            GameObject proj = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(fireDirection));

            // --- SÉCURITÉ COLLISION (Ton code précédent) ---
            if (_playerController == null) _playerController = FindAnyObjectByType<HeavyFPSController>();

            if (_playerController != null)
            {
                Collider projCollider = proj.GetComponent<Collider>();
                Collider playerCollider = _playerController.GetComponent<Collider>(); // Attention, ici on prend le collider racine

                // Sécurité supplémentaire : On ignore aussi les colliders enfants du joueur (bras, etc.)
                Collider[] allPlayerColliders = _playerController.GetComponentsInChildren<Collider>();
                foreach (var col in allPlayerColliders)
                {
                    if (projCollider != null) Physics.IgnoreCollision(projCollider, col);
                }
            }
            // -----------------------------------------------

            // 3. APPLICATION DE LA VITESSE
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // C'EST ICI LE CHANGEMENT : On utilise fireDirection au lieu de muzzle.forward
                rb.linearVelocity = fireDirection * launchForce;
            }
        }
    }
}