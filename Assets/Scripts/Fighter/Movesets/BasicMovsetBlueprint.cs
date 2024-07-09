using System;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicMovesetBlueprint : IMovesetBlueprint
{
    const float WALKING_SPEED = 0.07f;

    const float HORIZONTAL_GROUND_DECELERATION = 0.5f;

    const float FALL_SPEED = 0.4f;
    const float FALL_GRAVITY_ACCELERATION = 0.8f;

    const float AIR_DRIFT_SPEED = 0.2f;
    const float AIR_DRIFT_ACCELERATION = 1.0f;

    const float SHORTJUMP_JUMP_SPEED = 0.5f;
    const float SHORTJUMP_AIR_DECELERATION = 22.0f;
    const float FULLJUMP_JUMP_SPEED = 0.7f;
    const float FULLJUMP_AIR_DECELERATION = 28.0f;
    const float AIRJUMP_JUMP_SPEED = 0.8f;
    const float AIRJUMP_AIR_DECELERATION = 28.0f;

    const int MAX_AIR_JUMP_COUNT = 2;

    #region Declarations

    protected override HashSet<string> DefineStates() => new HashSet<string>
    {
        "WALKING_STATE",
        "STANDING_STATE",
        "JUMPSQUATING_STATE",
        "JUMP_RISING_STATE",
        "AIR_JUMP_RISING_STATE",
        "FALLING_STATE",
        "LANDING_STATE",
        "F_TILT_ATTACK_STATE",
        "F_AIR_ATTACK_STATE",
    };

    protected override Dictionary<string, bool> DefineFlags() => new Dictionary<string, bool>
    {
        {"TRANSITION_TO_JUMP_RISING_STATE_FLAG", false},
        {"NO_TRANSITION_TO_FALLING_STATE_FLAG", false},
        {"PERFORMING_SHORTJUMP_FLAG", false},
        {"PERFORMING_FULLJUMP_FLAG", false},
        {"GROUND_JUMP_PERFORMED_FLAG", false},
        {"LAST_AIR_JUMP_PERFORMED", false},
        {"NO_TRANSITION_TO_AIR_JUMP_STATE_FLAG", false},
        {"NO_TRANSITION_TO_STANDING_STATE_FLAG", false},
        {"GRAVITY_ACTIVE_FLAG", true},
        {"PERFORMING_GROUNDED_ATTACK_FLAG", false},
        {"PERFORMING_AERIAL_ATTACK_FLAG", false},
        {"PERFORMING_AIR_JUMP_FLAG", false},
    };

    protected override List<(string, string)> DefineInputActions() => new List<(string, string)>
    {
        ("InGame", "Movement"),
        ("InGame", "Jump"),
        ("InGame", "Attack"),
    };

    protected override List<GeneralAnimationCommandWrapper> DefineGeneralAcmds() => new List<GeneralAnimationCommandWrapper>
    {
        NewGACMD(0, "STATE_TRANSITIONS_INPUT_GACMD", STATE_TRANSITIONS_WALK_GACMD),
        NewGACMD(1, "STATE_TRANSITIONS_JUMP_GACMD", STATE_TRANSITIONS_JUMP_AIR_GACMD),
        NewGACMD(2, "STATE_TRANSITION_ATTACK_GACMD", STATE_TRANSITION_ATTACK_GACMD),
        NewGACMD(1000, "GRAVITY_GACMD", GRAVITY_GACMD),
        NewGACMD(1001, "ACMD_TRANSITION_CONDITIONS_GACMD", ACMD_TRANSITION_CONDITIONS_GACMD),
    };

    protected override Dictionary<string, ACMD> DefineAcmds() => new Dictionary<string, ACMD>
    {
        { "WALKING_ACMD", WALKING_ACMD },
        { "STANDING_ACMD", STANDING_ACMD },
        { "JUMPSQUATING_ACMD", JUMPSQUATING_ACMD },
        { "JUMP_RISING_ACMD", JUMP_RISING_ACMD },
        { "AIR_JUMP_RISING_ACMD", AIR_JUMP_RISING_ACMD },
        { "FALLING_ACMD", FALLING_ACMD },
        { "LANDING_ACMD", LANDING_ACMD },
        { "F_TILT_ATTACK_ACMD", F_TILT_ATTACK_ACMD },
        { "F_AIR_ATTACK_ACMD", F_AIR_ATTACK_ACMD },
    };

    #endregion
    #region GACMD


    private static void STATE_TRANSITIONS_WALK_GACMD(FighterMoveset fm)
    {
        if (!fm.fighterController.isGrounded)
            return;

        if (!fm.IsCurrentState("JUMPSQUATING_STATE"))
        {
            if (fm.inputActions["Movement"].isActive
                && !fm.IsCurrentState("LANDING_STATE")
                && !fm.IsCurrentState("FALLING_STATE")
                && fm.IsFlagFalse("PERFORMING_GROUNDED_ATTACK_FLAG"))
            {
                bool isEnoughMovement = Mathf.Abs(fm.inputActions["Movement"].inputAction.ReadValue<Vector2>().x) > 0.1;
                if (isEnoughMovement)
                {
                    fm.TransitionToState("WALKING_STATE");
                }
            }
            else
            {
                if (!fm.fighterController.wasGrounded && fm.fighterController.isGrounded)
                    fm.TransitionToState("LANDING_STATE");
                else if (fm.IsFlagFalse("PERFORMING_GROUNDED_ATTACK_FLAG")
                    && fm.IsFlagFalse("NO_TRANSITION_TO_STANDING_STATE_FLAG")
                    && fm.fighterController.isGrounded)
                {
                    fm.TransitionToState("STANDING_STATE");
                }
            }
        }
    }

    private static void STATE_TRANSITIONS_JUMP_AIR_GACMD(FighterMoveset fm)
    {
        if (fm.fighterController.isGrounded)
        {
            fm.SetFlag("GROUND_JUMP_PERFORMED_FLAG", false);
            fm.SetFlag("LAST_AIR_JUMP_PERFORMED", false);
            fm.airJumpsCount = 0;

            if (fm.inputActions["Jump"].isStarted && fm.IsFlagFalse("PERFORMING_GROUNDED_ATTACK_FLAG"))
                fm.TransitionToState("JUMPSQUATING_STATE");
        }
        else // if (!fm.fighterController.isGrounded)
        {
            if (fm.inputActions["Jump"].isStarted
                && fm.IsFlagFalse("NO_TRANSITION_TO_AIR_JUMP_STATE_FLAG")
                && fm.IsFlagFalse("PERFORMING_AERIAL_ATTACK_FLAG")
                && fm.airJumpsCount < MAX_AIR_JUMP_COUNT)
            {
                fm.TransitionToState("AIR_JUMP_RISING_STATE");
            }
            else if (fm.IsFlagFalse("NO_TRANSITION_TO_FALLING_STATE_FLAG"))
                fm.TransitionToState("FALLING_STATE");
        }

        if (fm.IsFlagTrue("TRANSITION_TO_JUMP_RISING_STATE_FLAG"))
        {
            fm.TransitionToState("JUMP_RISING_STATE");
        }
    }

    private void STATE_TRANSITION_ATTACK_GACMD(FighterMoveset fm)
    {
        if (fm.inputActions["Attack"].isStarted)
        {
            if (fm.fighterController.isGrounded && fm.IsFlagFalse("NO_TRANSITION_TO_STANDING_STATE_FLAG") && fm.IsFlagFalse("PERFORMING_GROUNDED_ATTACK_FLAG"))
            {
                fm.TransitionToState("F_TILT_ATTACK_STATE");
            }
            else if (!fm.fighterController.isGrounded && fm.IsFlagFalse("PERFORMING_AERIAL_ATTACK_FLAG"))
            {
                fm.TransitionToState("F_AIR_ATTACK_STATE");
            }
        }
    }

    private void GRAVITY_GACMD(FighterMoveset fm)
    {
        if (fm.IsFlagTrue("GRAVITY_ACTIVE_FLAG") && !fm.fighterController.isGrounded)
        {
            fm.fighterController.ApproachVerticalVelocity(FALL_GRAVITY_ACCELERATION, -FALL_SPEED);
        }
    }

    private static void ACMD_TRANSITION_CONDITIONS_GACMD(FighterMoveset fm)
    {
        if (fm.IsCurrentState("WALKING_STATE"))
        {
            fm.TransitionToAcmd("WALKING_ACMD");
        }
        else if (fm.IsCurrentState("STANDING_STATE"))
        {
            fm.TransitionToAcmd("STANDING_ACMD");
        }
        else if (fm.IsCurrentState("FALLING_STATE"))
        {
            fm.SetFlag("NO_TRANSITION_TO_FALLING_STATE_FLAG", false);
            fm.TransitionToAcmd("FALLING_ACMD");
        }
        else if (fm.IsCurrentState("JUMPSQUATING_STATE"))
        {
            fm.TransitionToAcmd("JUMPSQUATING_ACMD");
        }
        else if (fm.IsCurrentState("JUMP_RISING_STATE"))
        {
            fm.SetFlag("TRANSITION_TO_JUMP_RISING_STATE_FLAG", false);
            fm.TransitionToAcmd("JUMP_RISING_ACMD");
        }
        else if (fm.IsCurrentState("AIR_JUMP_RISING_STATE"))
        {
            fm.TransitionToAcmd("AIR_JUMP_RISING_ACMD");
        }
        else if (fm.IsCurrentState("LANDING_STATE"))
        {
            fm.TransitionToAcmd("LANDING_ACMD");
        }
        else if (fm.IsCurrentState("F_TILT_ATTACK_STATE"))
        {
            fm.TransitionToAcmd("F_TILT_ATTACK_ACMD");
        }
        else if (fm.IsCurrentState("F_AIR_ATTACK_STATE"))
        {
            if (fm.IsFlagTrue("NO_TRANSITION_TO_FALLING_STATE_FLAG"))
                fm.TransitionToAcmd("F_AIR_ATTACK_ACMD");
            else
                fm.TransitionToAcmd("F_AIR_ATTACK_ACMD");
        }
    }

    #endregion
    #region ACMD

    private void WALKING_ACMD(FighterMoveset fm)
    {
        float joystickMultiplier = fm.inputActions["Movement"].inputAction.ReadValue<Vector2>().x;
        float joystickMultiplierAbs = Mathf.Abs(joystickMultiplier);
        fm.SetAnimationSpeed(joystickMultiplierAbs * 2.8f);

        if (joystickMultiplierAbs > 0)
        {
            fm.SetFaceDirection(joystickMultiplier < 0 ? FighterMoveset.FaceDirection.LEFT : FighterMoveset.FaceDirection.RIGHT);
        }

        fm.fighterController.SetHorizontalVelocity(joystickMultiplier * WALKING_SPEED);
        //if (!fm.IsAnimationPlaying()
        //|| fm.GetCurrentAnimationName() == "Idle"
        //|| fm.GetCurrentAnimationName() == "AirIdleOld"
        //|| fm.GetCurrentAnimationName() == "Landing")
        if (fm.GetCurrentAnimationName() != "Walking" || (fm.GetCurrentAnimationName() != "Landing" && !fm.IsAnimationPlaying()))
            fm.PlayAnimation("Walking");
    }

    private void STANDING_ACMD(FighterMoveset fm)
    {
        fm.fighterController.ApproachHorizontalVelocity(HORIZONTAL_GROUND_DECELERATION, 0.0f);
        if (!fm.IsAnimationPlaying() || fm.GetCurrentAnimationName() == "Walking" || fm.GetCurrentAnimationName() == "AirIdleOld")
            fm.PlayAnimation("Idle");

    }

    private void JUMPSQUATING_ACMD(FighterMoveset fm)
    {
        if (fm.OnFrame(0))
        {
            fm.PlayAnimation("Jumpsquat", 10, false);
            fm.SetFlag("NO_TRANSITION_TO_FALLING_STATE_FLAG", true);
            fm.SetFlag("NO_TRANSITION_TO_STANDING_STATE_FLAG", true);
        }
        if (fm.OnFrame(3))
        {
            fm.SetFlag("TRANSITION_TO_JUMP_RISING_STATE_FLAG", true);
            fm.SetFlag("NO_TRANSITION_TO_STANDING_STATE_FLAG", false);
        }
    }

    private static void JUMP_RISING_ACMD(FighterMoveset fm)
    {
        float joystickMultiplier = fm.inputActions["Movement"].inputAction.ReadValue<Vector2>().x;
        fm.fighterController.ApproachHorizontalVelocity(AIR_DRIFT_ACCELERATION, AIR_DRIFT_SPEED * joystickMultiplier);

        if (fm.OnFrame(0))
        {
            fm.PlayAnimation("Jumprise", 1, false);

            fm.SetFlag("NO_TRANSITION_TO_FALLING_STATE_FLAG", true);
            fm.SetFlag("GROUND_JUMP_PERFORMED_FLAG", true);
            if (fm.inputActions["Jump"].isPerformed)
            {
                fm.fighterController.SetVerticalVelocity(FULLJUMP_JUMP_SPEED);
                fm.SetFlag("PERFORMING_FULLJUMP_FLAG", true);
            }
            else
            {
                fm.fighterController.SetVerticalVelocity(SHORTJUMP_JUMP_SPEED);
                fm.SetFlag("PERFORMING_SHORTJUMP_FLAG", true);
            }
        }

        if (fm.OnFrame(6))
        {
            if (fm.IsFlagTrue("PERFORMING_FULLJUMP_FLAG"))
                fm.fighterController.ApproachVerticalVelocity(FULLJUMP_AIR_DECELERATION, 0.0f);
            else if (fm.IsFlagTrue("PERFORMING_SHORTJUMP_FLAG"))
                fm.fighterController.ApproachVerticalVelocity(SHORTJUMP_AIR_DECELERATION, 0.0f);
            fm.SetFlag("PERFORMING_FULLJUMP_FLAG", false);
            fm.SetFlag("PERFORMING_SHORTJUMP_FLAG", false);
            fm.SetFlag("NO_TRANSITION_TO_FALLING_STATE_FLAG", false);

            fm.PlayAnimation("JumpriseToAirIdle", 1, false);
        }
    }

    private static void AIR_JUMP_RISING_ACMD(FighterMoveset fm)
    {
        float joystickMultiplier = fm.inputActions["Movement"].inputAction.ReadValue<Vector2>().x;
        fm.fighterController.ApproachHorizontalVelocity(AIR_DRIFT_ACCELERATION, AIR_DRIFT_SPEED * joystickMultiplier);

        if (fm.OnFrame(0))
        {
            fm.PlayAnimation("Jumprise", 1, false);

            fm.SetFlag("NO_TRANSITION_TO_FALLING_STATE_FLAG", true);

            if (fm.IsFlagTrue("GROUND_JUMP_PERFORMED_FLAG"))
                fm.SetFlag("PERFORMING_AIR_JUMP_FLAG", true);
            fm.SetFlag("GROUND_JUMP_PERFORMED_FLAG", true);

            fm.airJumpsCount++;
            if (fm.airJumpsCount == MAX_AIR_JUMP_COUNT)
                fm.SetFlag("LAST_AIR_JUMP_PERFORMED", true);

            fm.fighterController.SetVerticalVelocity(AIRJUMP_JUMP_SPEED);
        }

        if (fm.OnFrame(6))
        {
            fm.SetFlag("PERFORMING_FULLJUMP_FLAG", false);
            fm.SetFlag("PERFORMING_SHORTJUMP_FLAG", false);

            fm.SetFlag("PERFORMING_AIR_JUMP_FLAG", false);

            fm.fighterController.ApproachVerticalVelocity(AIRJUMP_AIR_DECELERATION, 0.0f);
            fm.SetFlag("NO_TRANSITION_TO_FALLING_STATE_FLAG", false);

            fm.PlayAnimation("JumpriseToAirIdle", 1, false);
        }
    }

    private static void FALLING_ACMD(FighterMoveset fm)
    {
        float joystickMultiplier = fm.inputActions["Movement"].inputAction.ReadValue<Vector2>().x;
        fm.fighterController.ApproachHorizontalVelocity(AIR_DRIFT_ACCELERATION, AIR_DRIFT_SPEED * joystickMultiplier);
        if (!fm.IsAnimationPlaying() || fm.GetCurrentAnimationName() == "Walking" || fm.GetCurrentAnimationName() == "Idle")
        {
            fm.PlayAnimation("AirIdleOld", 2);
        }
        if (fm.OnFrames(0, 4) && fm.IsFlagTrue("GROUND_JUMP_PERFORMED_FLAG"))
        {
            if (fm.inputActions["Jump"].isStarted)
            {
                fm.SetFlag("TRANSITION_TO_JUMP_RISING_STATE_FLAG", true);
            }
        }
    }

    private static void LANDING_ACMD(FighterMoveset fm)
    {
        if (fm.OnFrame(0))
        {
            fm.SetFlag("PERFORMING_AERIAL_ATTACK_FLAG", false);
            fm.SetFlag("PERFORMING_GROUNDED_ATTACK_FLAG", false);
            fm.fighterController.SetHorizontalVelocity(0.0f);
            fm.SetFlag("NO_TRANSITION_TO_STANDING_STATE_FLAG", true);
            fm.PlayAnimation("Landing", 16, false);
        }

        if (fm.OnFrame(4))
        {
            fm.SetFlag("NO_TRANSITION_TO_STANDING_STATE_FLAG", false);
        }
    }

    private static void F_TILT_ATTACK_ACMD(FighterMoveset fm)
    {
        if (fm.OnFrame(0))
        {
            fm.SetFlag("PERFORMING_GROUNDED_ATTACK_FLAG", true);
            fm.PlayAnimation("FTilt", 3, false);
            fm.fighterController.SetHorizontalVelocity(0.0f);
        }

        if (fm.OnFrame(6))
        {
            fm.CreateHitbox(duration: 3, damage: 10, baseKnockback: 0.3f, knockback: 0.005f, direction: new Vector2(1, 1), considerLookDirection: true,
                            boneName: "Sword", radius: 0.007f, offset: new Vector3(0, 0, 0.007f));
        }

        if (fm.OnFrame(35))
        {
            fm.SetFlag("PERFORMING_GROUNDED_ATTACK_FLAG", false);
        }
    }

    private static void F_AIR_ATTACK_ACMD(FighterMoveset fm)
    {
        if (fm.OnFrame(0))
        {
            fm.SetFlag("PERFORMING_AERIAL_ATTACK_FLAG", true);
            fm.SetFlag("NO_TRANSITION_TO_FALLING_STATE_FLAG", true);
            fm.PlayAnimation("FAir", 4, false);

            if (fm.IsFlagTrue("PERFORMING_FULLJUMP_FLAG") || fm.IsFlagTrue("PERFORMING_SHORTJUMP_FLAG") || fm.IsFlagTrue("PERFORMING_AIR_JUMP_FLAG"))
            {
                fm.SetFlag("PERFORMING_FULLJUMP_FLAG", false);
                fm.SetFlag("PERFORMING_SHORTJUMP_FLAG", false);
                fm.SetFlag("PERFORMING_AIR_JUMP_FLAG", false);
                fm.fighterController.ApproachVerticalVelocity(AIRJUMP_AIR_DECELERATION, 0.0f);
            }
        }

        if (fm.OnFrame(8))
        {
            fm.CreateHitbox(duration: 4, damage: 8, baseKnockback: 0.2f, knockback: 0.006f, direction: new Vector2(1, 1), considerLookDirection: true,
                            boneName: "Sword", radius: 0.007f, offset: new Vector3(0, 0, 0.007f));
        }

        if (fm.OnFrame(18))
        {
            fm.SetFlag("PERFORMING_AERIAL_ATTACK_FLAG", false);
            fm.SetFlag("NO_TRANSITION_TO_FALLING_STATE_FLAG", false);
        }
    }

    #endregion
}