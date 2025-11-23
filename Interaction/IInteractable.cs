using UnityEngine;

public interface IInteractable
{
    // Le texte qui s'affichera à l'écran (ex: "Ouvrir Porte", "Ramasser Munitions")
    string InteractionPrompt { get; }

    // La fonction qui se lance quand on appuie sur E
    // On passe le controller du joueur en paramètre au cas où l'objet ait besoin de modifier le joueur (ex: le soigner)
    bool Interact(HeavyFPSController player);
}