using UnityEngine;

public enum ObjectWeight { Light, Heavy }

[RequireComponent(typeof(Rigidbody))]
public class PhysicsGrabbable : MonoBehaviour, IInteractable
{
    [Header("Infos Objet")]
    public string objectName = "Objet";
    public ObjectWeight weightType = ObjectWeight.Light;

    [Header("Impact sur le Joueur")]
    [Range(0.1f, 1f)]
    public float speedMultiplier = 0.9f; // 0.9 = 90% de la vitesse (Léger)

    // Pour un objet lourd, mets ça à false dans l'inspecteur
    public bool allowSprinting = true;

    // Interaction Prompt
    public string InteractionPrompt => $"Saisir {objectName}";

    public bool Interact(HeavyFPSController player)
    {
        PhysicsGrabber grabber = player.GetComponent<PhysicsGrabber>();
        if (grabber != null)
        {
            // On passe "this" (le script entier) pour que le grabber puisse lire les stats
            grabber.Grab(this);
            return true;
        }
        return false;
    }

    void Start()
    {
        // Configuration automatique suggérée selon le type (tu peux override dans l'inspecteur)
        Rigidbody rb = GetComponent<Rigidbody>();

        if (weightType == ObjectWeight.Heavy)
        {
            rb.mass = 6f;
            speedMultiplier = 0.4f; // Très lent
            allowSprinting = false; // Pas de sprint
        }
        else
        {
            rb.mass = 1f;
            speedMultiplier = 0.9f; // Légèrement ralenti
            allowSprinting = true;
        }
    }
}