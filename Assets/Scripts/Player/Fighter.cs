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

    public int playerNumber;
    [SerializeField] public FighterMoveset fighterMoveset;
    [SerializeField] public CharacterType characterType { get; private set; }

    void Start()
    {
        InputManager inputManager = FindAnyObjectByType<InputManager>();
        inputManager.UpdateDeviceIdsEvent += UpdateDeviceIds;

        InputActionMap inputActionMap = inputManager.inputActions.FindActionMap("InGame");
        fighterMoveset = MovesetRegistry.GetMoveset("BASIC_MOVESET").BuildFighterMoveset(transform);
        
        UpdateDeviceIds(inputManager);
    }

    void FixedUpdate()
    {
        fighterMoveset.UpdateTick(transform);
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