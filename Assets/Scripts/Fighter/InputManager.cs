using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    InputAction anyKeyAction;
    InputAction reloadSceneAction;

    [field: SerializeField] public InputActionAsset inputActions { get; set; }

    [SerializeField] float delayBetweenConnects = 0.25f;
    float lastConnect;

    [field: SerializeField] public bool allowControllerAssigns { get; set; }
    [field: SerializeField] public List<int> playerControllerDeviceIds = new List<int> { -1, -1 };
    [field: SerializeField] public int sharedKeyboardDeviceId { get; set; } = -1;
    public event Action<InputManager> UpdateDeviceIdsEvent;

    public static InputManager Instance { get; private set; }

    private float startTime;

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            throw new InvalidOperationException("Multiple InputManager active instances of InputManager in Scene!");
        }
        InputSystem.onDeviceChange += OnDeviceChange;
        if (inputActions != null)
        {
            anyKeyAction = inputActions.FindActionMap("AnyKey").FindAction("AnyKey");
            reloadSceneAction = inputActions.FindActionMap("AnyKey").FindAction("ReloadScene");
        }
        anyKeyAction.started += OnAnyKeyPerformed;
        reloadSceneAction.started += OnReloadScenePerformed;
        reloadSceneAction.Enable();
    }

    private void OnReloadScenePerformed(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnValidate()
    {
        if (UpdateDeviceIdsEvent != null)
            UpdateDeviceIdsEvent.Invoke(this);
    }

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        if (Time.time - startTime < 1f)
            return;

        if (!reloadSceneAction.enabled)
            reloadSceneAction.Enable();
        if (allowControllerAssigns && !anyKeyAction.enabled)
        {
            anyKeyAction.Enable();
        }
        else if (!allowControllerAssigns && anyKeyAction.enabled)
        {
            anyKeyAction.Disable();
        }
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
        if (inputActions != null)
        {
            anyKeyAction = inputActions.FindActionMap("AnyKey").FindAction("AnyKey");
            reloadSceneAction = inputActions.FindActionMap("AnyKey").FindAction("ReloadScene");
        }
        anyKeyAction.started -= OnAnyKeyPerformed;
        reloadSceneAction.started -= OnReloadScenePerformed;

        anyKeyAction.Disable();
        reloadSceneAction.Disable();

        Instance = null;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            Debug.Log("Device connected: " + device.name + " Id: " + device.deviceId);
        }
        else if (change == InputDeviceChange.Removed)
        {
            if (device is Keyboard && sharedKeyboardDeviceId == device.deviceId)
            {
                sharedKeyboardDeviceId = -1;
                Debug.LogWarning("Keyboard device: " + device.name + " (Id: " + device.deviceId + ") has been disconnected and cleared from shared keyboard slot.");
                return;
            }
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
        //Debug.Log("Button (" + context.control.name + ") pressed by Device: " + context.control.device.name +
        //    " (Id: " + context.control.device.deviceId + ").");

        if (Time.time - lastConnect < delayBetweenConnects)
        {
            //Debug.LogWarning("Device: " + context.control.device.name + " (Id: " + context.control.device.deviceId +
            //    ") is trying get assigned before the set delay!");
        }

        if (context.control.device is Keyboard)
        {
            // Track the shared keyboard device id separately so both players can use the same keyboard
            var keyboardId = context.control.device.deviceId;
            if (sharedKeyboardDeviceId != keyboardId)
            {
                sharedKeyboardDeviceId = keyboardId;
                Debug.Log("Keyboard detected (Id: " + keyboardId + ") assigned as shared keyboard.");
                UpdateDeviceIdsEvent.Invoke(this);
            }
        }
        else if (!playerControllerDeviceIds.Contains(context.control.device.deviceId))
            for (int i = 0; i < playerControllerDeviceIds.Count; i++)
                if (playerControllerDeviceIds[i] == -1)
                {
                    playerControllerDeviceIds[i] = context.control.device.deviceId;
                    Debug.Log("Device: " + context.control.device.name + " (Id: " + context.control.device.deviceId
                    + ") is now assigned to Player" + (i + 1) + ".");
                    UpdateDeviceIdsEvent.Invoke(this);
                    break;
                }
        lastConnect = Time.time;
    }

    public static InputAction FindInputAction(InputActionAsset inputActionAsset, string actionMapName, string actionName)
        => inputActionAsset.FindActionMap(actionMapName).FindAction(actionName);
}
