using UnityEngine;
using UnityEngine.EventSystems;

public class InspectionManager : MonoBehaviour, IDragHandler
{
    [Header("Scène 3D")]
    public Transform objectSpawnPoint; // Le point devant la caméra d'inspection (Y=-500)
    public Camera inspectionCamera;
    public float rotationSpeed = 5f;

    [Header("UI")]
    public GameObject inspectionPanel; // Le panneau noir qui couvre l'écran

    private GameObject _currentModel;

    void Start()
    {
        inspectionPanel.SetActive(false);
        inspectionCamera.gameObject.SetActive(false); // On coupe la cam quand on ne s'en sert pas (perf)
    }

    public void InspectItem(ItemData item)
    {
        if (item.prefab == null) return;

        // 1. Activer l'interface
        inspectionPanel.SetActive(true);
        inspectionCamera.gameObject.SetActive(true);

        // 2. Nettoyer l'ancien objet
        if (_currentModel != null) Destroy(_currentModel);

        // 3. Spawner le nouvel objet
        _currentModel = Instantiate(item.prefab, objectSpawnPoint.position, Quaternion.identity, objectSpawnPoint);

        // 4. Changer le Layer pour "Inspection" (pour que seule la cam inspection le voie)
        SetLayerRecursively(_currentModel, LayerMask.NameToLayer("Inspection"));

        // 5. Désactiver la physique sur la copie (pour qu'elle ne tombe pas)
        Rigidbody rb = _currentModel.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }

    public void CloseInspection()
    {
        inspectionPanel.SetActive(false);
        inspectionCamera.gameObject.SetActive(false);
        if (_currentModel != null) Destroy(_currentModel);
    }

    // Permet de tourner l'objet en glissant la souris sur l'écran
    public void OnDrag(PointerEventData eventData)
    {
        if (_currentModel != null)
        {
            float rotX = -eventData.delta.x * rotationSpeed * 0.1f;
            float rotY = eventData.delta.y * rotationSpeed * 0.1f;

            // Rotation libre (style RE)
            _currentModel.transform.Rotate(Vector3.up, rotX, Space.World);
            _currentModel.transform.Rotate(Vector3.right, rotY, Space.World);
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}