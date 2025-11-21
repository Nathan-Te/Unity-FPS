using UnityEngine;
using TMPro; // Indispensable pour l'écran du digicode
using UnityEngine.Events;
using System.Collections;

public class Keypad : MonoBehaviour
{
    [Header("Sécurité")]
    public string correctCode = "1234";
    public int maxDigits = 4;

    [Header("Feedback Visuel")]
    public TextMeshProUGUI displayText; // L'écran du digicode
    public Renderer statusLight; // Une petite lumière (Rouge/Vert)
    public Color defaultColor = Color.grey;
    public Color successColor = Color.green;
    public Color errorColor = Color.red;

    [Header("Événements")]
    public UnityEvent onAccessGranted; // Ce qui se passe quand c'est bon
    public UnityEvent onAccessDenied;  // Ce qui se passe quand c'est faux

    // État interne
    private string _currentInput = "";
    private bool _isLocked = false; // Pour empêcher de taper pendant l'animation de succès/échec

    void Start()
    {
        UpdateDisplay();
        if (statusLight != null) statusLight.material.color = defaultColor;
    }

    // Appelé par les touches (KeypadKey)
    public void InputKey(string key)
    {
        if (_isLocked) return;

        // Gestion des touches spéciales
        if (key == "Clear" || key == "C")
        {
            _currentInput = "";
            UpdateDisplay();
            return;
        }

        if (key == "Enter" || key == "E")
        {
            CheckCode();
            return;
        }

        // Saisie de chiffre
        if (_currentInput.Length < maxDigits)
        {
            _currentInput += key;
            UpdateDisplay();

            // Optionnel : Auto-validation si on atteint la longueur max
            // if (_currentInput.Length == maxDigits) CheckCode();
        }
    }

    void CheckCode()
    {
        if (_currentInput == correctCode)
        {
            StartCoroutine(HandleResult(true));
        }
        else
        {
            StartCoroutine(HandleResult(false));
        }
    }

    IEnumerator HandleResult(bool success)
    {
        _isLocked = true;

        if (success)
        {
            displayText.text = "OK";
            displayText.color = successColor;
            if (statusLight) statusLight.material.color = successColor;

            onAccessGranted.Invoke();
            // On reste débloqué ou on reset ? Ici on laisse "OK" affiché.
        }
        else
        {
            displayText.text = "ERR";
            displayText.color = errorColor;
            if (statusLight) statusLight.material.color = errorColor;

            onAccessDenied.Invoke();

            yield return new WaitForSeconds(1.0f);

            // Reset
            _currentInput = "";
            UpdateDisplay();
            if (statusLight) statusLight.material.color = defaultColor;
            _isLocked = false;
        }
    }

    void UpdateDisplay()
    {
        // Affiche le code ou des étoiles "****"
        displayText.text = _currentInput;
        displayText.color = Color.white;
    }
}