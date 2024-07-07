using System.Collections.Generic;

public static class MovesetRegistry
{
    public static Dictionary<string, IMovesetBlueprint> blueprintRegistry { get; private set; } = new Dictionary<string, IMovesetBlueprint>
    {
        { "BASIC_MOVESET", new BasicMovesetBlueprint() },
    };

    public static Dictionary<string, MovesetBuilder> registry { get; private set; } = new Dictionary<string, MovesetBuilder>
    {
        { "BASIC_MOVESET", GetBlueprint("BASIC_MOVESET").movesetBuilder },
    };

    public static MovesetBuilder GetBuilder(string type) =>
        registry.TryGetValue(type, out var action) ? action : null;

    public static IMovesetBlueprint GetBlueprint(string type) =>
        blueprintRegistry.TryGetValue(type, out var action) ? action : null;
}
