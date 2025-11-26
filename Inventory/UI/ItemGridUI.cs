using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemGridUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("UI")]
    public Image iconImage;
    public TMPro.TextMeshProUGUI quantityText;
    public TextMeshProUGUI shortcutText;

    // Données internes
    public InventoryItem myItem;
    private InventoryUI _manager;
    private RectTransform _rect;
    private Canvas _canvas; // Pour gérer l'échelle du drag

    private bool _isDragging = false;

    public void Setup(InventoryItem item, InventoryUI manager)
    {
        myItem = item;
        _manager = manager;
        _rect = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();

        iconImage.sprite = item.data.icon;

        // GESTION QUANTITÉ
        if (item.data.isStackable)
        {
            quantityText.gameObject.SetActive(true);
            quantityText.text = item.stackSize.ToString();
        }
        else
        {
            // On cache le texte si c'est 1 ou non stackable
            if (quantityText) quantityText.gameObject.SetActive(false);
        }

        if (shortcutText) shortcutText.gameObject.SetActive(false);

        RefreshVisualSize(); // On sort la logique de taille dans une fonction
        UpdatePositionOnGrid();
    }

    public void SetShortcutDisplay(int slotIndex)
    {
        if (shortcutText == null) return;

        if (slotIndex >= 0)
        {
            shortcutText.gameObject.SetActive(true);
            shortcutText.text = "[" + (slotIndex + 1).ToString() + "]"; // Affiche 1 au lieu de 0
        }
        else
        {
            shortcutText.gameObject.SetActive(false);
        }
    }

    public void UpdatePositionOnGrid()
    {
        float size = _manager.playerInventory.tileSize;

        // POSITION : X * Taille, -Y * Taille
        // On se cale sur le coin haut-gauche
        _rect.anchoredPosition = new Vector2(myItem.x * size, -myItem.y * size);
    }

    // --- DRAG EVENTS ---
    void RefreshVisualSize()
    {
        float size = _manager.playerInventory.tileSize;

        // 1. Dimensionner le CONTENEUR (La Hitbox / Cadre invisible)
        Vector2 containerSize = new Vector2(myItem.Width * size, myItem.Height * size);
        _rect.sizeDelta = containerSize;

        // 2. Dimensionner et Tourner l'ICÔNE (Le Visuel)
        RectTransform iconRect = iconImage.rectTransform;

        // --- CORRECTIF MARGE (90% ou 95%) ---
        // On réduit la taille de l'image pour qu'elle ne touche pas les bords
        float marginScale = 0.90f; // Essaie 0.9f, c'est souvent plus joli que 0.95f
        // ------------------------------------

        if (myItem.isRotated)
        {
            iconRect.localRotation = Quaternion.Euler(0, 0, -90);
            // Inversion des dimensions pour la rotation + Application de la marge
            iconRect.sizeDelta = new Vector2(containerSize.y * marginScale, containerSize.x * marginScale);
        }
        else
        {
            iconRect.localRotation = Quaternion.identity;
            // Taille standard + Application de la marge
            iconRect.sizeDelta = containerSize * marginScale;
        }
    }

    void Update()
    {
        // Si on drag et qu'on appuie sur R
        if (_isDragging && Input.GetKeyDown(KeyCode.R))
        {
            Rotate();
        }
    }

    void Rotate()
    {
        // 1. On sauvegarde l'état AVANT rotation
        float size = _manager.playerInventory.tileSize;
        float oldWidth = myItem.Width * size;
        float oldHeight = myItem.Height * size;

        // 2. On applique la rotation logique
        myItem.isRotated = !myItem.isRotated;

        // 3. On récupère les dimensions APRÈS rotation
        float newWidth = myItem.Width * size;
        float newHeight = myItem.Height * size;

        // 4. CORRECTION DE POSITION (Le pivot magique)
        // Le pivot est en Haut-Gauche (0,1).
        // Le centre visuel se trouve à (+Largeur/2, -Hauteur/2) par rapport au pivot.

        Vector2 oldCenterOffset = new Vector2(oldWidth / 2, -oldHeight / 2);
        Vector2 newCenterOffset = new Vector2(newWidth / 2, -newHeight / 2);

        // On calcule la différence pour que les centres s'alignent
        Vector2 adjustment = oldCenterOffset - newCenterOffset;

        // On applique le décalage à la position de l'objet
        _rect.anchoredPosition += adjustment;

        // 5. Mise à jour visuelle finale
        RefreshVisualSize();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true; // ACTIVE
        iconImage.raycastTarget = false;
        transform.SetAsLastSibling();
        _manager.OnItemBeginDrag(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rect.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false; // DESACTIVE
        iconImage.raycastTarget = true;
        _manager.OnItemEndDrag(this);
    }

    // --- CLICK EVENT ---

    public void OnPointerClick(PointerEventData eventData)
    {
        // Simple clic sans drag = Sélectionner pour voir les infos
        _manager.SelectItem(myItem);
    }
}