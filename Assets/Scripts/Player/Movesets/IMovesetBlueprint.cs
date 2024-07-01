using System.Collections.Generic;
using UnityEngine.InputSystem;

public abstract class IMovesetBlueprint : UnityEngine.Object
{
    public string movesetName { get; protected set; } = "";
    public MovesetBuilder movesetBuilder { get; protected set; } = new MovesetBuilder();

    protected abstract HashSet<string> DefineStates();
    protected abstract Dictionary<string, int> DefineFlags();
    protected abstract List<InputAction> DefineInputActions();
    protected abstract List<GeneralAnimationCommandWrapper> DefineGeneralAcmds();
    protected abstract Dictionary<string, AnimationCommand> DefineAcmds();

    public IMovesetBlueprint()
    {
        movesetBuilder.AddStates(DefineStates());
        movesetBuilder.AddFlags(DefineFlags());
        movesetBuilder.AddInputActions(DefineInputActions());
        movesetBuilder.AddGeneralAcmds(DefineGeneralAcmds());
        movesetBuilder.AddAcmds(DefineAcmds());
    }
}
