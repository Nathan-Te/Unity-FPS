using UnityEngine;
using System.Collections;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Configuration Générale")]
    public string weaponName = "Arme";
    public float fireRate = 0.1f;
    public bool isAutomatic = false;

    [Header("Munitions")]
    public bool infiniteAmmo = false;
    public int currentAmmo = 12;
    public int maxAmmo = 12;
    public ItemData ammoItemData;

    [Header("Animation")]
    public Animator weaponAnimator; // <-- Référence à l'Animator du modèle 3D
    public float reloadTime = 1.5f; // Durée du rechargement (doit matcher l'anim)

    [Header("Recul & Sensations")]
    public ProceduralRecoil recoilScript;
    public DynamicCrosshair crosshairScript;
    public Vector3 recoilPunch = new Vector3(2f, 0.5f, 0.1f);
    public float cameraRecoilVertical = 2.0f;
    public float cameraRecoilHorizontal = 0.5f;
    public float crosshairSpread = 30f;

    [Header("Audio & VFX")]
    public ParticleSystem muzzleFlash;
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;

    [Header("Collision Mur")]
    public WeaponCollision weaponCollision;

    [Tooltip("Si VRAI, le réticule s'affiche quand cette arme est équipée.")]
    public bool showCrosshair = true;

    [Header("Visée (ADS)")]
    public bool canAim = true;
    public float aimFOV = 40f; // FOV quand on vise (Default est souvent 60 ou 75)
    [Range(0.1f, 1f)]
    public float aimSensitivityRatio = 0.5f;

    protected bool _isAiming = false;

    protected float _nextFireTime;
    protected bool _isReloading = false;
    protected PlayerInventory _inventory;
    protected HeavyFPSController _playerController;

    // Hachage des paramètres Animator pour la performance
    protected int _animIDSpeed;
    protected int _animIDIsSprinting;
    protected int _animIDFire;
    protected int _animIDReload;
    protected int _animIDReloadSpeed;

    protected virtual void Start()
    {
        _inventory = FindAnyObjectByType<PlayerInventory>();
        _playerController = GetComponentInParent<HeavyFPSController>();

        if (_playerController == null)
        {
            _playerController = FindAnyObjectByType<HeavyFPSController>();
        }

        // Si l'animator n'est pas assigné manuellement, on cherche dans les enfants
        if (weaponAnimator == null) weaponAnimator = GetComponentInChildren<Animator>();

        SetupAnimatorIds();
    }

    protected virtual void OnDisable()
    {
        if (_isAiming)
        {
            _isAiming = false;
            ResetAimEffects();
        }
    }

    void SetupAnimatorIds()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDIsSprinting = Animator.StringToHash("IsSprinting");
        _animIDFire = Animator.StringToHash("Fire");
        _animIDReload = Animator.StringToHash("Reload");
        _animIDReloadSpeed = Animator.StringToHash("ReloadSpeed");
    }

    protected virtual void Update()
    {
        UpdateAnimationState();

        if (Cursor.lockState != CursorLockMode.Locked) return;
        if (_isReloading) return;

        HandleShooting();

        // --- AJOUT ---
        HandleAiming();
        // -------------

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    void HandleAiming()
    {
        if (!canAim) return;

        // Entrée en visée
        if (Input.GetMouseButtonDown(1))
        {
            _isAiming = true;
            ApplyAimEffects(true);
        }

        // Sortie de visée
        if (Input.GetMouseButtonUp(1))
        {
            _isAiming = false;
            ApplyAimEffects(false);
        }
    }

    void ApplyAimEffects(bool state)
    {
        // 1. Contrôleur (FOV + Sensibilité)
        if (_playerController)
        {
            _playerController.SetAimState(state, aimFOV, aimSensitivityRatio);
        }

        // 2. Crosshair (Rétrécissement)
        if (crosshairScript)
        {
            crosshairScript.SetAiming(state);
        }

        // 3. Animator (Optionnel pour plus tard : position des bras)
        if (weaponAnimator)
        {
            weaponAnimator.SetBool("IsAiming", state); // Pense à ajouter ce paramètre bool dans ton Animator si tu veux l'utiliser
        }
    }

    void ResetAimEffects()
    {
        if (_playerController) _playerController.SetAimState(false, 0, 1); // Reset
        if (crosshairScript) crosshairScript.SetAiming(false);
    }

    // Synchronisation des variables de mouvement
    void UpdateAnimationState()
    {
        if (weaponAnimator == null || _playerController == null) return;

        // Vitesse (pour le Blend Tree Idle/Walk/Run)
        float currentSpeed = _playerController.GetCurrentSpeed();
        weaponAnimator.SetFloat(_animIDSpeed, currentSpeed, 0.1f, Time.deltaTime);

        // État de sprint
        weaponAnimator.SetBool(_animIDIsSprinting, _playerController.IsSprinting);
    }

    void HandleShooting()
    {
        if (weaponCollision != null && weaponCollision.IsBlocked) return;

        bool intentToFire = isAutomatic ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);

        if (intentToFire && Time.time >= _nextFireTime)
        {
            if (currentAmmo > 0 || infiniteAmmo)
            {
                Fire();
                _nextFireTime = Time.time + fireRate;
            }
            else
            {
                _nextFireTime = Time.time + 0.2f;
                if (audioSource && emptySound) audioSource.PlayOneShot(emptySound);
            }
        }
    }

    protected abstract void ExecuteFireLogic();

    public void Fire()
    {
        if (!infiniteAmmo) currentAmmo--;

        // Trigger Animation Tir
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger(_animIDFire);
        }

        if (muzzleFlash) muzzleFlash.Play();
        if (audioSource && shootSound)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(shootSound);
        }

        ApplyRecoil();
        ExecuteFireLogic();
    }

    void ApplyRecoil()
    {
        if (recoilScript) recoilScript.RecoilFire(recoilPunch.x, recoilPunch.y, recoilPunch.z);
        if (crosshairScript) crosshairScript.AddRecoil(crosshairSpread);
        if (_playerController) _playerController.AddCameraRecoil(cameraRecoilVertical, cameraRecoilHorizontal);
    }

    protected IEnumerator ReloadRoutine()
    {
        _isReloading = true;

        // Trigger Animation Reload
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger(_animIDReload);
            // Optionnel : Ajuster la vitesse de l'anim pour qu'elle dure exactement 'reloadTime'
            // weaponAnimator.SetFloat(_animIDReloadSpeed, 1.0f / reloadTime); 
        }

        if (audioSource && reloadSound) audioSource.PlayOneShot(reloadSound);

        // On attend la durée définie dans l'inspecteur (plutôt que 1.5f en dur)
        yield return new WaitForSeconds(reloadTime);

        int needed = maxAmmo - currentAmmo;

        if (needed <= 0)
        {
            _isReloading = false;
            yield break;
        }

        int taken = 0;
        if (ammoItemData != null && _inventory != null)
        {
            taken = _inventory.ConsumeItem(ammoItemData, needed);
        }
        else
        {
            taken = needed;
        }

        if (taken > 0)
        {
            currentAmmo += taken;
            FindAnyObjectByType<InventoryUI>()?.SendMessage("RefreshItems", SendMessageOptions.DontRequireReceiver);
        }

        _isReloading = false;
    }
}