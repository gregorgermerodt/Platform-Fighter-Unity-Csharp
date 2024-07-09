using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public static class MovesetRegistry
{
    public static Dictionary<string, string> blueprintRegistry { get; private set; } = new Dictionary<string, string>
    {
        { "BASIC_MOVESET", typeof(BasicMovesetBlueprint).FullName },
    };

    //public static Dictionary<string, MovesetBuilder> registry { get; private set; } = new Dictionary<string, MovesetBuilder>
    //{
    //    { "BASIC_MOVESET", GetBlueprint("BASIC_MOVESET").movesetBuilder },
    //};

    //public static MovesetBuilder GetBuilder(string type) =>
    //    registry.TryGetValue(type, out var action) ? action : null;


    public static IMovesetBlueprint GetBlueprint(string type, InputActionAsset inputActionAsset)
    {
        if (blueprintRegistry.TryGetValue(type, out var className))
        {
            var instance = CreateInstanceFromClassName(className);
            instance.CreateInputsfromAsset(inputActionAsset);
            return instance;

        }
        return null;
    }

    public static IMovesetBlueprint CreateInstanceFromClassName(string className)
    {
        Type type = Type.GetType(className);
        if (type != null && typeof(IMovesetBlueprint).IsAssignableFrom(type))
        {
            return Activator.CreateInstance(type) as IMovesetBlueprint;
        }
        return null;
    }
}
