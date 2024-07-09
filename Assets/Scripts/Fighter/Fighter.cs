using UnityEngine;
using UnityEngine.InputSystem;

public class Fighter : MonoBehaviour
{
    public enum CharacterType
    {
        None,
        Red,
        Green,
    }

    [field: SerializeField] public int playerNumber { get; private set; }
    [SerializeField] public FighterMoveset fighterMoveset;
    [field: SerializeField] public CharacterType characterType { get; private set; }

    [SerializeField] private InputActionAsset inputActionAsset;
    [SerializeField] public float percentDamage;

    [SerializeField] private FighterMoveset.FaceDirection initialFaceDirection;

    [SerializeField] private bool frameByFrame = false;
    [SerializeField] private bool continueFrame = false;

    void Start()
    {
        InputManager inputManager = FindAnyObjectByType<InputManager>();
        inputManager.UpdateDeviceIdsEvent += UpdateDeviceIds;
        fighterMoveset
            = MovesetRegistry.GetBlueprint("BASIC_MOVESET", inputActionAsset).movesetBuilder.BuildFighterMoveset(GetComponentInChildren<FighterController>(), GetComponentInChildren<Animator>());
        UpdateDeviceIds(inputManager);
        fighterMoveset.SetFaceDirection(initialFaceDirection);
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
            fighterMoveset = MovesetRegistry.GetBlueprint("BASIC_MOVESET", inputActionAsset).movesetBuilder.BuildFighterMoveset(GetComponentInChildren<FighterController>(), GetComponentInChildren<Animator>());
            UpdateDeviceIds(inputManager);
        }
        if (!frameByFrame || continueFrame)
        {
            fighterMoveset.UpdateTick();
            continueFrame = false;
        }
        if (!(transform.position.y > -15 && transform.position.y < 30) || !(transform.position.x < 30 || transform.position.x > -30))
        {
            transform.position = new Vector3(0.0f, 25.0f, 0.0f);
            fighterMoveset.fighterController.SetVelocity(Vector2.zero);
            percentDamage = 0.0f;
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