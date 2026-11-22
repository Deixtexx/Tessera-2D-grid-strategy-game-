using Godot;

[GlobalClass]
public partial class UnitData : Resource
{
    public enum MovementType
    {
        Ground,   // ходит по земле, учитывает стоимость
        Flying,   // летает, игнорирует стоимость и препятствия (кроме границ)
        Water     // ходит только по воде (задел на будущее)
    }

    [Export] public string UnitName { get; set; } = "Unit";
    [Export] public int MoveAPCostPerCell { get; set; } = 1;
    [Export] public MovementType Movement { get; set; } = MovementType.Ground;
    [Export] public Color UnitColor { get; set; } = Colors.Red;

    public enum Faction { Player, Enemy }

    [Export] public Faction Team { get; set; } = Faction.Player;

    // --- Атака ---
    [Export] public int AttackDamage { get; set; } = 10;
    [Export] public int AttackAPCost { get; set; } = 3;
    [Export] public int MaxHealth { get; set; } = 30;

    // Паттерн атаки: смещения относительно позиции юнита
    [Export]
    public Godot.Collections.Array<Vector2I> AttackPattern { get; set; } = new()
    {
        Vector2I.Right, Vector2I.Left, Vector2I.Up, Vector2I.Down
    };

    [Export] public Godot.Collections.Array<AbilityData> Abilities { get; set; } = new();
}
