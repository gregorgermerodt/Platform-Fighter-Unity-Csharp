using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicMovesetBlueprint : IMovesetBlueprint
{
    protected override HashSet<string> DefineStates() => new HashSet<string>
    {
        "WALKING_STATE",
        "STANDING_STATE",
    };

    protected override Dictionary<string, int> DefineFlags() => new Dictionary<string, int>
    {
    };

    protected override List<InputAction> DefineInputActions() => new List<InputAction>
    {
        InputManager.Instance.FindInputAction("InGame", "Movement"),
    };

    protected override List<GeneralAnimationCommandWrapper> DefineGeneralAcmds() => new List<GeneralAnimationCommandWrapper>
    {
        new GeneralAnimationCommandWrapper(0, "STATE_TRANSITION_INPUT_GACMD", fm =>
        {
            if (fm.inputActions["Movement"].isStarted)
                fm.TransitionToState("WALKING_STATE");
            else if (fm.inputActions["Movement"].isCanceled)
                fm.TransitionToState("STANDING_STATE");
        }),

        new GeneralAnimationCommandWrapper(1, "TRANSITION_CONDITIONS_GACMD", fm =>
        {
            if (fm.currentState == "WALKING_STATE")
                fm.TransitionToMove("WALKING_ACMD");
            else if (fm.currentState == "STANDING_STATE")
                fm.TransitionToMove("STANDING_ACMD");
        }),
    };

    protected override Dictionary<string, AnimationCommand> DefineAcmds() => new Dictionary<string, AnimationCommand>
    {
        {
            "WALKING_ACMD", fm =>
            {
                if (fm.fighterTranform == null) {
                    int i = fm.frameCounter + 1;
                }
                fm.fighterTranform.position += new Vector3(fm.inputActions["Movement"].inputAction.ReadValue<Vector2>().x, 0.0f, 0.0f) * 10.0f * Time.deltaTime;
            }
        },
        {
            "STANDING_ACMD", fm => { }
        },
    };
}