using UnityEngine;

public class SimpleInteractable : MonoBehaviour, IInteractable // Note l'héritage de l'interface
{
    [Header("Settings")]
    public string promptMessage = "Utiliser Cube";

    // Implémentation de la propriété de l'interface
    public string InteractionPrompt => promptMessage;

    // Implémentation de la fonction de l'interface
    public bool Interact(HeavyFPSController player)
    {
        Debug.Log("INTERACTION RÉUSSIE !");

        // Exemple d'action : Changer la couleur aléatoirement
        GetComponent<Renderer>().material.color = Random.ColorHSV();

        // On pourrait aussi jouer un son ici
        // AudioSource.PlayClipAtPoint(sound, transform.position);

        return true;
    }
}