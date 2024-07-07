using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.InputSystem;

public class Fighter : MonoBehaviour
{
    public enum CharacterType
    {
        None,
        Mario,
        Luigi
    }

    [SerializeField] public int playerNumber;
    [SerializeField] public FighterMoveset fighterMoveset;
    [field: SerializeField] public CharacterType characterType { get; private set; }

    [SerializeField] private bool frameByFrame = false;
    [SerializeField] private bool continueFrame = false;

    void Start()
    {
        InputManager inputManager = FindAnyObjectByType<InputManager>();
        inputManager.UpdateDeviceIdsEvent += UpdateDeviceIds;
        fighterMoveset = MovesetRegistry.GetBuilder("BASIC_MOVESET").BuildFighterMoveset(GetComponentInChildren<FighterController>());
        UpdateDeviceIds(inputManager);
    }

    void OnValidate()
    {
        InputManager inputManager = FindAnyObjectByType<InputManager>();
        try
        {
            UpdateDeviceIds(inputManager);
        }
        catch (System.Exception)
        {
            //Debug.LogWarning("Couldn't update controller for Player " + (playerNumber + 1));
        }
    }

    void FixedUpdate()
    {
        if (fighterMoveset == null)
        {
            InputManager inputManager = FindAnyObjectByType<InputManager>();
            inputManager.UpdateDeviceIdsEvent += UpdateDeviceIds;
            fighterMoveset = MovesetRegistry.GetBuilder("BASIC_MOVESET").BuildFighterMoveset(GetComponentInChildren<FighterController>());
            UpdateDeviceIds(inputManager);
        }
        if (!frameByFrame || continueFrame)
        {
            fighterMoveset.UpdateTick();
            continueFrame = false;
        }
    }

    void OnDisable()
    {
        InputManager inputManager = FindAnyObjectByType<InputManager>();
        if (inputManager != null)
        {
            inputManager.UpdateDeviceIdsEvent -= UpdateDeviceIds;
        }
    }

    protected void UpdateDeviceIds(InputManager inputManager)
    {
        int playerControllerId = inputManager.playerControllerDeviceIds[playerNumber];
        if (playerControllerId == -1)
        {
            foreach (var pair in fighterMoveset.inputActions)
                pair.Value.inputAction.Disable();

            return;
        }
        foreach (var pair in fighterMoveset.inputActions)
        {
            pair.Value.inputAction.Enable();
            pair.Value.inputAction.actionMap.devices = new InputDevice[] { InputSystem.GetDeviceById(playerControllerId) };
        }
    }
}