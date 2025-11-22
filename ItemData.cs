using UnityEngine;

// Cela ajoute une option dans le menu clic-droit de Unity
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "Nom de l'objet";
    [TextArea] public string description = "Description de l'objet...";

    [Header("Visuel")]
    public Sprite icon; // L'image dans l'inventaire (2D)
    public GameObject prefab; // L'objet physique à faire apparaître quand on le jette (3D)

    [Header("Dimensions (Tetris)")]
    [Min(1)] public int width = 1;  // Largeur (Colonnes)
    [Min(1)] public int height = 1; // Hauteur (Lignes)

    [Header("Stacking")]
    public bool isStackable = false;
    [Min(1)] public int maxStackSize = 1; // Ex: 60 pour des balles
}