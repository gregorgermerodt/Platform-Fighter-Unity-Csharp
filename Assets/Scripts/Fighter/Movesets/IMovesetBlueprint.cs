using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.LowLevel;

public abstract class IMovesetBlueprint : UnityEngine.Object
{
    public string movesetName { get; protected set; } = "";
    public MovesetBuilder movesetBuilder { get; protected set; } = new MovesetBuilder();

    protected abstract HashSet<string> DefineStates();
    protected abstract Dictionary<string, bool> DefineFlags();
    protected abstract List<(string, string)> DefineInputActions();
    protected abstract List<GeneralAnimationCommandWrapper> DefineGeneralAcmds();
    protected abstract Dictionary<string, ACMD> DefineAcmds();

    public IMovesetBlueprint()
    {
        movesetBuilder.AddStates(DefineStates());
        movesetBuilder.AddFlags(new Dictionary<string, bool>(DefineFlags()));
        movesetBuilder.AddGeneralAcmds(DefineGeneralAcmds());
        movesetBuilder.AddAcmds(DefineAcmds());
    }

    public void CreateInputsfromAsset(InputActionAsset inputActionAsset)
    {
        foreach (var pair in DefineInputActions())
            movesetBuilder.AddInputAction(InputManager.FindInputAction(inputActionAsset, pair.Item1, pair.Item2));
    }

    protected GeneralAnimationCommandWrapper NewGACMD(int priority, string description, ACMD acmd)
        => new GeneralAnimationCommandWrapper(priority, description, acmd);

    protected static void EMPTY_ACMD(FighterMoveset fm) { }
}
