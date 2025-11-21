using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Réglages Porte")]
    [Tooltip("Angle positif = ouvre vers la droite/gauche. Négatif = sens inverse.")]
    public float openAngle = 90f;
    public float openSpeed = 2f;

    [Tooltip("Multiplicateur de vitesse si le joueur sprint (ex: 3x plus vite)")]
    public float sprintMultiplier = 3.0f; // NOUVEAU

    public bool isLocked = false;
    public string lockedMessage = "Verrouillé";

    private bool _isOpen = false;
    private Coroutine _currentCoroutine;
    private Quaternion _closedRotation;
    private Quaternion _openRotation;

    public string InteractionPrompt
    {
        get
        {
            if (isLocked) return lockedMessage;
            return _isOpen ? "Fermer" : "Ouvrir";
        }
    }

    void Start()
    {
        _closedRotation = transform.rotation;
        _openRotation = _closedRotation * Quaternion.Euler(0, openAngle, 0);
    }

    public bool Interact(HeavyFPSController player)
    {
        if (isLocked)
        {
            Debug.Log("La porte est fermée à clé.");
            return false;
        }

        // On change l'état cible
        _isOpen = !_isOpen;

        if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);

        Quaternion destination = _isOpen ? _openRotation : _closedRotation;

        float currentActionSpeed = openSpeed;

        // --- LOGIQUE VITESSE CORRIGÉE (Ouverture Uniquement) ---

        // On accélère SEULEMENT si :
        // 1. Le joueur sprint
        // 2. Le joueur est du côté "Poussée"
        // 3. ET la porte est en train de s'OUVRIR (_isOpen == true)
        if (player.IsSprinting && IsOnPushSide(player.transform.position) && _isOpen)
        {
            currentActionSpeed *= sprintMultiplier;
        }
        // Sinon (Fermeture ou Mauvais côté), vitesse normale
        // ------------------------------------------------------

        _currentCoroutine = StartCoroutine(MoveDoor(destination, currentActionSpeed));

        return true;
    }

    public void Interact()
    {
        // Surcharge pour les interactions sans joueur (ex: Trigger)
        if (isLocked)
        {
            Debug.Log("La porte est fermée à clé.");
            return;
        }
        // On change l'état cible
        _isOpen = !_isOpen;
        if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
        Quaternion destination = _isOpen ? _openRotation : _closedRotation;
        _currentCoroutine = StartCoroutine(MoveDoor(destination, openSpeed));
    }

    // On ajoute le paramètre 'speed' ici
    IEnumerator MoveDoor(Quaternion targetRot, float speed)
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        while (Quaternion.Angle(transform.rotation, targetRot) > 0.5f)
        {
            // On utilise 'speed' au lieu de la variable globale 'openSpeed'
            Quaternion nextRotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * speed);

            if (rb != null)
                rb.MoveRotation(nextRotation);
            else
                transform.rotation = nextRotation;

            yield return new WaitForFixedUpdate();
        }

        if (rb != null) rb.MoveRotation(targetRot);
        else transform.rotation = targetRot;

        _currentCoroutine = null;
    }

    public void Unlock()
    {
        isLocked = false;
    }

    // REMPLACE OnCollisionEnter PAR CELLE-CI :
    void OnTriggerEnter(Collider other)
    {
        // Attention : 'other' est le collider, on cherche le script sur l'objet ou ses parents
        HeavyFPSController player = other.GetComponent<HeavyFPSController>();

        // Si le collider n'a pas le script, on cherche dans le parent (au cas où le joueur ait des colliders enfants)
        if (player == null) player = other.GetComponentInParent<HeavyFPSController>();

        if (player != null)
        {
            // Mêmes conditions : Sprint + Fermée + Pas verrouillée + Immobile
            if (player.IsSprinting && !_isOpen && !isLocked && _currentCoroutine == null)
            {
                if (IsOnPushSide(player.transform.position))
                {
                    Debug.Log("BOOM ! Coup d'épaule sans impact.");

                    // 1. Récupérer le collider DUR de la porte
                    // Comme on est dans OnTriggerEnter, "this" est le pivot, mais le collider est sur l'enfant ou sur soi
                    // On cherche TOUS les colliders de la porte (Trigger et Non-Trigger)
                    Collider[] allDoorColliders = GetComponentsInChildren<Collider>();

                    // Le collider du joueur
                    Collider playerCol = other;

                    // 2. On désactive la collision physique immédiatement
                    // Le joueur traverse le Trigger, on désactive le Mur Dur avant qu'il ne le touche
                    foreach (var doorCol in allDoorColliders)
                    {
                        // On ignore tout sauf le trigger lui-même (qui ne bloque pas de toute façon)
                        if (!doorCol.isTrigger)
                        {
                            StartCoroutine(TemporarilyIgnoreCollision(playerCol, doorCol, 1.0f));
                        }
                    }

                    Interact(player);
                }
            }
        }
    }

    IEnumerator TemporarilyIgnoreCollision(Collider playerCol, Collider doorCol, float delay)
    {
        Physics.IgnoreCollision(playerCol, doorCol, true);
        yield return new WaitForSeconds(delay);
        if (playerCol != null && doorCol != null)
        {
            Physics.IgnoreCollision(playerCol, doorCol, false);
        }
    }

    // Retourne VRAI si le joueur est du côté où on POUSSE la porte
    private bool IsOnPushSide(Vector3 playerPosition)
    {
        // Vecteur allant de la porte vers le joueur
        Vector3 directionToPlayer = playerPosition - transform.position;

        // Produit Scalaire
        float dot = -Vector3.Dot(transform.forward, directionToPlayer);

        // Cas A : Ouverture positive (vers l'extérieur) -> Il faut être Derrière (dot < 0)
        if (openAngle > 0 && dot < 0) return true;

        // Cas B : Ouverture négative (vers l'intérieur) -> Il faut être Devant (dot > 0)
        if (openAngle < 0 && dot > 0) return true;

        return false;
    }
}