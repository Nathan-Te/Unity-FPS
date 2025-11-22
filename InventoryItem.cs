[System.Serializable]
public class InventoryItem
{
    public ItemData data;
    public int x; // Position Colonne (0 à 3)
    public int y; // Position Ligne (0 à 2)
    public bool isRotated; // Pour le futur (si tu veux tourner les objets)

    public InventoryItem(ItemData data)
    {
        this.data = data;
    }

    public int Width => isRotated ? data.height : data.width;
    public int Height => isRotated ? data.width : data.height;
}