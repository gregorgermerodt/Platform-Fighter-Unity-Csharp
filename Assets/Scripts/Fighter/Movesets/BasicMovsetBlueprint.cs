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
            if (fm.fighterPhysics.isGrounded && fm.inputActions["Movement"].isNoAction) 
                fm.TransitionToState("STANDING_STATE");
            if (!fm.fighterPhysics.isGrounded && fm.fighterPhysics.velocity.y < 0.0f)
                fm.TransitionToState("FALLING_STATE");
        }),
        new GeneralAnimationCommandWrapper(1, "STATE_TRANSITIONS_WALK_GACMD", fm =>
        {
            if (fm.inputActions["Movement"].isStarted || fm.inputActions["Movement"].isPerformed)
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
                fm.fighterPhysics.SetHorizontalVelocity(fm.inputActions["Movement"].inputAction.ReadValue<Vector2>().x * 10.0f);
                //fm.fighterPhysics.transform.position += new Vector3(fm.inputActions["Movement"].inputAction.ReadValue<Vector2>().x, 0.0f, 0.0f) * 10.0f * Time.deltaTime;
            }
        },
        {
            "STANDING_ACMD", fm => {
                fm.OnFrame(0);
                if (fm.CanExecute()) {
                    fm.fighterPhysics.SetHorizontalVelocity(0);
                }
                }
        },
        {
            "JUMP_RISING_ACMD", fm => {
                fm.OnFrame(0);
                if (fm.CanExecute()) {
                    fm.SetUniformValue("JUMPSQUAD_UNIFORM", 1.0);
                }
                fm.OnFrame(3);
                if (fm.CanExecute()) {
                    fm.fighterPhysics.SetVerticalVelocity(20f);
                    fm.SetUniformValue("JUMPSQUAD_UNIFORM", 0.0);
                }
            }
        },
        {
            "FALLING_ACMD", fm => {
                fm.OnFrame(0);
                if (fm.CanExecute()) {
                    fm.SetUniformValue("TRANSITION_TO_FALLING_UNIFORM", 0.0);
                }
            }
        }
    };
}