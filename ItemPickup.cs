using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [Header("Données")]
    public ItemData itemData; // On glissera la fiche "Clé Rouge" ici

    public string InteractionPrompt
    {
        get
        {
            return itemData != null ? $"Prendre {itemData.itemName}" : "Prendre Objet";
        }
    }

    public bool Interact(HeavyFPSController player)
    {
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();

        if (inventory != null && itemData != null)
        {
            // On tente d'ajouter l'objet
            bool success = inventory.AddItem(itemData);

            if (success)
            {
                // Si ça a marché, on détruit l'objet au sol
                Destroy(gameObject);
                return true;
            }
            else
            {
                // Si c'est plein, on ne fait rien (ou on affiche un message UI "Inventaire Plein")
                // Le joueur ne peut pas ramasser
                return false;
            }
        }
        return false;
    }
}