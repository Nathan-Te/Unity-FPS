[System.Serializable]
public class InventoryItem
{
    public ItemData data;
    public int x;
    public int y;
    public bool isRotated;

    // NOUVEAU : Quantité
    public int stackSize = 1;

    public InventoryItem(ItemData data)
    {
        this.data = data;
        this.stackSize = 1; // Par défaut 1
    }

    // ... propriétés Width/Height inchangées
    public int Width => isRotated ? data.height : data.width;
    public int Height => isRotated ? data.width : data.height;
}