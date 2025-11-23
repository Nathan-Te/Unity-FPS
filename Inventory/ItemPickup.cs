using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [Header("Données")]
    public ItemData itemData;

    [Min(1)]
    public int quantity = 1; // NOUVEAU : Quantité contenue (ex: 12 balles)

    public string InteractionPrompt
    {
        get
        {
            string qtyString = (itemData != null && itemData.isStackable && quantity > 1) ? $" x{quantity}" : "";
            return itemData != null ? $"Prendre {itemData.itemName}{qtyString}" : "Prendre Objet";
        }
    }

    public bool Interact(HeavyFPSController player)
    {
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();

        if (inventory != null && itemData != null)
        {
            // On passe la quantité à l'inventaire
            bool success = inventory.AddItem(itemData, quantity);

            if (success)
            {
                Destroy(gameObject);
                return true;
            }
        }
        return false;
    }
}