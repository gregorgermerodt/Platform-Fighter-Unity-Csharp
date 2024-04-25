using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RuntimeControllerDetector : MonoBehaviour
{
    private void OnEnable()
    {
        // Hinzufügen des Listeners zum onDeviceChange Ereignis
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        // Entfernen des Listeners, wenn das Skript deaktiviert wird
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            // Gerät wurde verbunden
            Debug.Log("Gerät verbunden: " + device.name);

            // Hier können Sie spezifische Aktionen durchführen, wenn ein Gerät verbunden wird
            // Zum Beispiel: Überprüfen, ob es sich um ein Gamepad handelt
            if (device is Gamepad)
            {
                Debug.Log("Gamepad verbunden: " + device.name);
                // Führen Sie hier Aktionen durch, die spezifisch für Gamepads sind
            }
        }
        else if (change == InputDeviceChange.Removed)
        {
            // Gerät wurde getrennt
            Debug.Log("Gerät getrennt: " + device.name);

            // Hier können Sie spezifische Aktionen durchführen, wenn ein Gerät getrennt wird
        }
    }
}
