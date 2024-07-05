using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public delegate void ACMD(FighterMoveset fm);

public class AnimationCommandWrapper
{
    public string description;
    public ACMD acmd;

    public AnimationCommandWrapper(string description, ACMD acmd)
    {
        this.description = description;
        this.acmd = acmd;
    }
}

public class GeneralAnimationCommandWrapper : AnimationCommandWrapper
{
    public int priority;

    public GeneralAnimationCommandWrapper(int priority, string description, ACMD acmd) : base(description, acmd)
    {
        this.priority = priority;
    }
}

public class MovesetBuilder
{
    public Transform fighterTransform { get; private set; }
    public HashSet<string> states { get; private set; }
    public Dictionary<string, bool> flags { get; private set; }
    public Dictionary<string, InputActionWrapper> inputActions { get; private set; }
    public List<GeneralAnimationCommandWrapper> generalAcmds { get; private set; }
    public Dictionary<string, ACMD> acmds { get; private set; }

    public FighterMoveset.LookDirection lookDirection { get; private set; }

    public MovesetBuilder()
    {
        this.acmds = new Dictionary<string, ACMD>();
        this.states = new HashSet<string>();
        this.flags = new Dictionary<string, bool>();
        this.inputActions = new Dictionary<string, InputActionWrapper>();
        this.generalAcmds = new List<GeneralAnimationCommandWrapper>();
        this.lookDirection = FighterMoveset.LookDirection.RIGHT;
    }

    public MovesetBuilder AddState(string state)
    {
        if (!states.Contains(state))
        {
            states.Add(state);
        }
        return this;
    }

    public MovesetBuilder AddStates(HashSet<string> states)
    {
        foreach (var state in states)
        {
            AddState(state);
        }
        return this;
    }

    public MovesetBuilder AddFlag(string key, bool value = false)
    {
        flags[key] = value;
        return this;
    }

    public MovesetBuilder AddFlags(Dictionary<string, bool> flags)
    {
        foreach (var pair in flags)
        {
            AddFlag(pair.Key, pair.Value);
        }
        return this;
    }

    public MovesetBuilder AddInputAction(InputAction inputAction)
    {
        inputActions[inputAction.name] = new InputActionWrapper(inputAction);
        return this;
    }

    public MovesetBuilder AddInputAction(InputActionWrapper inputActionWrapper)
    {
        inputActions[inputActionWrapper.inputAction.name] = inputActionWrapper;
        return this;
    }

    public MovesetBuilder AddInputActions(Dictionary<string, InputActionWrapper> inputActions)
    {
        foreach (var pair in inputActions)
        {
            AddInputAction(pair.Value);
        }
        return this;
    }

    public MovesetBuilder AddInputActions(List<InputAction> inputActions)
    {
        foreach (var ia in inputActions)
        {
            AddInputAction(ia);
        }
        return this;
    }

    public MovesetBuilder AddInputActions(List<InputActionWrapper> inputActions)
    {
        foreach (var ia in inputActions)
        {
            AddInputAction(ia);
        }
        return this;
    }

    public MovesetBuilder AddGeneralAcmd(int priority, string description, ACMD acmd)
    {
        generalAcmds.Add(new GeneralAnimationCommandWrapper(priority, description, acmd));
        return this;
    }

    public MovesetBuilder AddGeneralAcmd(GeneralAnimationCommandWrapper gacmd)
    {
        if (!generalAcmds.Contains(gacmd))
        {
            generalAcmds.Add(gacmd);
        }
        return this;
    }

    public MovesetBuilder AddGeneralAcmds(List<GeneralAnimationCommandWrapper> generalAcmds)
    {
        foreach (var gacmd in generalAcmds)
        {
            AddGeneralAcmd(gacmd);
        }
        return this;
    }

    public MovesetBuilder AddAcmd(string key, ACMD acmd)
    {
        acmds[key] = acmd;
        return this;
    }

    public MovesetBuilder AddAcmds(Dictionary<string, ACMD> acmds)
    {
        foreach (var pair in acmds)
        {
            AddAcmd(pair.Key, pair.Value);
        }
        return this;
    }

    public MovesetBuilder MergeData(HashSet<string> states, Dictionary<string, bool> flags, Dictionary<string, InputActionWrapper> inputActions,
    List<GeneralAnimationCommandWrapper> generalAcmds, Dictionary<string, ACMD> acmds, FighterMoveset.LookDirection lookDirection)
    {
        AddStates(states);
        AddFlags(flags);
        AddInputActions(inputActions);
        AddGeneralAcmds(generalAcmds);
        AddAcmds(acmds);
        this.lookDirection = lookDirection;
        return this;
    }

    public MovesetBuilder MergeData(FighterMoveset fighterMoveset)
    {
        MergeData(fighterMoveset.states, fighterMoveset.flags, fighterMoveset.inputActions,
        fighterMoveset.generalAcmds, fighterMoveset.acmds, fighterMoveset.lookDirection);
        return this;
    }

    public MovesetBuilder MergeData(MovesetBuilder fighterMovesetBuilder)
    {
        MergeData(fighterMovesetBuilder.states, fighterMovesetBuilder.flags, fighterMovesetBuilder.inputActions,
        fighterMovesetBuilder.generalAcmds, fighterMovesetBuilder.acmds, fighterMovesetBuilder.lookDirection);
        return this;
    }

    public FighterMoveset BuildFighterMoveset(FighterController fighterController)
    {
        if (fighterController == null)
            throw new ArgumentNullException(nameof(fighterController), "fighterTransform darf nicht null sein.");

        generalAcmds.Sort((gacmd1, gacmd2) => gacmd1.priority.CompareTo(gacmd2.priority));
        return new FighterMoveset(fighterController, acmds, generalAcmds, states, flags, inputActions, lookDirection);
    }
}
