using UnityEngine;

public class HitscanWeapon : WeaponBase
{
    [Header("Spécifique Hitscan")]
    public Transform muzzle;
    public float range = 100f; // Vérifie que cette ligne est bien là
    public float damage = 10f;
    public float impactForce = 5f;
    public LayerMask hitLayers;

    [Header("Impacts")]
    public GameObject impactPrefab;

    protected override void ExecuteFireLogic()
    {
        RaycastHit hit;

        // --- DEBUG VISUEL ---
        // Dessine le rayon dans la scène (visible dans l'onglet Scene, pas Game)
        // Vert = Portée max, Rouge = Impact
        Debug.DrawRay(muzzle.position, muzzle.forward * range, Color.green, 2f);
        // --------------------

        if (Physics.Raycast(muzzle.position, muzzle.forward, out hit, range, hitLayers))
        {
            IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce, ForceMode.Impulse);
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