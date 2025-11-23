using UnityEngine;
using System.Collections;

public class AutomaticSlidingDoor : MonoBehaviour
{
    [Header("Configuration")]
    public Transform movingPart; // L'objet visuel qui bouge (la porte elle-même)
    public Vector3 slideDirection = new Vector3(1, 0, 0); // Direction locale (X = Droite)
    public float slideDistance = 2.0f; // Distance d'ouverture en mètres
    public float speed = 3.0f;

    [Header("Automatisme")]
    public float closeDelay = 1.0f; // Temps avant fermeture après le départ du joueur

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip motionSound; // Son de "Woooosh" mécanique

    // États internes
    private Vector3 _closedPos;
    private Vector3 _openPos;
    private Vector3 _targetPos;
    private int _objectsInZone = 0; // Compteur pour éviter que la porte se ferme si un ennemi est encore dedans
    private Coroutine _closeTimerRoutine;

    void Start()
    {
        if (movingPart == null) movingPart = transform;

        _closedPos = movingPart.localPosition;
        _openPos = _closedPos + (slideDirection.normalized * slideDistance);
        _targetPos = _closedPos;
    }

    void Update()
    {
        // Mouvement fluide vers la cible
        if (Vector3.Distance(movingPart.localPosition, _targetPos) > 0.001f)
        {
            movingPart.localPosition = Vector3.MoveTowards(
                movingPart.localPosition,
                _targetPos,
                Time.deltaTime * speed
            );

            // Gestion sommaire du son (optionnel : loop tant que ça bouge)
            /* if (audioSource && !audioSource.isPlaying) audioSource.PlayOneShot(motionSound); */
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // On détecte le Joueur OU les objets physiques (pour ne pas coincer une caisse)
        if (IsValidTarget(other))
        {
            _objectsInZone++;
            OpenDoor();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsValidTarget(other))
        {
            _objectsInZone--;

            // Sécurité : On ne descend jamais sous 0
            if (_objectsInZone < 0) _objectsInZone = 0;

            // Si plus personne n'est dans la zone, on lance le timer de fermeture
            if (_objectsInZone == 0)
            {
                if (_closeTimerRoutine != null) StopCoroutine(_closeTimerRoutine);
                _closeTimerRoutine = StartCoroutine(CloseRoutine());
            }
        }
    }

    bool IsValidTarget(Collider other)
    {
        // Accepte le joueur, les ennemis, ou les objets physiques
        return other.GetComponent<HeavyFPSController>() != null
            || other.GetComponent<Rigidbody>() != null;
    }

    void OpenDoor()
    {
        // Si un timer de fermeture était en cours, on l'annule
        if (_closeTimerRoutine != null) StopCoroutine(_closeTimerRoutine);

        _targetPos = _openPos;

        if (audioSource && motionSound)
        {
            // Astuce : On peut changer le Pitch aléatoirement pour varier
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            if (!audioSource.isPlaying) audioSource.PlayOneShot(motionSound);
        }
    }

    IEnumerator CloseRoutine()
    {
        yield return new WaitForSeconds(closeDelay);
        _targetPos = _closedPos;

        if (audioSource && motionSound)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(motionSound);
        }
    }

    // Dessin de debug pour voir où la porte va aller dans l'éditeur
    void OnDrawGizmosSelected()
    {
        if (movingPart != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 endPosGlobal = movingPart.parent.TransformPoint(movingPart.localPosition + (slideDirection.normalized * slideDistance));
            Gizmos.DrawWireCube(endPosGlobal, movingPart.localScale);
            Gizmos.DrawLine(movingPart.position, endPosGlobal);
        }
    }
}