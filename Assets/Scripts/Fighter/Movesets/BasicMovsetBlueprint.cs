using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicMovesetBlueprint : IMovesetBlueprint
{
    protected override HashSet<string> DefineStates() => new HashSet<string>
    {
        "WALKING_STATE",
        "STANDING_STATE",
        "JUMP_RISING_STATE",
        "FALLING_STATE"
    };

    protected override Dictionary<string, double> DefineUniforms() => new Dictionary<string, double>
    {
        {"DOUBLE_JUMP_PERFORMED_UNIFORM", 0.0},
        {"TRANSITION_TO_FALLING_UNIFORM", 0.0},
        {"JUMPSQUAD_UNIFORM", 0.0}
    };

    protected override List<InputAction> DefineInputActions() => new List<InputAction>
    {
        InputManager.Instance.FindInputAction("InGame", "Movement"),
        InputManager.Instance.FindInputAction("InGame", "Jump"),
    };

    protected override List<GeneralAnimationCommandWrapper> DefineGeneralAcmds() => new List<GeneralAnimationCommandWrapper>
    {
        new GeneralAnimationCommandWrapper(0, "STATE_TRANSITIONS_INPUT_GACMD", fm =>
        {
            if (fm.fighterController.isGrounded && fm.inputActions["Movement"].isNoAction)
                fm.TransitionToState("STANDING_STATE");
            if (!fm.fighterController.isGrounded && fm.fighterController.GetVelocityY() < 0.0f)
                fm.TransitionToState("FALLING_STATE");
        }),
        new GeneralAnimationCommandWrapper(1, "STATE_TRANSITIONS_WALK_GACMD", fm =>
        {
            InputActionWrapper movementInputAction = fm.inputActions["Movement"];
            if ((movementInputAction.isStarted || movementInputAction.isPerformed)
            && Mathf.Abs(movementInputAction.inputAction.ReadValue<Vector2>().x) > 0.1)
                fm.TransitionToState("WALKING_STATE");
        }),
        new GeneralAnimationCommandWrapper(1, "STATE_TRANSITIONS_JUMP_GACMD", fm =>
        {
            if (fm.inputActions["Jump"].isStarted)
                fm.TransitionToState("JUMP_RISING_STATE");
        }),
        new GeneralAnimationCommandWrapper(2, "TRANSITION_CONDITIONS_GACMD", fm =>
        {
            switch (fm.currentState) {
                case "WALKING_STATE":
                    fm.TransitionToAcmd("WALKING_ACMD"); break;
                case "STANDING_STATE":
                    fm.TransitionToAcmd("STANDING_ACMD"); break;
                case "JUMP_RISING_STATE":
                    fm.TransitionToAcmd("JUMP_RISING_ACMD"); break;
                case "FALLING_STATE":
                    fm.TransitionToAcmd("FALLING_ACMD"); break;
            }
        }),
    };

    protected override Dictionary<string, AnimationCommand> DefineAcmds() => new Dictionary<string, AnimationCommand>
    {
        {
            "WALKING_ACMD", fm =>
            {
                fm.fighterController.SetHorizontalVelocity(fm.inputActions["Movement"].inputAction.ReadValue<Vector2>().x * 10.0f);
                //fm.fighterController.transform.position += new Vector3(fm.inputActions["Movement"].inputAction.ReadValue<Vector2>().x, 0.0f, 0.0f) * 10.0f * Time.deltaTime;
            }
        },
        {
            "STANDING_ACMD", fm => {
                if (fm.OnFrame(0))
                {
                    //fm.fighterController.SetHorizontalVelocity(0);
                    Debug.Log("0");
                }
                if (fm.OnFrame(1))
                {
                    Debug.Log("1");
                }
                if (fm.OnFrame(4))
                {
                    Debug.Log("4");
                }
                if (fm.OnFrames(5, 8))
                {
                    Debug.Log(fm.frameCounter);
                }
            }
        },
        {
            "JUMP_RISING_ACMD", fm => {
                if (fm.OnFrame(0)) {
                    fm.SetUniformValue("JUMPSQUAD_UNIFORM", 1.0);
                }
                if (fm.OnFrame(3)) {
                    fm.fighterController.SetVerticalVelocity(20f);
                    fm.SetUniformValue("JUMPSQUAD_UNIFORM", 0.0);
                }
            }
        },
        {
            "FALLING_ACMD", fm => {
                if (fm.OnFrame(0)) {
                    fm.SetUniformValue("TRANSITION_TO_FALLING_UNIFORM", 0.0);
                }
            }
        }
    };
}