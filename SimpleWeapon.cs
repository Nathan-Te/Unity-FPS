using UnityEngine;
using System.Collections;

public class SimpleWeapon : MonoBehaviour
{
    [Header("Balistique")]
    public Transform muzzle; // Le bout du canon (Créer un enfant vide)
    public float range = 100f;
    public float damage = 10f;
    public float fireRate = 0.1f; // Temps entre deux tirs
    public LayerMask hitLayers;
    public float impactForce = 5f;

    [Header("Munitions")]
    public int currentAmmo = 12;
    public int maxAmmo = 12;
    public ItemData ammoItemData; // Glisse ici le ScriptableObject "Boîte de Balles"

    [Header("Recul & Sensations")]
    public ProceduralRecoil recoilScript;
    public DynamicCrosshair crosshairScript;
    // Valeurs envoyées au ProceduralRecoil (X=Vertical, Y=Horizontal, Z=Kickback)
    public Vector3 recoilPunch = new Vector3(2f, 0.5f, 0.1f);
    public float crosshairSpread = 30f; // Ouverture du viseur UI

    [Header("Effets Visuels")]
    public ParticleSystem muzzleFlash;
    public GameObject impactPrefab; // Trou de balle ou étincelle
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip emptyClickSound;
    public AudioClip reloadSound;

    private float _nextFireTime;
    private bool _isReloading = false;
    private PlayerInventory _inventory; // Pour trouver les munitions

    void Start()
    {
        // On cherche l'inventaire dans la scène (sur le joueur)
        _inventory = FindAnyObjectByType<PlayerInventory>();
    }

    void Update()
    {
        // Sécurité curseur
        if (Cursor.lockState != CursorLockMode.Locked) return;

        // Tir (Clic Gauche)
        if (Input.GetMouseButton(0) && Time.time >= _nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Shoot();
            }
            else
            {
                // Clic à vide
                _nextFireTime = Time.time + 0.2f;
                if (audioSource && emptyClickSound) audioSource.PlayOneShot(emptyClickSound);
            }
        }

        // Rechargement (R)
        if (Input.GetKeyDown(KeyCode.R) && !_isReloading && currentAmmo < maxAmmo)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    void Shoot()
    {
        _nextFireTime = Time.time + fireRate;
        currentAmmo--;

        // 1. Effets Visuels / Sonores
        if (muzzleFlash) muzzleFlash.Play();
        if (audioSource && shootSound) audioSource.PlayOneShot(shootSound);

        // 2. Connexion aux systèmes existants (Juice)
        if (recoilScript) recoilScript.RecoilFire(recoilPunch.x, recoilPunch.y, recoilPunch.z);
        if (crosshairScript) crosshairScript.AddRecoil(crosshairSpread);

        // 3. Raycast Physique (Depuis le canon, pas la caméra !)
        RaycastHit hit;
        // On tire droit devant le canon. Si le canon bouge (Sway/FreeAim), la balle suit.
        if (Physics.Raycast(muzzle.position, muzzle.forward, out hit, range, hitLayers))
        {
            // Debug visuel
            Debug.DrawLine(muzzle.position, hit.point, Color.red, 1f);

            // Gestion des impacts
            IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            // Poussée physique sur les objets non vivants
            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce, ForceMode.Impulse);
            }

            // Spawn effet impact (Decal)
            if (impactPrefab)
            {
                GameObject impact = Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                impact.transform.parent = hit.collider.transform; // Colle l'impact à l'objet
                Destroy(impact, 5f); // Nettoyage
            }
        }

        var player = GetComponentInParent<HeavyFPSController>();
        if (player)
        {
            // Valeurs arbitraires : 2.0f vers le haut, 0.5f sur les côtés
            player.AddCameraRecoil(2.0f, 0.5f);
        }
    }

    IEnumerator ReloadRoutine()
    {
        // Vérification : A-t-on des munitions dans l'inventaire ?
        // (Si ammoItemData est null, on considère munitions infinies pour le test)
        InventoryItem foundAmmoBox = null;

        // 2. CONSOMMATION MUNITIONS (NOUVEAU)
        if (ammoItemData != null && _inventory != null)
        {
            int needed = maxAmmo - currentAmmo;

            // On demande à l'inventaire de nous trouver 'needed' balles
            int taken = _inventory.ConsumeItem(ammoItemData, needed);

            if (taken > 0)
            {
                currentAmmo += taken;
                // Important : Rafraichir l'UI car des nombres ont changé
                FindAnyObjectByType<InventoryUI>().SendMessage("RefreshItems", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.Log("Pas de munitions trouvées !");
                _isReloading = false;
                yield break;
            }
        }

        _isReloading = true;
        Debug.Log("Reloading...");
        if (audioSource && reloadSound) audioSource.PlayOneShot(reloadSound);

        // Simulation temps de rechargement
        yield return new WaitForSeconds(1.5f);

        // Consommation de l'objet (Tetris)
        if (foundAmmoBox != null)
        {
            _inventory.RemoveItem(foundAmmoBox);
            // Mettre à jour l'UI
            FindAnyObjectByType<InventoryUI>().SendMessage("RefreshItems", SendMessageOptions.DontRequireReceiver);
        }

        currentAmmo = maxAmmo;
        _isReloading = false;
    }
}