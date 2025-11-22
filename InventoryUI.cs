using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("Références Générales")]
    public GameObject inventoryPanel;
    public PlayerInventory playerInventory;
    public InspectionManager inspectionManager;

    [Header("Grille (Tetris)")]
    public RectTransform gridContainer; // Le conteneur des ITEMS (Items_Layer)
    public Transform backgroundContainer; // Le conteneur des CASES GRISES (Background_Layer)

    [Header("Prefabs")]
    public GameObject itemUiPrefab; // Le prefab avec le script ItemGridUI
    public GameObject slotBackgroundPrefab; // Le prefab du carré gris (avec script InventorySlot)

    [Header("Détails & Actions")]
    public GameObject detailsPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public Button dropButton;

    private bool _isOpen = false;
    private List<ItemGridUI> _spawnedItems = new List<ItemGridUI>();
    private InventoryItem _selectedItem; // L'objet actuellement cliqué

    void Start()
    {
        inventoryPanel.SetActive(false);
        detailsPanel.SetActive(false);

        // On dessine la grille de fond une fois au démarrage
        CreateBackgroundGrid();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        _isOpen = !_isOpen;
        inventoryPanel.SetActive(_isOpen);

        if (_isOpen)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // On rafraichit les objets à chaque ouverture
            RefreshItems();

            // Reset sélection
            _selectedItem = null;
            detailsPanel.SetActive(false);
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // --- GÉNÉRATION VISUELLE ---

    public void CreateBackgroundGrid()
    {
        // Nettoyage ancien fond
        foreach (Transform child in backgroundContainer) Destroy(child.gameObject);

        float size = playerInventory.tileSize;

        // On dimensionne les conteneurs pour qu'ils fassent exactement la taille de la grille
        // Largeur = Colonnes * TailleCase
        // Hauteur = Lignes * TailleCase
        Vector2 gridSize = new Vector2(playerInventory.columns * size, playerInventory.Rows * size);
        gridContainer.sizeDelta = gridSize;
        backgroundContainer.GetComponent<RectTransform>().sizeDelta = gridSize;

        // Création des cases grises
        for (int y = 0; y < playerInventory.Rows; y++)
        {
            for (int x = 0; x < playerInventory.columns; x++)
            {
                GameObject bg = Instantiate(slotBackgroundPrefab, backgroundContainer);
                RectTransform rt = bg.GetComponent<RectTransform>();

                // Taille
                rt.sizeDelta = new Vector2(size, size);

                // Position (Haut-Gauche vers Bas-Droite)
                // X positif, Y négatif
                rt.anchoredPosition = new Vector2(x * size, -y * size);
            }
        }
    }

    void RefreshItems()
    {
        // Nettoyage des items affichés
        foreach (var item in _spawnedItems) Destroy(item.gameObject);
        _spawnedItems.Clear();

        // On recrée l'UI pour chaque objet dans l'inventaire logique
        foreach (var invItem in playerInventory.storedItems)
        {
            GameObject obj = Instantiate(itemUiPrefab, gridContainer);
            ItemGridUI script = obj.GetComponent<ItemGridUI>();

            // Initialisation de l'item UI
            script.Setup(invItem, this);

            _spawnedItems.Add(script);
        }
    }

    // --- LOGIQUE DRAG & DROP ---

    public void OnItemBeginDrag(ItemGridUI itemUI)
    {
        // Quand on commence à drag, on sélectionne l'objet
        SelectItem(itemUI.myItem);
    }

    public void OnItemEndDrag(ItemGridUI itemUI)
    {
        float size = playerInventory.tileSize;
        Vector2 localPos = itemUI.GetComponent<RectTransform>().anchoredPosition;

        // Pour centrer le pivot lors du calcul (optionnel mais aide la précision quand l'objet est tourné)
        // On garde ton calcul actuel pour l'instant qui est simple et efficace
        int targetX = Mathf.RoundToInt(localPos.x / size);
        int targetY = Mathf.RoundToInt(-localPos.y / size);

        // ON PASSE LA TAILLE ACTUELLE (qui a peut-être changé avec R)
        if (playerInventory.CanPlaceItemAt(itemUI.myItem.Width, itemUI.myItem.Height, targetX, targetY, itemUI.myItem))
        {
            itemUI.myItem.x = targetX;
            itemUI.myItem.y = targetY;
            itemUI.UpdatePositionOnGrid();
        }
        else
        {
            // Invalide : On annule TOUT (Position ET Rotation)
            // Si l'utilisateur a tourné l'objet mais ne peut pas le poser, on remet la rotation d'avant ?
            // Ou on laisse la rotation mais on remet à la place ?
            // Simplification : On remet à la place, mais on garde la rotation si elle rentre à la place d'origine.

            // Vérifions si la rotation actuelle rentre à la place d'origine
            if (!playerInventory.CanPlaceItemAt(itemUI.myItem.Width, itemUI.myItem.Height, itemUI.myItem.x, itemUI.myItem.y, itemUI.myItem))
            {
                // Si ça rentre pas à l'origine avec la nouvelle rotation, on annule la rotation
                itemUI.myItem.isRotated = !itemUI.myItem.isRotated; // Undo rotation
                itemUI.Setup(itemUI.myItem, this); // Refresh total
            }

            itemUI.UpdatePositionOnGrid();
        }
    }

    // --- SÉLECTION & ACTIONS ---

    public void SelectItem(InventoryItem item)
    {
        _selectedItem = item;

        detailsPanel.SetActive(true);
        itemNameText.text = item.data.itemName;
        itemDescriptionText.text = item.data.description;

        // Configuration du bouton Drop
        dropButton.onClick.RemoveAllListeners();
        dropButton.onClick.AddListener(OnDropButton);
    }

    void OnDropButton()
    {
        if (_selectedItem != null)
        {
            playerInventory.DropItem(_selectedItem);
            _selectedItem = null;
            detailsPanel.SetActive(false);
            RefreshItems(); // On redessine tout car un objet a disparu
        }
    }

    public void OnExamineButton()
    {
        if (_selectedItem != null)
        {
            inspectionManager.InspectItem(_selectedItem.data);
        }
    }
}