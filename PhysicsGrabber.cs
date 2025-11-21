using UnityEngine;

[RequireComponent(typeof(HeavyFPSController))]
public class PhysicsGrabber : MonoBehaviour
{
    [Header("Références")]
    public Transform holdPoint;
    public LayerMask grabLayer;

    [Header("Settings Physique")]
    public float grabForce = 150f;
    public float grabDamper = 10f;
    public float dragForce = 10f;
    public float throwForce = 15f;
    public float reachDistance = 3f;

    [Header("Sécurité")]
    // Si l'objet s'éloigne de plus de X mètres (ex: coincé dans un mur), on le lâche
    public float breakDistance = 4.0f;

    // ... (Variables privées inchangées : _heldRigidbody, _playerController, etc.) ...
    private Rigidbody _heldRigidbody;
    private HeavyFPSController _playerController;
    private float _initialDrag;
    private float _initialAngularDrag;
    public bool IsGrabbing => _heldRigidbody != null;

    void Start()
    {
        _playerController = GetComponent<HeavyFPSController>();
    }

    void Update()
    {
        // Sécurité Menu
        if (Cursor.lockState != CursorLockMode.Locked) return;

        if (_heldRigidbody != null)
        {
            if (Input.GetMouseButtonDown(0)) ThrowObject();
            else if (Input.GetMouseButtonDown(1)) DropObject();
        }
    }

    void FixedUpdate()
    {
        if (_heldRigidbody != null)
        {
            MoveObjectToHoldPoint();
        }
    }

    // ... (Méthodes Grab, DropObject, ThrowObject, ClearObjectPhysics inchangées) ...
    public void Grab(PhysicsGrabbable grabbableObject)
    {
        if (_heldRigidbody != null) DropObject();
        _heldRigidbody = grabbableObject.GetComponent<Rigidbody>();
        _heldRigidbody.useGravity = false;
        _initialDrag = _heldRigidbody.linearDamping;
        _initialAngularDrag = _heldRigidbody.angularDamping;
        _heldRigidbody.linearDamping = dragForce;
        _heldRigidbody.angularDamping = dragForce;
        _playerController.SetCarryingState(true, grabbableObject.speedMultiplier, grabbableObject.allowSprinting);
    }

    public void DropObject()
    {
        if (_heldRigidbody == null) return;
        ClearObjectPhysics();
        _heldRigidbody = null;
        _playerController.ResetCarryingState();
    }

    void ThrowObject()
    {
        if (_heldRigidbody == null) return;
        Rigidbody rb = _heldRigidbody;
        ClearObjectPhysics();
        rb.AddForce(holdPoint.forward * throwForce, ForceMode.Impulse);
        _heldRigidbody = null;
        _playerController.ResetCarryingState();
    }

    void MoveObjectToHoldPoint()
    {
        // Calcul de la distance actuelle
        Vector3 direction = holdPoint.position - _heldRigidbody.position;
        float distance = direction.magnitude;

        // --- NOUVEAU : CHECK DE RUPTURE ---
        if (distance > breakDistance)
        {
            // L'objet est trop loin (coincé), on force le lâcher
            DropObject();
            return; // On arrête là pour ne pas faire de calculs sur un objet null
        }
        // ----------------------------------

        Vector3 targetVelocity = direction.normalized * (distance * grabForce);
        Vector3 force = targetVelocity - _heldRigidbody.linearVelocity * grabDamper;

        _heldRigidbody.AddForce(force);
        _heldRigidbody.angularVelocity *= 0.95f;
    }

    void ClearObjectPhysics()
    {
        if (_heldRigidbody != null)
        {
            _heldRigidbody.useGravity = true;
            _heldRigidbody.linearDamping = _initialDrag;
            _heldRigidbody.angularDamping = _initialAngularDrag;
        }
    }
}