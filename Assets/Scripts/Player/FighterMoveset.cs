using System.Collections.Generic;
using UnityEngine;

public class FighterMoveset
{
    public enum LookDirection
    {
        RIGHT,
        LEFT
    }

    public Transform fighterTranform {get; private set;}

    public HashSet<string> states { get; private set; }
    public Dictionary<string, int> flags { get; private set; }
    public Dictionary<string, InputActionWrapper> inputActions { get; private set; }
    public List<GeneralAnimationCommandWrapper> generalAcmds { get; private set; }
    public Dictionary<string, AnimationCommand> acmds { get; private set; }

    public LookDirection lookDirection { get; private set; }

    private AnimationCommand currentAcmd;
    public string currentAcmdName { get; private set; }
    public string currentState { get; private set; }

    public int frameCounter { get; private set; } = 0;
    public int frameTarget { get; private set; } = 0;

    public FighterMoveset(Transform fighterTranform, Dictionary<string, AnimationCommand> acmds, List<GeneralAnimationCommandWrapper> generalAcmds, HashSet<string> states, Dictionary<string, int> flags, Dictionary<string, InputActionWrapper> inputActions, LookDirection lookDirection)
    {
        this.fighterTranform = fighterTranform;

        this.acmds = acmds;
        this.states = states;
        this.flags = flags;
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

    public void UpdateTick(Transform newTranform)
    {
        fighterTranform = newTranform;
        foreach (var gacmd in generalAcmds)
        {
            gacmd.acmd(this);
        }

        currentAcmd(this);

        foreach (var pair in inputActions)
        {
            pair.Value.ResetInputStates();
        }
        frameCounter++;
    }

    public void TransitionToMove(string acmdName, bool resetFrameCounter = true)
    {
        if (acmds.ContainsKey(acmdName))
        {
            frameCounter = resetFrameCounter ? -1 : frameCounter;
            currentAcmdName = acmdName;
            currentAcmd = acmds.GetValueOrDefault(acmdName);
        }
        else
        {
            Debug.LogWarning("TransitionToMove(): ACMD with name \"" + acmdName + "\" not found. \n");
            currentAcmd = acmds.GetValueOrDefault("error_acmd");
        }
    }

    public void TransitionToState(string stateName)
    {
        if (states.Contains(stateName))
        {
            currentState = stateName;
        }
        else
        {
            Debug.LogWarning("TransitionToState(): State with name \"" + stateName + "\" not found. \n");
        }
    }

    public void SetFlagValue(string flagName, int value)
    {
        if (flags.ContainsKey(flagName))
        {
            flags[flagName] = value;
        }
        else
        {
            Debug.LogWarning("SetFlagValue(): Flag with name \"" + flagName + "\" not found. \n");
        }
    }

    public bool IsFlagSet(string flagName)
    {
        if (flags.ContainsKey(flagName))
        {
            return flags[flagName] != 0;
        }
        else
        {
            Debug.LogWarning("IsFlagSet(): Flag with name \"" + flagName + "\" not found. \n");
        }
        return false;
    }

    public int GetFlagValue(string flagName)
    {
        if (flags.ContainsKey(flagName))
        {
            return flags[flagName];
        }
        else
        {
            Debug.LogWarning("GetFlagValue(): Flag with name \"" + flagName + "\" not found. \n");
        }
        return 0;
    }

    public void WaitUntilFrame(int frame)
    {
        frameTarget = frame;
    }

    public bool CanExecute()
    {
        return frameCounter == frameTarget;
    }
}