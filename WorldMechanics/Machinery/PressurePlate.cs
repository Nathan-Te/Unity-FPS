using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    [Header("Réglages Détection")]
    [Tooltip("Distance de descente nécessaire pour activer (ex: 0.1m)")]
    public float activationDistance = 0.1f;

    [Tooltip("Marge pour éviter le clignotement Activé/Désactivé à la limite")]
    public float buffer = 0.02f;

    [Tooltip("Le socle de la pressure plate dont on veut ignorer les collisions")]
    public GameObject socle;

    [Header("Événements")]
    public UnityEvent OnPressed;
    public UnityEvent OnReleased;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickDownSound;
    public AudioClip clickUpSound;

    private Vector3 _initialLocalPos;
    private bool _isPressed = false;

    void Start()
    {
        // On mémorise la hauteur "au repos" (ressort détendu)
        _initialLocalPos = transform.localPosition;

        if (socle != null) Physics.IgnoreCollision(socle.GetComponent<Collider>(), GetComponent<Collider>());
    }

    void Update()
    {
        // On calcule de combien la plaque est descendue par rapport à sa position initiale
        // (On suppose que la plaque descend en Y négatif localement)
        float displacement = _initialLocalPos.y - transform.localPosition.y;

        // LOGIQUE D'ACTIVATION (Hystérésis)
        if (!_isPressed)
        {
            // Si on descend plus bas que le seuil
            if (displacement >= activationDistance)
            {
                Press();
            }
        }
        else
        {
            // Si on remonte (Seuil - Buffer pour éviter que ça saute)
            if (displacement < activationDistance - buffer)
            {
                Release();
            }
        }
    }

    void Press()
    {
        _isPressed = true;
        OnPressed.Invoke();
        Debug.Log("Plaque ACTIVÉE");

        if (audioSource && clickDownSound)
        {
            audioSource.pitch = Random.Range(0.9f, 1.0f);
            audioSource.PlayOneShot(clickDownSound);
        }
    }

    void Release()
    {
        _isPressed = false;
        OnReleased.Invoke();
        Debug.Log("Plaque DÉSACTIVÉE");

        if (audioSource && clickUpSound)
        {
            audioSource.pitch = Random.Range(1.0f, 1.1f);
            audioSource.PlayOneShot(clickUpSound);
        }
    }

    // Gizmo pour visualiser le seuil dans l'éditeur
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 activePos = transform.parent.TransformPoint(transform.localPosition - new Vector3(0, activationDistance, 0));
        Gizmos.DrawWireCube(activePos, transform.localScale);
    }
}