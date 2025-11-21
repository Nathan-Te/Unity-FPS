using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemGridUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("UI")]
    public Image iconImage;

    // Données internes
    public InventoryItem myItem;
    private InventoryUI _manager;
    private RectTransform _rect;
    private Canvas _canvas; // Pour gérer l'échelle du drag

    public void Setup(InventoryItem item, InventoryUI manager)
    {
        myItem = item;
        _manager = manager;
        _rect = GetComponent<RectTransform>();
        // On récupère le Canvas racine pour que le drag suive bien la souris
        _canvas = GetComponentInParent<Canvas>();

        iconImage.sprite = item.data.icon;

        // TAILLE : Largeur/Hauteur * TailleCase
        float size = _manager.playerInventory.tileSize;
        _rect.sizeDelta = new Vector2(item.data.width * size, item.data.height * size);

        UpdatePositionOnGrid();
    }

    public void UpdatePositionOnGrid()
    {
        float size = _manager.playerInventory.tileSize;

        // POSITION : X * Taille, -Y * Taille
        // On se cale sur le coin haut-gauche
        _rect.anchoredPosition = new Vector2(myItem.x * size, -myItem.y * size);
    }

    // --- DRAG EVENTS ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        iconImage.raycastTarget = false; // Laisser passer le rayon pour voir dessous
        transform.SetAsLastSibling(); // Mettre au premier plan visuel
        _manager.OnItemBeginDrag(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // On déplace l'objet avec la souris
        // On divise par scaleFactor pour que ce soit précis quelle que soit la résolution
        _rect.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        iconImage.raycastTarget = true; // Réactiver le clic
        _manager.OnItemEndDrag(this);
    }

    // --- CLICK EVENT ---

    public void OnPointerClick(PointerEventData eventData)
    {
        // Simple clic sans drag = Sélectionner pour voir les infos
        _manager.SelectItem(myItem);
    }
}