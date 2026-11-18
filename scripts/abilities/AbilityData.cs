using Godot;

public enum AbilityTargetType { Enemy, Ally, Self, AnyUnit, EmptyCell }

[GlobalClass]
public partial class AbilityData : Resource
{
    [Export] public string AbilityName { get; set; } = "Ability";
    [Export] public int APCost { get; set; } = 2;
    [Export] public AbilityTargetType Target { get; set; } = AbilityTargetType.Enemy;

    // Где можно выбрать цель (ограничение дальности)
    [Export]
    public Godot.Collections.Array<Vector2I> TargetingPattern { get; set; } = new()
    {
        Vector2I.Right, Vector2I.Left, Vector2I.Up, Vector2I.Down
    };

    // Форма эффекта относительно выбранной цели
    [Export]
    public Godot.Collections.Array<Vector2I> EffectPattern { get; set; } = new()
    {
        Vector2I.Zero
    };

    [Export] public Godot.Collections.Array<AbilityEffect> Effects { get; set; } = new();
}
