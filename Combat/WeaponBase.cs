using UnityEngine;
using System.Collections;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Configuration Générale")]
    public string weaponName = "Arme";
    public float fireRate = 0.1f;
    public bool isAutomatic = false;

    [Header("Munitions")]
    public int currentAmmo = 12; // C'est LA SEULE variable ammo qui doit exister
    public int maxAmmo = 12;
    public ItemData ammoItemData;

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

    protected float _nextFireTime;
    protected bool _isReloading = false;
    protected PlayerInventory _inventory;
    protected HeavyFPSController _playerController;

    protected virtual void Start()
    {
        _inventory = FindAnyObjectByType<PlayerInventory>();
        _playerController = GetComponentInParent<HeavyFPSController>();
    }

    protected virtual void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        // Bloque le tir si on recharge
        if (_isReloading) return;

        HandleShooting();

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    void HandleShooting()
    {
        bool intentToFire = isAutomatic ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);

        if (intentToFire && Time.time >= _nextFireTime)
        {
            if (currentAmmo > 0)
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
        currentAmmo--; // Modifie la variable du parent

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
        // 1. Début Rechargement
        _isReloading = true;
        if (audioSource && reloadSound) audioSource.PlayOneShot(reloadSound);

        // 2. Attente (Animation) - On ne touche PAS aux balles ici
        yield return new WaitForSeconds(1.5f);

        // 3. Calcul & Consommation (Au dernier moment)
        // On recalcule 'needed' maintenant, au cas où quelque chose aurait changé
        int needed = maxAmmo - currentAmmo;

        // Sécurité : si on est plein, on annule
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
            taken = needed; // Mode Debug (Munitions infinies)
        }

        // 4. Ajout des balles
        if (taken > 0)
        {
            currentAmmo += taken;
            // Mise à jour UI
            FindAnyObjectByType<InventoryUI>()?.SendMessage("RefreshItems", SendMessageOptions.DontRequireReceiver);
        }

        _isReloading = false;
    }
}