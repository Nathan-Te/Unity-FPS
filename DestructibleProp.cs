using UnityEngine;

public class DestructibleProp : MonoBehaviour, IDamageable
{
    public float health = 50f;
    public GameObject destroyEffect; // Assignes-y une explosion ou des particules

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} a pris {damage} dégâts. Reste : {health}");

        // Petit impact physique pour le fun
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.AddForce(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        }

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (destroyEffect != null) Instantiate(destroyEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}