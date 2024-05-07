using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [field: SerializeField] public InputActionAsset inputActions { get; set; }
    InputAction anyKeyAction;

    [SerializeField] float delayBetweenConnects = 0.25f;
    float lastConnect;

    [field: SerializeField] public bool allowControllerAssigns { get; set; }
    [field: SerializeField] public List<int> playerControllerDeviceIds = new List<int> { -1, -1 };

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        if (inputActions != null)
        {
            anyKeyAction = inputActions.FindActionMap("AnyKey").FindAction("AnyKey");
            anyKeyAction.started += OnAnyKeyPerformed;
        }
    }

    void Update()
    {
        if (allowControllerAssigns && !anyKeyAction.enabled)
            anyKeyAction.Enable();
        else if (!allowControllerAssigns && anyKeyAction.enabled)
            anyKeyAction.Disable();
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    void Start()
    {
        lastConnect = Time.time;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            Debug.Log("Device connected: " + device.name + " Id: " + device.deviceId);
        }
        else if (change == InputDeviceChange.Removed)
        {
            for (int i = 0; i < playerControllerDeviceIds.Count; i++)
            {
                if (playerControllerDeviceIds[i] == device.deviceId)
                {
                    playerControllerDeviceIds[i] = -1;
                    Debug.LogWarning("Device: " + device.name + " (Id: " + device.deviceId
                            + ") of Player " + (i + 1) + " has been disconnected!");
                    return;
                }
            }
            Debug.LogWarning("Device disconnected: " + device.name + " Id: " + device.deviceId);
        }
    }

    private void OnAnyKeyPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Button (" + context.control.name + ") pressed by Device: " + context.control.device.name +
            " (Id: " + context.control.device.deviceId + ").");

        if (Time.time - lastConnect < delayBetweenConnects)
        {
            Debug.LogWarning("Device: " + context.control.device.name + " (Id: " + context.control.device.deviceId +
                ") is trying get assigned before the set delay!");
        }
        if (!playerControllerDeviceIds.Contains(context.control.device.deviceId))
            for (int i = 0; i < playerControllerDeviceIds.Count; i++)
                if (playerControllerDeviceIds[i] == -1)
                {
                    playerControllerDeviceIds[i] = context.control.device.deviceId;
                    Debug.Log("Device: " + context.control.device.name + " (Id: " + context.control.device.deviceId
                    + ") is now assigned to Player" + (i + 1) + ".");
                    break;
                }
        lastConnect = Time.time;
    }
}
