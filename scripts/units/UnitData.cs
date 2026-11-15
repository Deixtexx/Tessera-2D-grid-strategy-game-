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
    [Export] public int MovePoints { get; set; } = 5;
    [Export] public MovementType Movement { get; set; } = MovementType.Ground;
    [Export] public Color UnitColor { get; set; } = Colors.Red;
}
