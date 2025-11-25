using UnityEngine;

public class HitscanWeapon : WeaponBase
{
    [Header("Spécifique Hitscan")]
    public Transform muzzle;
    public float range = 100f;
    public float damage = 10f;
    public float impactForce = 5f;
    public LayerMask hitLayers;

    [Header("Impacts")]
    public GameObject impactPrefab;

    protected override void ExecuteFireLogic()
    {
        RaycastHit hit;

        // Debug
        Debug.DrawRay(muzzle.position, muzzle.forward * range, Color.green, 2f);

        if (Physics.Raycast(muzzle.position, muzzle.forward, out hit, range, hitLayers))
        {
            IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            if (hit.rigidbody != null)
            {
                // --- CORRECTION PHYSIQUE ---
                // 1. Direction : On utilise muzzle.forward (direction de la balle) au lieu de la normale
                // 2. Application : On utilise AddForceAtPosition pour créer de la rotation réaliste
                hit.rigidbody.AddForceAtPosition(muzzle.forward * impactForce, hit.point, ForceMode.Impulse);
                // ---------------------------
            }

            if (impactPrefab)
            {
                GameObject impact = Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                impact.transform.parent = hit.collider.transform;
                Destroy(impact, 5f);
            }
        }
    }
}