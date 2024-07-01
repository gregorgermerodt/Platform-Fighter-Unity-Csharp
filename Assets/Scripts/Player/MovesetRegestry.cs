using System.Collections.Generic;

public static class MovesetRegistry
{
    public static Dictionary<string, IMovesetBlueprint> registry { get; private set; } = new Dictionary<string, IMovesetBlueprint>
    {
        { "BASIC_MOVESET", new BasicMovesetBlueprint() },
    };

    public static MovesetBuilder GetMoveset(string type) =>
        registry.TryGetValue(type, out var action) ? action.movesetBuilder : null;

    public static IMovesetBlueprint GetMovesetBlueprint(string type) =>
        registry.TryGetValue(type, out var action) ? action : null;
}
