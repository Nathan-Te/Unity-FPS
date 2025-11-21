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

    // Tente d'ajouter un objet automatiquement (cherche la première place libre)
    public bool AddItem(ItemData data)
    {
        // On parcourt la grille case par case pour trouver un trou
        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (CanPlaceItemAt(data, x, y))
                {
                    // Place trouvée !
                    InventoryItem newItem = new InventoryItem(data);
                    newItem.x = x;
                    newItem.y = y;
                    storedItems.Add(newItem);
                    Debug.Log($"Item placé en {x},{y}");
                    return true;
                }
            }
        }
        Debug.Log("Pas de place !");
        return false;
    }

    // Vérifie si un objet rentre à cette position (x,y)
    public bool CanPlaceItemAt(ItemData data, int startX, int startY, InventoryItem ignoreItem = null)
    {
        // --- CORRECTION ICI : Vérifier les limites négatives ---
        if (startX < 0 || startY < 0) return false;
        // -----------------------------------------------------

        // 1. Est-ce que ça sort de la grille (Limites positives) ?
        if (startX + data.width > columns) return false;
        if (startY + data.height > Rows) return false;

        // 2. Est-ce que ça chevauche un autre objet ?
        foreach (var item in storedItems)
        {
            if (item == ignoreItem) continue;

            // Test de collision de rectangles (AABB)
            bool overlapX = (startX < item.x + item.data.width) && (startX + data.width > item.x);
            bool overlapY = (startY < item.y + item.data.height) && (startY + data.height > item.y);

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