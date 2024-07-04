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

    public FighterPhysics fighterPhysics;

    public HashSet<string> states { get; private set; }
    public Dictionary<string, double> uniforms { get; private set; }
    public Dictionary<string, InputActionWrapper> inputActions { get; private set; }
    public List<GeneralAnimationCommandWrapper> generalAcmds { get; private set; }
    public Dictionary<string, AnimationCommand> acmds { get; private set; }

    public LookDirection lookDirection { get; private set; }

    private AnimationCommand currentAcmd;
    public string currentAcmdName { get; private set; }
    public string currentState { get; private set; }

    public int frameCounter { get; private set; } = 0;
    public int targetFrame { get; private set; } = 0;
    public bool isTargetFrameSet { get; private set; }
    public bool isFrameExecuted { get; private set; }


    public FighterMoveset(FighterPhysics fighterPhysics, Dictionary<string, AnimationCommand> acmds,
        List<GeneralAnimationCommandWrapper> generalAcmds, HashSet<string> states,
        Dictionary<string, double> uniforms, Dictionary<string, InputActionWrapper> inputActions, LookDirection lookDirection)
    {
        this.fighterPhysics = fighterPhysics;

        this.acmds = acmds;
        this.states = states;
        this.uniforms = uniforms;
        this.inputActions = inputActions;
        this.generalAcmds = generalAcmds;

        this.lookDirection = lookDirection;

        this.currentAcmd = acmds["STANDING_ACMD"];
        this.currentState = "STANDING";

        AddDebugAcmd();
    }

    private void AddDebugAcmd()
    {
        if (!acmds.ContainsKey("print_error_move"))

            if (!acmds.ContainsKey("error_acmd"))
                acmds.Add("error_acmd", fm => { });

        if (!states.Contains("error_state"))
            states.Add("error_state");
    }

    public void UpdateTick()
    {
        isTargetFrameSet = false;
        isFrameExecuted = false;

        targetFrame = 0;
        foreach (var gacmd in generalAcmds)
        {
            gacmd.acmd(this);
        }
        if (frameCounter == 0)
            Debug.Log("Running ACMD: \"" + currentAcmdName + "\", Current State: " + currentState);

        currentAcmd(this);

        fighterPhysics.UpdateTick();

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

    public void TransitionToState(string stateName)
    {
        if (currentState == stateName)
        {
            return;
        }
        if (states.Contains(stateName))
        {
            currentState = stateName;
        }
        else
        {
            Debug.LogWarning("TransitionToState(): State with name \"" + stateName + "\" not found. \n");
        }
        //Debug.Log("Transitioned to State: " + currentState);
    }

    public void SetUniformValue(string uniformName, double value)
    {
        if (uniforms.ContainsKey(uniformName))
        {
            uniforms[uniformName] = value;
        }
        else
        {
            Debug.LogWarning("SetUniformValue(): Uniform with name \"" + uniformName + "\" not found. \n");
        }
        Debug.Log("Set Uniform " + uniformName + " to value " + value);
    }

    public bool IsUniformSet(string uniformName)
    {
        if (uniforms.ContainsKey(uniformName))
        {
            return uniforms[uniformName] != 0.0;
        }
        else
        {
            Debug.LogWarning("IsUniformSet(): Uniform with name \"" + uniformName + "\" not found. \n");
        }
        return false;
    }

    public double GetUniformValue(string uniformName)
    {
        if (uniforms.ContainsKey(uniformName))
        {
            return uniforms[uniformName];
        }
        else
        {
            Debug.LogWarning("GetUniformValue(): Uniform with name \"" + uniformName + "\" not found. \n");
        }
        return 0;
    }

    public void OnFrame(int frame)
    {
        if (isTargetFrameSet)
            return;

        if (frameCounter <= frame)
        {
            targetFrame = frame;
            isTargetFrameSet = true;
        }
    }

    public void LoopForFrames(int firstFrame, int lastIncludedFrame)
    {
        if (isTargetFrameSet)
            return;

        if (frameCounter >= firstFrame && frameCounter <= lastIncludedFrame)
        {
            isTargetFrameSet = true;
            targetFrame = frameCounter;
        }
    }

    public bool CanExecute()
    {
        if (isFrameExecuted)
            return false;

        isFrameExecuted = frameCounter == targetFrame;
        return isFrameExecuted;
    }
}