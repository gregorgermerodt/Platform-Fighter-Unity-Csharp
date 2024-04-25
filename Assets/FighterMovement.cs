using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class RuntimeControllerDetector : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset inputActions;
    private InputAction anyKeyAction;
    [SerializeField]
    float delayBetweenConnects = 0.5f;
    float lastConnect;
    int activeDeviceId = -1;

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        if (inputActions != null)
        {
            anyKeyAction = inputActions.FindActionMap("AnyKey").FindAction("AnyKey");
            anyKeyAction.performed += OnAnyKeyPerformed;
            anyKeyAction.Enable();
        }
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    void Start()
    {
        lastConnect = Time.time;
    }

    void Update()
    {

    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            Debug.Log("Gerät verbunden: " + device.name + " Id: " + device.deviceId);
        }
        else if (change == InputDeviceChange.Removed)
        {
            Debug.LogWarning("Gerät getrennt: " + device.name + " Id: " + device.deviceId);
        }
    }

    private void OnAnyKeyPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Taste (" + context.control.name + ") gedrückt von Gerät: " + context.control.device.name + " Id: " + context.control.device.deviceId);

        if (Time.time - lastConnect < delayBetweenConnects)
        {
            Debug.LogWarning("Gerät: " + context.control.device.name + " (Id: " + context.control.device.deviceId
            + ") versucht sich zu schnell zu verbinden!");
        }

        if (activeDeviceId == -1)
        {
                activeDeviceId = context.control.device.deviceId;
            Debug.Log("Gerät: " + context.control.device.name + " (Id: " + context.control.device.deviceId
            + ") ist jetzt das Hauptgerät!");
        }


        lastConnect = Time.time;

    }
}
