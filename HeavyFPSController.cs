using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class HeavyFPSController : MonoBehaviour
{
    [Header("Architecture Découplée")]
    public Transform visualRoot;

    [Header("État & Sol")]
    public LayerMask groundMask;
    public float groundCheckRadius = 0.3f;
    public float gravityMultiplier = 2f;

    [Header("Mouvement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2f;
    public float accelerationTime = 0.15f;
    public float stopTime = 0.1f;

    // Propriété publique pour les autres scripts (Porte, etc.)
    public bool IsSprinting { get; private set; }

    [Header("Stamina & Fatigue")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 15f;
    public float staminaRegenRate = 10f;
    public float staminaRegenDelay = 1.5f;
    public float CurrentStamina { get; private set; }

    [Header("Impact Atterrissage")]
    public float minFallSpeedForImpact = 4f;
    public ProceduralRecoil recoilScript;

    [Header("Vaulting")]
    public float vaultSpeed = 1.5f;
    public float vaultMaxHeight = 1.5f;
    public float vaultCheckDistance = 1.0f;
    public float vaultMaxDepth = 1.2f;
    public LayerMask vaultLayer;
    public float vaultLandingOffset = 0.6f;

    [Header("Accroupissement")]
    public float crouchHeight = 1.0f;
    public float crouchTransitionSpeed = 10f;
    public float standEyeHeight = 1.6f;
    public float crouchEyeHeight = 0.8f;

    [Header("Vue Souris")]
    public float mouseSensitivity = 2f;
    public float topClamp = -85f;
    public float bottomClamp = 85f;

    [Header("Head Bob")]
    public bool useHeadBob = true;
    public float bobFrequency = 1.5f;
    public float bobAmplitude = 0.05f;

    [Header("Free Aim")]
    public bool useFreeAim = true;
    public float freeAimMaxAngle = 15f;
    public float recenterSpeed = 2f;
    public float CurrentFreeAimX { get; private set; }
    public float CurrentFreeAimY { get; private set; }

    // --- INTERNES ---
    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Vector3 smoothVelocityRef;

    private float _rotationX;
    private float _rotationY;

    private bool _isGrounded;
    private bool _wasGrounded;
    private float _previousYVelocity;

    private bool _isCrouching;
    private bool _isVaulting;
    private float _originalHeight;
    private float _currentEyeHeight;
    private float _headTopMargin;

    // Stamina
    private float _lastSprintTime;
    private bool _isExhausted;

    // HeadBob
    private float _bobTimer;
    private Vector3 _bobPositionOffset;

    // Carrying
    private bool _isCarryingObject;
    private float _currentCarrySpeedMultiplier = 1.0f;
    private bool _canSprintWhileCarrying = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        if (recoilScript == null) recoilScript = GetComponentInChildren<ProceduralRecoil>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        _originalHeight = capsule.height;
        _currentEyeHeight = standEyeHeight;
        _headTopMargin = _originalHeight - standEyeHeight;

        CurrentStamina = maxStamina;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (visualRoot)
        {
            visualRoot.parent = null;
            _rotationX = visualRoot.eulerAngles.x;
            _rotationY = visualRoot.eulerAngles.y;
        }
    }

    void Update()
    {
        if (_isVaulting) return;

        // C'est cette fonction qui manquait dans ta version précédente !
        HandleRotationWithFreeAim();

        HandleStamina();

        if (Input.GetKeyDown(KeyCode.Space) && !_isCrouching && !_isCarryingObject) VaultCheck();
        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (!_isCrouching) _isCrouching = true;
            else if (CanStandUp()) _isCrouching = false;
        }
    }

    void LateUpdate()
    {
        if (visualRoot == null) return;

        if (!_isVaulting)
        {
            float targetEyeHeight = _isCrouching ? crouchEyeHeight : standEyeHeight;
            _currentEyeHeight = Mathf.Lerp(_currentEyeHeight, targetEyeHeight, Time.deltaTime * crouchTransitionSpeed);
            CalculateHeadBob();
        }
        else
        {
            _bobPositionOffset = Vector3.Lerp(_bobPositionOffset, Vector3.zero, Time.deltaTime * 5f);
        }

        float feetY = capsule.bounds.min.y;
        Vector3 finalEyePos = new Vector3(transform.position.x, feetY + _currentEyeHeight, transform.position.z);

        visualRoot.position = finalEyePos + _bobPositionOffset;
        visualRoot.rotation = Quaternion.Euler(_rotationX, _rotationY, 0f);
    }

    void FixedUpdate()
    {
        _previousYVelocity = rb.linearVelocity.y;

        if (_isVaulting) return;

        CheckGround();
        HandleLandingImpact();
        HandleCrouchPhysics();
        HandleBodyRotation();
        HandleMovement();
        ApplyExtraGravity();
    }

    // --- LOGIQUE ROTATION (LA PARTIE QUI ETAIT VIDE AVANT) ---
    void HandleRotationWithFreeAim()
    {
        // --- AJOUT : SI LE CURSEUR EST LIBRE (MENU), ON NE TOURNE PAS ---
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        bool isFreeAimActive = useFreeAim && !IsSprinting;

        if (isFreeAimActive)
        {
            CurrentFreeAimX += mouseX;
            CurrentFreeAimY -= mouseY;

            if (CurrentFreeAimX > freeAimMaxAngle)
            {
                float overflow = CurrentFreeAimX - freeAimMaxAngle;
                _rotationY += overflow;
                CurrentFreeAimX = freeAimMaxAngle;
            }
            else if (CurrentFreeAimX < -freeAimMaxAngle)
            {
                float overflow = CurrentFreeAimX - (-freeAimMaxAngle);
                _rotationY += overflow;
                CurrentFreeAimX = -freeAimMaxAngle;
            }

            if (CurrentFreeAimY > freeAimMaxAngle)
            {
                float overflow = CurrentFreeAimY - freeAimMaxAngle;
                _rotationX += overflow;
                CurrentFreeAimY = freeAimMaxAngle;
            }
            else if (CurrentFreeAimY < -freeAimMaxAngle)
            {
                float overflow = CurrentFreeAimY - (-freeAimMaxAngle);
                _rotationX += overflow;
                CurrentFreeAimY = -freeAimMaxAngle;
            }
        }
        else
        {
            _rotationY += mouseX;
            _rotationX -= mouseY;

            float sprintReturnSpeed = recenterSpeed * 5f;
            CurrentFreeAimX = Mathf.Lerp(CurrentFreeAimX, 0, Time.deltaTime * sprintReturnSpeed);
            CurrentFreeAimY = Mathf.Lerp(CurrentFreeAimY, 0, Time.deltaTime * sprintReturnSpeed);
        }

        _rotationX = Mathf.Clamp(_rotationX, topClamp, bottomClamp);

        if (isFreeAimActive && (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0))
        {
            CurrentFreeAimX = Mathf.Lerp(CurrentFreeAimX, 0, Time.deltaTime * recenterSpeed);
            CurrentFreeAimY = Mathf.Lerp(CurrentFreeAimY, 0, Time.deltaTime * recenterSpeed);
        }
    }

    void HandleBodyRotation()
    {
        Quaternion targetBodyRotation = Quaternion.Euler(0f, _rotationY, 0f);
        rb.MoveRotation(targetBodyRotation);
    }

    // --- LOGIQUE STAMINA & IMPACT ---
    void HandleLandingImpact()
    {
        if (_isGrounded && !_wasGrounded)
        {
            if (_previousYVelocity < -minFallSpeedForImpact)
            {
                if (recoilScript != null)
                {
                    recoilScript.LandingImpact(_previousYVelocity);
                }
            }
        }
        _wasGrounded = _isGrounded;
    }

    void HandleStamina()
    {
        if (IsSprinting)
        {
            CurrentStamina -= staminaDrainRate * Time.deltaTime;
            _lastSprintTime = Time.time;
            if (CurrentStamina <= 0)
            {
                CurrentStamina = 0;
                _isExhausted = true;
            }
        }
        else
        {
            if (Time.time > _lastSprintTime + staminaRegenDelay)
            {
                CurrentStamina += staminaRegenRate * Time.deltaTime;
            }
            if (CurrentStamina > maxStamina * 0.2f)
            {
                _isExhausted = false;
            }
        }
        CurrentStamina = Mathf.Clamp(CurrentStamina, 0, maxStamina);
    }

    // --- MOUVEMENT ---
    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        bool isTryingToRun = Input.GetKey(KeyCode.LeftShift);
        bool isMovingForward = moveZ > 0;

        bool hasStamina = CurrentStamina > 0 && !_isExhausted;
        IsSprinting = isTryingToRun && isMovingForward && !_isCrouching && _canSprintWhileCarrying && hasStamina;

        float baseSpeed = walkSpeed;
        if (_isCrouching) baseSpeed = crouchSpeed;
        else if (IsSprinting) baseSpeed = runSpeed;

        float finalSpeed = baseSpeed * _currentCarrySpeedMultiplier;

        Vector3 inputDir = new Vector3(moveX, 0, moveZ).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            Vector3 forward = visualRoot.forward; forward.y = 0; forward.Normalize();
            Vector3 right = visualRoot.right; right.y = 0; right.Normalize();

            Vector3 targetMove = right * inputDir.x + forward * inputDir.z;
            targetMove *= finalSpeed;

            float currentSmoothTime = (_isCrouching) ? stopTime : accelerationTime;
            if (IsSprinting) currentSmoothTime *= 1.2f;
            if (_isCarryingObject && _currentCarrySpeedMultiplier < 0.8f) currentSmoothTime *= 1.5f;
            if (!_isGrounded) currentSmoothTime /= 0.1f;

            Vector3 targetVelocity = new Vector3(targetMove.x, rb.linearVelocity.y, targetMove.z);
            Vector3 smoothedVel = Vector3.SmoothDamp(rb.linearVelocity, targetVelocity, ref smoothVelocityRef, currentSmoothTime);

            smoothedVel.y = rb.linearVelocity.y;
            rb.linearVelocity = smoothedVel;
        }
        else
        {
            float friction = _isGrounded ? stopTime : 2f;
            Vector3 targetVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            Vector3 smoothedVel = Vector3.SmoothDamp(rb.linearVelocity, targetVelocity, ref smoothVelocityRef, friction);
            rb.linearVelocity = smoothedVel;
        }
    }

    // --- PHYSIQUE BASE ---
    void CheckGround()
    {
        Vector3 feetPosition = new Vector3(capsule.bounds.center.x, capsule.bounds.min.y, capsule.bounds.center.z);
        Vector3 spherePos = feetPosition + Vector3.up * (groundCheckRadius - 0.05f);
        _isGrounded = Physics.CheckSphere(spherePos, groundCheckRadius, groundMask);
    }

    void HandleCrouchPhysics()
    {
        float targetColliderHeight = _isCrouching ? (crouchEyeHeight + _headTopMargin) : _originalHeight;
        if (Mathf.Abs(capsule.height - targetColliderHeight) > 0.001f)
        {
            capsule.height = Mathf.Lerp(capsule.height, targetColliderHeight, Time.deltaTime * crouchTransitionSpeed);
            capsule.center = new Vector3(0, capsule.height * 0.5f, 0);
        }
    }

    bool CanStandUp()
    {
        if (!_isCrouching) return true;
        float radius = capsule.radius * 0.9f;
        float currentCeiling = transform.position.y + capsule.height;
        Vector3 start = new Vector3(transform.position.x, currentCeiling - radius, transform.position.z);
        float distanceToStand = _originalHeight - capsule.height;
        return !Physics.SphereCast(start, radius, Vector3.up, out RaycastHit hit, distanceToStand + 0.1f, groundMask);
    }

    void ApplyExtraGravity()
    {
        if (!_isGrounded && rb.linearVelocity.y < 0)
        {
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1), ForceMode.Acceleration);
        }
    }

    // --- VAULTING ---
    void VaultCheck()
    {
        Vector3 forward = visualRoot.forward; forward.y = 0; forward.Normalize();
        Vector3 feetPos = new Vector3(capsule.bounds.center.x, capsule.bounds.min.y, capsule.bounds.center.z);
        Vector3 kneePos = feetPos + Vector3.up * 0.4f;
        Vector3 headPos = feetPos + Vector3.up * (capsule.height - 0.1f);

        if (Physics.Raycast(kneePos, forward, out RaycastHit frontHit, vaultCheckDistance, vaultLayer))
        {
            if (!Physics.Raycast(headPos, forward, vaultCheckDistance + 0.5f, vaultLayer))
            {
                Vector3 topCheckStart = frontHit.point + (forward * 0.1f) + (Vector3.up * vaultMaxHeight);
                if (Physics.Raycast(topCheckStart, Vector3.down, out RaycastHit topHit, vaultMaxHeight * 2, vaultLayer))
                {
                    Vector3 landPoint = Vector3.zero;
                    bool foundLanding = false;
                    LayerMask combinedMask = vaultLayer | groundMask;

                    for (float dist = 0.2f; dist <= vaultMaxDepth; dist += 0.2f)
                    {
                        Vector3 scanPos = frontHit.point + (forward * dist) + (Vector3.up * vaultMaxHeight);
                        if (Physics.Raycast(scanPos, Vector3.down, out RaycastHit scanHit, vaultMaxHeight * 2, combinedMask))
                        {
                            if (((1 << scanHit.collider.gameObject.layer) & vaultLayer) != 0) continue;
                            if (((1 << scanHit.collider.gameObject.layer) & groundMask) != 0)
                            {
                                if (scanHit.point.y < topHit.point.y - 0.1f)
                                {
                                    landPoint = scanHit.point + (forward * vaultLandingOffset);
                                    foundLanding = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (foundLanding) StartCoroutine(VaultRoutine(frontHit.point, topHit.point, landPoint));
                }
            }
        }
    }

    IEnumerator VaultRoutine(Vector3 approachPoint, Vector3 topPoint, Vector3 landPoint)
    {
        _isVaulting = true;
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;

        Vector3 startPos = transform.position;
        float offsetFromFeetToPivot = transform.position.y - capsule.bounds.min.y;
        Vector3 peakPos = new Vector3(approachPoint.x, topPoint.y + offsetFromFeetToPivot + 0.1f, approachPoint.z);
        Vector3 endPos = new Vector3(landPoint.x, landPoint.y + offsetFromFeetToPivot, landPoint.z);

        float totalDistance = Vector3.Distance(startPos, peakPos) + Vector3.Distance(peakPos, endPos);
        float dynamicSpeed = vaultSpeed * (2.0f / Mathf.Max(totalDistance, 1.0f));

        float oldEyeHeight = _currentEyeHeight;
        float progress = 0;

        while (progress < 1)
        {
            progress += Time.deltaTime * dynamicSpeed;
            transform.position = Vector3.Lerp(startPos, peakPos, progress);
            _currentEyeHeight = Mathf.Lerp(oldEyeHeight, crouchEyeHeight, progress);
            yield return null;
        }

        progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime * dynamicSpeed;
            transform.position = Vector3.Lerp(peakPos, endPos, progress);
            _currentEyeHeight = Mathf.Lerp(crouchEyeHeight, oldEyeHeight, progress);
            yield return null;
        }

        rb.isKinematic = false;
        _isVaulting = false;
        _currentEyeHeight = oldEyeHeight;
    }

    // --- HELPERS ---
    void CalculateHeadBob()
    {
        if (!useHeadBob || !_isGrounded)
        {
            _bobPositionOffset = Vector3.Lerp(_bobPositionOffset, Vector3.zero, Time.deltaTime * 5f);
            return;
        }

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float speed = horizontalVelocity.magnitude;

        if (speed < 0.1f)
        {
            _bobTimer = 0;
            _bobPositionOffset = Vector3.Lerp(_bobPositionOffset, Vector3.zero, Time.deltaTime * 5f);
        }
        else
        {
            float actualFrequency = bobFrequency;
            if (_isCrouching) actualFrequency *= 0.7f;
            if (speed > walkSpeed + 1) actualFrequency *= 1.3f;

            _bobTimer += Time.deltaTime * (speed * actualFrequency);

            float amp = bobAmplitude;
            if (_isCrouching) amp *= 0.5f;

            float yPos = Mathf.Sin(_bobTimer) * amp;
            float xPos = Mathf.Cos(_bobTimer / 2) * (amp / 2);
            _bobPositionOffset = new Vector3(xPos, yPos, 0);
        }
    }

    public void SetCarryingState(bool isCarrying, float speedMult, bool allowSprint)
    {
        _isCarryingObject = isCarrying;
        _currentCarrySpeedMultiplier = speedMult;
        _canSprintWhileCarrying = allowSprint;
    }

    public void ResetCarryingState()
    {
        _isCarryingObject = false;
        _currentCarrySpeedMultiplier = 1.0f;
        _canSprintWhileCarrying = true;
    }

    void OnDrawGizmosSelected()
    {
        if (capsule == null || visualRoot == null) return;

        Vector3 forward = visualRoot.forward; forward.y = 0; forward.Normalize();
        Vector3 feetPos = new Vector3(capsule.bounds.center.x, capsule.bounds.min.y, capsule.bounds.center.z);
        Vector3 kneePos = feetPos + Vector3.up * 0.4f;

        bool hitFront = Physics.Raycast(kneePos, forward, out RaycastHit frontHit, vaultCheckDistance, vaultLayer);
        Gizmos.color = hitFront ? Color.green : Color.red;
        Gizmos.DrawLine(kneePos, kneePos + forward * vaultCheckDistance);

        // (Je ne remets pas tout le gizmo de vaulting complexe ici pour alléger, 
        // mais ton code précédent pour OnDrawGizmosSelected était bon !)
    }
}