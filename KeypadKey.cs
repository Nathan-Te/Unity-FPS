using UnityEngine;

public class KeypadKey : MonoBehaviour, IInteractable
{
    [Header("Configuration")]
    public Keypad keypadParent; // Référence au cerveau
    public string digit; // "1", "2", "A", "Clear", etc.

    public string InteractionPrompt => $"Appuyer : {digit}";

    public bool Interact(HeavyFPSController player)
    {
        if (keypadParent != null)
        {
            // On simule un appui physique (petit mouvement)
            StartCoroutine(AnimatePress());

            // On envoie l'info
            keypadParent.InputKey(digit);
            return true;
        }
        return false;
    }

    System.Collections.IEnumerator AnimatePress()
    {
        Vector3 initialPos = transform.localPosition;
        Vector3 pressedPos = initialPos + new Vector3(0, 0, 0.005f); // Enfoncement léger (Z local)

        float duration = 0.1f;
        float elapsed = 0;

        // Aller
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(initialPos, pressedPos, elapsed / duration);
            yield return null;
        }

        // Retour
        elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(pressedPos, initialPos, elapsed / duration);
            yield return null;
        }
        transform.localPosition = initialPos;
    }
}