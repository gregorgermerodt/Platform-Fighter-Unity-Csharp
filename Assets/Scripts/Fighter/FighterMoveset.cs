using System;
using System.Collections.Generic;
using UnityEngine;

public class FighterMoveset
{
    public enum LookDirection
    {
        RIGHT,
        LEFT
    }

    public FighterController fighterController;

    public HashSet<string> states { get; private set; }
    public Dictionary<string, bool> flags { get; private set; }
    public Dictionary<string, InputActionWrapper> inputActions { get; private set; }
    public List<GeneralAnimationCommandWrapper> generalAcmds { get; private set; }
    public Dictionary<string, ACMD> acmds { get; private set; }

    public LookDirection lookDirection { get; private set; }

    private ACMD currentAcmd;
    public string currentAcmdName { get; private set; }
    public string currentState { get; private set; }

    public int frameCounter { get; private set; } = 0;
    public int targetFrame { get; private set; } = 0;

    [SerializeField] public int airJumpsCount = 0;

    public FighterMoveset(FighterController fighterController, Dictionary<string, ACMD> acmds,
        List<GeneralAnimationCommandWrapper> generalAcmds, HashSet<string> states,
        Dictionary<string, bool> flags, Dictionary<string, InputActionWrapper> inputActions, LookDirection lookDirection)
    {
        this.fighterController = fighterController;

        this.acmds = acmds;
        this.states = states;
        this.flags = flags;
        this.inputActions = inputActions;
        this.generalAcmds = generalAcmds;

        this.lookDirection = lookDirection;

        this.currentAcmdName = "STANDING_ACMD";
        this.currentAcmd = acmds["STANDING_ACMD"];
        this.currentState = "STANDING_STATE";

        //AddDebugAcmd();
    }

    //private void AddDebugAcmd()
    //{
    //    if (!acmds.ContainsKey("print_error_move"))
    //
    //        if (!acmds.ContainsKey("error_acmd"))
    //            acmds.Add("error_acmd", fm => { });
    //
    //    if (!states.Contains("error_state"))
    //        states.Add("error_state");
    //}

    public void UpdateTick()
    {
        targetFrame = 0;
        fighterController.UpdateTick();

        foreach (var gacmd in generalAcmds)
        {
            gacmd.acmd(this);
        }
        if (frameCounter == 0)
            Debug.Log("Running ACMD: \"" + currentAcmdName + "\", Current State: " + currentState);

        currentAcmd(this);

        foreach (var pair in inputActions)
        {
            pair.Value.ResetInputStates();
        }
        frameCounter++;
    }

    public void ForceTransitionToAcmd(string acmdName, bool resetFrameCounter = true)
    {
        if (acmds.ContainsKey(acmdName))
        {
            frameCounter = resetFrameCounter ? 0 : frameCounter;
            currentAcmdName = acmdName;
            currentAcmd = acmds.GetValueOrDefault(acmdName);
        }
        else
        {
            Debug.LogWarning("TransitionToMove(): ACMD with name \"" + acmdName + "\" not found. \n");
            currentAcmd = acmds.GetValueOrDefault("error_acmd");
        }
        Debug.Log("Force-transitioned to ACMD: " + currentAcmdName);
    }

    public void TransitionToAcmd(string acmdName, bool resetFrameCounter = true)
    {
        if (currentAcmdName == acmdName)
        {
            return;
        }
        if (acmds.ContainsKey(acmdName))
        {
            frameCounter = resetFrameCounter ? 0 : frameCounter;
            currentAcmdName = acmdName;
            currentAcmd = acmds.GetValueOrDefault(acmdName);
        }
        else
        {
            Debug.LogWarning("TransitionToMove(): ACMD with name \"" + acmdName + "\" not found. \n");
            currentAcmd = acmds.GetValueOrDefault("error_acmd");
        }
        //Debug.Log("Transitioned to ACMD: " + currentAcmdName);
    }

    public bool IsCurrentState(string stateName)
    {
        if (!states.Contains(stateName))
        {
            Debug.LogWarning("IsCurrentState(): State with name \"" + stateName + "\" not found. \n");
            return false;
        }
        return currentState == stateName;
    }

    public void TransitionToState(string stateName)
    {
        if (!states.Contains(stateName))
        {
            Debug.LogWarning("TransitionToState(): State with name \"" + stateName + "\" not found. \n");
            return;
        }
        if (currentState != stateName)
        {
            currentState = stateName;
        }

        //Debug.Log("Transitioned to State: " + currentState);
    }

    public void SetFlag(string flagName, bool value)
    {
        if (!flags.ContainsKey(flagName))
        {
            Debug.LogWarning("SetFlagValue(): Flag with name \"" + flagName + "\" not found. \n");
            return;
        }
        if (flags[flagName] != value)
        {
            flags[flagName] = value;
            Debug.Log("Set Flag " + flagName + " to value " + value);
        }
    }

    public bool IsFlagTrue(string flagName)
    {
        if (!flags.ContainsKey(flagName))
        {
            Debug.LogWarning("IsFlagSet(): Flag with name \"" + flagName + "\" not found. \n");
            return false;
        }
        return flags[flagName];
    }

    public bool IsFlagFalse(string flagName) => !IsFlagTrue(flagName);

    public bool OnFrame(int frame) => frame == frameCounter;

    public bool OnFrames(int firstFrame, int lastIncludedFrame) => frameCounter >= firstFrame && frameCounter <= lastIncludedFrame;

}