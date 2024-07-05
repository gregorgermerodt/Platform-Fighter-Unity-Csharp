using System.Collections.Generic;
using UnityEngine.InputSystem;

public abstract class IMovesetBlueprint : UnityEngine.Object
{
    public string movesetName { get; protected set; } = "";
    public MovesetBuilder movesetBuilder { get; protected set; } = new MovesetBuilder();

    protected abstract HashSet<string> DefineStates();
    protected abstract Dictionary<string, bool> DefineFlags();
    protected abstract List<InputAction> DefineInputActions();
    protected abstract List<GeneralAnimationCommandWrapper> DefineGeneralAcmds();
    protected abstract Dictionary<string, ACMD> DefineAcmds();

    public IMovesetBlueprint()
    {
        movesetBuilder.AddStates(DefineStates());
        movesetBuilder.AddFlags(DefineFlags());
        movesetBuilder.AddInputActions(DefineInputActions());
        movesetBuilder.AddGeneralAcmds(DefineGeneralAcmds());
        movesetBuilder.AddAcmds(DefineAcmds());
    }

    protected GeneralAnimationCommandWrapper NewGACMD(int priority, string description, ACMD acmd)
        => new GeneralAnimationCommandWrapper(priority, description, acmd);

    protected static void EMPTY_ACMD(FighterMoveset fm) { }
}
