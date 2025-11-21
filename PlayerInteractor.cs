using UnityEngine;
using TMPro;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Paramètres")]
    public Transform cameraRoot;
    public float interactionDistance = 2.5f;
    public LayerMask interactionLayer;

    [Header("UI")]
    public TextMeshProUGUI promptText;

    private HeavyFPSController _playerController;
    private PhysicsGrabber _physicsGrabber; // Référence ajoutée
    private IInteractable _currentInteractable;

    void Start()
    {
        _playerController = GetComponent<HeavyFPSController>();
        _physicsGrabber = GetComponent<PhysicsGrabber>(); // On récupère le grabber

        if (cameraRoot == null && _playerController != null)
        {
            cameraRoot = _playerController.visualRoot;
        }
    }

    void Update()
    {
        ScanForInteractables();

        // GESTION UNIFIÉE DE LA TOUCHE E
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Cas 1 : On a une cible interactive en vue -> On interagit
            if (_currentInteractable != null)
            {
                _currentInteractable.Interact(_playerController);
            }
            // Cas 2 : Pas de cible, mais on tient un objet -> On le lâche
            else if (_physicsGrabber != null && _physicsGrabber.IsGrabbing)
            {
                _physicsGrabber.DropObject();
            }
        }
    }

    void ScanForInteractables()
    {
        // --- AJOUT : Si on tient un objet, on coupe le scan ---
        if (_physicsGrabber != null && _physicsGrabber.IsGrabbing)
        {
            // On s'assure que l'UI est cachée
            if (promptText != null) promptText.gameObject.SetActive(false);

            // On vide la référence interactable pour éviter les conflits
            _currentInteractable = null;
            return; // On arrête la fonction ici
        }
        // -------------------------------------------------------

        Vector3 origin = cameraRoot.position;
        Vector3 direction = cameraRoot.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, interactionDistance, interactionLayer))
        {
            // On cherche sur l'objet touché OU dans ses parents (ex: Porte visuelle -> Pivot)
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                _currentInteractable = interactable;

                if (promptText != null)
                {
                    promptText.text = interactable.InteractionPrompt;
                    promptText.gameObject.SetActive(true);
                }
                return;
            }
        }

        _currentInteractable = null;
        if (promptText != null) promptText.gameObject.SetActive(false);
    }
}