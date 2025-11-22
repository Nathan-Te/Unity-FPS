using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    [Header("Configuration Grille")]
    public int columns = 4; // Resident Evil a souvent 4 colonnes fixes
    public int maxSlots = 8; // Capacité totale (définit le nombre de lignes)
    public float tileSize = 100f; // Taille d'une case en pixels UI

    // La liste des objets placés (avec leurs coordonnées)
    public List<InventoryItem> storedItems = new List<InventoryItem>();

    [Header("Drop")]
    public Transform dropPoint;

    // Calcule le nombre de lignes nécessaires (8 slots / 4 cols = 2 lignes)
    public int Rows => Mathf.CeilToInt((float)maxSlots / columns);

    public bool AddItem(ItemData data)
    {
        // 1. SI STACKABLE : Chercher un stack existant non-plein
        if (data.isStackable)
        {
            foreach (var existingItem in storedItems)
            {
                if (existingItem.data == data && existingItem.stackSize < data.maxStackSize)
                {
                    existingItem.stackSize++;
                    return true; // Ajouté au stack existant
                }
            }
        }

        // 2. SINON : Chercher une place vide (Logique Tetris classique)
        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (CanPlaceItemAt(data.width, data.height, x, y))
                {
                    InventoryItem newItem = new InventoryItem(data);
                    newItem.x = x;
                    newItem.y = y;
                    newItem.stackSize = 1; // Commence à 1
                    storedItems.Add(newItem);
                    return true;
                }
            }
        }
        return false; // Inventaire plein
    }

    public int ConsumeItem(ItemData data, int amountNeeded)
    {
        int amountConsumed = 0;

        // On parcourt l'inventaire à l'envers pour pouvoir supprimer sans casser la boucle
        for (int i = storedItems.Count - 1; i >= 0; i--)
        {
            if (amountConsumed >= amountNeeded) break;

            var item = storedItems[i];
            if (item.data == data)
            {
                // Combien on peut prendre dans ce stack ?
                int take = Mathf.Min(amountNeeded - amountConsumed, item.stackSize);

                item.stackSize -= take;
                amountConsumed += take;

                // Si le stack est vide, on supprime l'objet
                if (item.stackSize <= 0)
                {
                    storedItems.RemoveAt(i);
                }
            }
        }
        return amountConsumed; // Retourne combien on a réussi à récupérer
    }

    // Vérifie si un objet rentre à cette position (x,y)
    public bool CanPlaceItemAt(int itemWidth, int itemHeight, int startX, int startY, InventoryItem ignoreItem = null)
    {
        // Vérifier limites négatives
        if (startX < 0 || startY < 0) return false;

        // 1. Limites Grille
        if (startX + itemWidth > columns) return false;
        if (startY + itemHeight > Rows) return false;

        // 2. Chevauchement
        foreach (var item in storedItems)
        {
            if (item == ignoreItem) continue;

            // On utilise les propriétés dynamiques Width/Height de l'item stocké (qui peut être tourné)
            bool overlapX = (startX < item.x + item.Width) && (startX + itemWidth > item.x);
            bool overlapY = (startY < item.y + item.Height) && (startY + itemHeight > item.y);

            if (overlapX && overlapY) return false;
        }

        return true;
    }

    public void RemoveItem(InventoryItem item)
    {
        if (storedItems.Contains(item)) storedItems.Remove(item);
    }

    public void DropItem(InventoryItem item)
    {
        RemoveItem(item);
        if (item.data.prefab != null && dropPoint != null)
        {
            Instantiate(item.data.prefab, dropPoint.position, dropPoint.rotation);
        }
    }
}