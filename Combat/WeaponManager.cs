using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [System.Serializable]
    public class WeaponEntry
    {
        public string id;
        public ItemData linkedItem;
        public WeaponBase weaponScript;
    }

    [Header("Références")]
    public PlayerInventory inventory;
    public PhysicsGrabber physicsGrabber;
    public Transform crosshairUI;

    [Header("Le Registre")]
    public List<WeaponEntry> weaponRegistry;

    [Header("Le Loadout")]
    // On rend le tableau public ou on utilise [SerializeField] pour voir l'état en debug
    [SerializeField] private WeaponEntry[] _loadoutSlots = new WeaponEntry[3];

    private int _currentSlotIndex = -1; // -1 = Mains nues

    void Start()
    {
        // Initialisation : Tout désactiver
        foreach (var w in weaponRegistry)
        {
            if (w.weaponScript) w.weaponScript.gameObject.SetActive(false);
        }

        // Forcer le mode mains nues au démarrage
        EquipSlot(-1);
    }

    void Update()
    {
        // --- CORRECTIF 3 : CONFLIT INPUT ---
        // Si le curseur est visible (Inventaire ouvert, Menu pause...), on interdit le switch d'arme.
        // Cela empêche d'équiper l'arme quand on appuie sur "1" pour l'assigner dans l'UI.
        if (Cursor.lockState != CursorLockMode.Locked) return;
        // -----------------------------------

        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipSlot(2);

        if (Input.GetKeyDown(KeyCode.H)) EquipSlot(-1);
    }

    // --- LOGIQUE D'ÉQUIPEMENT ---

    public void EquipSlot(int slotIndex)
    {
        // Toggle (Rengainer si on appuie sur la touche actuelle)
        if (slotIndex == _currentSlotIndex && slotIndex != -1)
        {
            EquipSlot(-1);
            return;
        }

        // Désactiver l'ancienne arme
        if (_currentSlotIndex != -1 && _loadoutSlots[_currentSlotIndex] != null)
        {
            _loadoutSlots[_currentSlotIndex].weaponScript.gameObject.SetActive(false);
        }

        _currentSlotIndex = slotIndex;

        // Mains nues
        if (slotIndex == -1)
        {
            SetExplorationMode(true);
            return;
        }

        // Nouvelle arme
        WeaponEntry weaponToEquip = _loadoutSlots[slotIndex];

        if (weaponToEquip == null || !InventoryHasItem(weaponToEquip.linkedItem))
        {
            // Slot vide ou item manquant
            SetExplorationMode(true);
            _currentSlotIndex = -1;
            return;
        }

        // Mode Combat
        SetExplorationMode(false);
        weaponToEquip.weaponScript.gameObject.SetActive(true);
    }

    void SetExplorationMode(bool isExploration)
    {
        if (physicsGrabber)
        {
            if (!isExploration) physicsGrabber.DropObject();
            physicsGrabber.enabled = isExploration;
        }
        if (crosshairUI) crosshairUI.gameObject.SetActive(!isExploration);
    }

    // --- LOGIQUE D'ASSIGNATION (CORRECTIF 2 : SLOT UNIQUE) ---

    public bool AssignWeaponToSlot(ItemData item, int targetSlotIndex)
    {
        WeaponEntry foundEntry = weaponRegistry.Find(x => x.linkedItem == item);

        if (foundEntry == null)
        {
            Debug.LogWarning("Cet item n'est pas une arme enregistrée !");
            return false;
        }

        // 1. Nettoyage : Si l'arme était déjà ailleurs, on vide l'ancien slot
        for (int i = 0; i < _loadoutSlots.Length; i++)
        {
            if (_loadoutSlots[i] == foundEntry)
            {
                // Si on déplace l'arme active vers un autre slot, on met à jour l'index actif
                if (_currentSlotIndex == i)
                {
                    _currentSlotIndex = targetSlotIndex;
                }
                _loadoutSlots[i] = null;
            }
        }

        // --- CORRECTIF : GESTION DU REMPLACEMENT SUR LE SLOT ACTIF ---

        // Si on écrase le slot qu'on tient actuellement avec UNE AUTRE arme
        if (_currentSlotIndex == targetSlotIndex)
        {
            // On désactive visuellement l'ancienne arme immédiatement
            if (_loadoutSlots[targetSlotIndex] != null && _loadoutSlots[targetSlotIndex].weaponScript != null)
            {
                _loadoutSlots[targetSlotIndex].weaponScript.gameObject.SetActive(false);
            }

            // On force le mode "Mains nues" temporairement pour éviter les bugs
            // Le joueur devra rappuyer sur la touche pour sortir la nouvelle arme,
            // OU on peut l'équiper automatiquement (choix de design).
            // Ici, je choisis de rengainer pour être propre.
            EquipSlot(-1);
        }
        // -------------------------------------------------------------

        // 2. Assignation
        _loadoutSlots[targetSlotIndex] = foundEntry;

        Debug.Log($"Arme {item.itemName} assignée au slot {targetSlotIndex + 1}");
        return true;
    }

    // --- LOGIQUE DE DROP (CORRECTIF 1 : NETTOYAGE SLOT) ---

    public void OnItemDropped(ItemData item)
    {
        for (int i = 0; i < _loadoutSlots.Length; i++)
        {
            // Si le slot contient l'arme qu'on vient de jeter
            if (_loadoutSlots[i] != null && _loadoutSlots[i].linkedItem == item)
            {
                // Si c'est l'arme active, on rengaine immédiatement
                if (_currentSlotIndex == i)
                {
                    EquipSlot(-1);
                }

                // On vide le slot (Suppression du raccourci)
                _loadoutSlots[i] = null;
                Debug.Log($"Slot {i + 1} vidé car l'objet a été jeté.");
            }
        }
    }

    bool InventoryHasItem(ItemData item)
    {
        if (inventory == null) return false;
        foreach (var i in inventory.storedItems)
        {
            if (i.data == item) return true;
        }
        return false;
    }

    public int GetItemSlotIndex(ItemData item)
    {
        for (int i = 0; i < _loadoutSlots.Length; i++)
        {
            if (_loadoutSlots[i] != null && _loadoutSlots[i].linkedItem == item)
            {
                return i; // Retourne 0, 1 ou 2
            }
        }
        return -1; // Pas équipé
    }
}