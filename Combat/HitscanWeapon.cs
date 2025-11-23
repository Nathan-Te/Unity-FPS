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

    // ATTENTION : NE PAS REDÉCLARER 'currentAmmo' ICI !
    // Elle est héritée de WeaponBase.

    protected override void ExecuteFireLogic()
    {
        RaycastHit hit;

        // On utilise 'muzzle' (défini ici) et la logique de tir
        if (Physics.Raycast(muzzle.position, muzzle.forward, out hit, range, hitLayers))
        {
            // Debug.DrawLine(muzzle.position, hit.point, Color.red, 1f);

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