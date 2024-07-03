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

    void Start()
    {
        InputManager inputManager = FindAnyObjectByType<InputManager>();
        inputManager.UpdateDeviceIdsEvent += UpdateDeviceIds;

        InputActionMap inputActionMap = inputManager.inputActions.FindActionMap("InGame");
        fighterMoveset = MovesetRegistry.GetMoveset("BASIC_MOVESET").BuildFighterMoveset(GetComponentInChildren<FighterPhysics>());

        UpdateDeviceIds(inputManager);
    }

    void FixedUpdate()
    {
        fighterMoveset.UpdateTick();
    }

    void OnDestroy()
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