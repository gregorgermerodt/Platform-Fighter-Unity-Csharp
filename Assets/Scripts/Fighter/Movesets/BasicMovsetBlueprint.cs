using System;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicMovesetBlueprint : IMovesetBlueprint
{
    const float WALKING_SPEED = 0.05f;

    const float HORIZONTAL_GROUND_DECELERATION = 0.5f;

    const float FALL_SPEED = 0.4f;
    const float FALL_GRAVITY_ACCELERATION = 0.8f;

    const float AIR_DRIFT_SPEED = 0.2f;
    const float AIR_DRIFT_ACCELERATION = 1.0f;

    const float SHORTHOP_JUMP_SPEED = 0.5f;
    const float SHORTHOP_DECELERATION = 22.0f;
    const float FULLHOP_JUMP_SPEED = 0.7f;
    const float FULLHOP_DECELERATION = 28.0f;

    #region Declarations

    protected override HashSet<string> DefineStates() => new HashSet<string>
    {
        "WALKING_STATE",
        "STANDING_STATE",
        "JUMPSQUATING_STATE",
        "JUMP_RISING_STATE",
        "FALLING_STATE",
    };

    protected override Dictionary<string, bool> DefineFlags() => new Dictionary<string, bool>
    {
        {"TRANSITION_TO_JUMP_RISING_STATE_FLAG", false},
        {"NO_TRANSITION_TO_FALLING_STATE_FLAG", false},
        {"PERFORMING_SHORTHOP_FLAG", false},
        {"PERFORMING_FULLHOP_FLAG", false},
        {"GROUND_JUMP_PERFORMED_FLAG", false},
        {"GRAVITY_ACTIVE_FLAG", true},
    };

    protected override List<InputAction> DefineInputActions() => new List<InputAction>
    {
        InputManager.Instance.FindInputAction("InGame", "Movement"),
        InputManager.Instance.FindInputAction("InGame", "Jump"),
    };

    protected override List<GeneralAnimationCommandWrapper> DefineGeneralAcmds() => new List<GeneralAnimationCommandWrapper>
    {
        NewGACMD(0, "STATE_TRANSITIONS_INPUT_GACMD", STATE_TRANSITIONS_WALK_GACMD),
        NewGACMD(1, "STATE_TRANSITIONS_JUMP_GACMD", STATE_TRANSITIONS_JUMP_AIR_GACMD),
        NewGACMD(2, "GRAVITY_GACMD", GRAVITY_GACMD),
        NewGACMD(1000, "ACMD_TRANSITION_CONDITIONS_GACMD", ACMD_TRANSITION_CONDITIONS_GACMD),
    };

    protected override Dictionary<string, ACMD> DefineAcmds() => new Dictionary<string, ACMD>
    {
        { "WALKING_ACMD", WALKING_ACMD },
        { "STANDING_ACMD", STANDING_ACMD },
        { "JUMPSQUATING_ACMD", JUMPSQUATING_ACMD },
        { "JUMP_RISING_ACMD", JUMP_RISING_ACMD },
        { "FALLING_ACMD", FALLING_ACMD },
    };

    #endregion
    #region GACMD

    private static void STATE_TRANSITIONS_WALK_GACMD(FighterMoveset fm)
    {
        if (fm.fighterController.isGrounded)
        {
            if (!fm.IsCurrentState("JUMPSQUATING_STATE"))
            {
                if (!fm.inputActions["Movement"].isActive)
                {
                    fm.TransitionToState("STANDING_STATE");
                }
                else if (fm.inputActions["Movement"].isActive)
                {
                    bool isMoving = Mathf.Abs(fm.inputActions["Movement"].inputAction.ReadValue<Vector2>().x) > 0.1;
                    if (isMoving)
                    {
                        fm.TransitionToState("WALKING_STATE");
                    }
                }
            }
        }
    }

    private static void STATE_TRANSITIONS_JUMP_AIR_GACMD(FighterMoveset fm)
    {
        if (fm.fighterController.isGrounded)
        {
            fm.SetFlag("GROUND_JUMP_PERFORMED_FLAG", false);
        }
        if (fm.IsFlagFalse("NO_TRANSITION_TO_FALLING_STATE_FLAG"))
        {
            if (!fm.fighterController.isGrounded)
            {
                fm.TransitionToState("FALLING_STATE");
            }
        }
        if (fm.IsFlagTrue("TRANSITION_TO_JUMP_RISING_STATE_FLAG"))
        {
            fm.TransitionToState("JUMP_RISING_STATE");
        }
        if (fm.fighterController.isGrounded && fm.inputActions["Jump"].isStarted)
        {
            fm.TransitionToState("JUMPSQUATING_STATE");
        }
    }

    private void GRAVITY_GACMD(FighterMoveset fm)
    {
        if (fm.IsFlagTrue("GRAVITY_ACTIVE_FLAG") && !fm.fighterController.isGrounded)
        {
            float joystickMultiplier = fm.inputActions["Movement"].inputAction.ReadValue<Vector2>().x;
            fm.fighterController.ApproachVertivalVelocity(FALL_GRAVITY_ACCELERATION, -FALL_SPEED);
        }
    }

    private static void ACMD_TRANSITION_CONDITIONS_GACMD(FighterMoveset fm)
    {
        if (fm.IsCurrentState("WALKING_STATE"))
        {
            fm.TransitionToAcmd("WALKING_ACMD");
        }
        if (fm.IsCurrentState("STANDING_STATE"))
        {
            fm.TransitionToAcmd("STANDING_ACMD");
        }
        if (fm.IsCurrentState("FALLING_STATE"))
        {
            fm.SetFlag("NO_TRANSITION_TO_FALLING_STATE_FLAG", false);
            fm.TransitionToAcmd("FALLING_ACMD");
        }
        if (fm.IsCurrentState("JUMPSQUATING_STATE"))
        {
            fm.TransitionToAcmd("JUMPSQUATING_ACMD");
        }
        if (fm.IsCurrentState("JUMP_RISING_STATE"))
        {
            fm.SetFlag("TRANSITION_TO_JUMP_RISING_STATE_FLAG", false);
            fm.TransitionToAcmd("JUMP_RISING_ACMD");
        }
    }

    #endregion
    #region ACMD

    private void WALKING_ACMD(FighterMoveset fm)
    {
        float joystickMultiplier = fm.inputActions["Movement"].inputAction.ReadValue<Vector2>().x;
        fm.fighterController.SetHorizontalVelocity(joystickMultiplier * WALKING_SPEED);
        //fm.fighterController.ApproachHorizontalVelocity(AIR_DRIFT_ACCELERATION, joystickMultiplier * WALKING_SPEED);

        //fm.fighterController.transform.position += new Vector3(fm.inputActions["Movement"].inputAction.ReadValue<Vector2>().x, 0.0f, 0.0f) * 10.0f * Time.deltaTime;
    }

    private void STANDING_ACMD(FighterMoveset fm)
    {
        fm.fighterController.ApproachHorizontalVelocity(HORIZONTAL_GROUND_DECELERATION, 0.0f);
    }

    private void JUMPSQUATING_ACMD(FighterMoveset fm)
    {
        if (fm.OnFrame(0))
        {
            fm.SetFlag("NO_TRANSITION_TO_FALLING_STATE_FLAG", true);
        }
        if (fm.OnFrame(3))
        {
            fm.SetFlag("TRANSITION_TO_JUMP_RISING_STATE_FLAG", true);
        }
    }

    private static void JUMP_RISING_ACMD(FighterMoveset fm)
    {
        float joystickMultiplier = fm.inputActions["Movement"].inputAction.ReadValue<Vector2>().x;
        fm.fighterController.ApproachHorizontalVelocity(AIR_DRIFT_ACCELERATION, AIR_DRIFT_SPEED * joystickMultiplier);

        if (fm.OnFrame(0))
        {
            fm.SetFlag("NO_TRANSITION_TO_FALLING_STATE_FLAG", true);
            fm.SetFlag("GROUND_JUMP_PERFORMED_FLAG", true);
            if (fm.inputActions["Jump"].isPerformed)
            {
                fm.fighterController.SetVerticalVelocity(FULLHOP_JUMP_SPEED);
                fm.SetFlag("PERFORMING_FULLHOP_FLAG", true);
            }
            else
            {
                fm.fighterController.SetVerticalVelocity(SHORTHOP_JUMP_SPEED);
                fm.SetFlag("PERFORMING_SHORTHOP_FLAG", true);

            }
        }

        if (fm.OnFrame(6))
        {
            if (fm.IsFlagTrue("PERFORMING_FULLHOP_FLAG"))
                fm.fighterController.ApproachVertivalVelocity(FULLHOP_DECELERATION, 0.0f);
            else if (fm.IsFlagTrue("PERFORMING_SHORTHOP_FLAG"))
                fm.fighterController.ApproachVertivalVelocity(SHORTHOP_DECELERATION, 0.0f);
            fm.SetFlag("PERFORMING_FULLHOP_FLAG", false);
            fm.SetFlag("PERFORMING_SHORTHOP_FLAG", false);
            fm.SetFlag("NO_TRANSITION_TO_FALLING_STATE_FLAG", false);
        }
    }

    private static void FALLING_ACMD(FighterMoveset fm)
    {
        float joystickMultiplier = fm.inputActions["Movement"].inputAction.ReadValue<Vector2>().x;
        fm.fighterController.ApproachHorizontalVelocity(AIR_DRIFT_ACCELERATION, AIR_DRIFT_SPEED * joystickMultiplier);
        if (fm.OnFrames(0, 4) && fm.IsFlagTrue("GROUND_JUMP_PERFORMED_FLAG"))
        {
            if (fm.inputActions["Jump"].isStarted)
            {
                fm.SetFlag("TRANSITION_TO_JUMP_RISING_STATE_FLAG", true);
            }
        }
    }

    #endregion
}