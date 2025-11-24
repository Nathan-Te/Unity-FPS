using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DestructibleLock : MonoBehaviour, IDamageable
{
    [Header("Cible")]
    public Door doorToUnlock; // La porte qu'on verrouille

    [Header("Santé")]
    public float health = 20f; // Assez faible pour casser en 1 ou 2 balles

    [Header("FX Destruction")]
    public GameObject brokenLockPrefab; // Optionnel : Des morceaux de métal
    public AudioSource audioSource;
    public AudioClip breakSound;

    private bool _isBroken = false;

    void Start()
    {
        // Au démarrage, on s'assure que la porte est bien verrouillée
        if (doorToUnlock != null)
        {
            // On force le verrouillage logique
            // (Assure-toi que ta variable isLocked est publique ou accessible, sinon on modifiera Door.cs)
            doorToUnlock.isLocked = true;
        }
    }

    // Interface IDamageable
    public void TakeDamage(float damage)
    {
        Debug.Log("Cadenas subit des dégâts ! ");

        if (_isBroken) return;

        health -= damage;

        // Petit impact physique quand on tire dessus
        GetComponent<Rigidbody>().AddForce(Random.insideUnitSphere * 5f, ForceMode.Impulse);

        if (health <= 0)
        {
            BreakLock();
        }
    }

    void BreakLock()
    {
        _isBroken = true;

        // 1. Déverrouiller la porte
        if (doorToUnlock != null)
        {
            doorToUnlock.Unlock();
        }

        // 2. Son
        if (audioSource && breakSound)
        {
            AudioSource.PlayClipAtPoint(breakSound, transform.position);
        }

        // 3. Effet Visuel (Remplacer le cadenas par des débris ou juste le faire tomber)
        if (brokenLockPrefab != null)
        {
            Instantiate(brokenLockPrefab, transform.position, transform.rotation);
            Destroy(gameObject); // On supprime le cadenas intact
        }
        else
        {
            // Alternative simple : Le cadenas "saute" et devient un objet physique inerte
            // On le détache de la porte (s'il était enfant)
            transform.parent = null;

            // On active la gravité et on le pousse
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce((transform.forward + Vector3.up) * 2f, ForceMode.Impulse);

            // On désactive ce script pour qu'il ne soit plus interactif/damageable
            Destroy(this);
        }
    }
}