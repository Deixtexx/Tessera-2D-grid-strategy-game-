using Godot;

[GlobalClass]
public partial class SplashDamageEffect : AbilityEffect
{
    [Export] public int Damage { get; set; } = 5;

    // Форма сплеша относительно цели (крест по умолчанию)
    [Export]
    public Godot.Collections.Array<Vector2I> SplashOffsets { get; set; } = new()
    {
        Vector2I.Zero, Vector2I.Right, Vector2I.Left, Vector2I.Up, Vector2I.Down
    };

    public override Godot.Collections.Array<Vector2I> GetEffectOffsets()
    {
        return SplashOffsets;
    }

    public override void Apply(Unit target, Unit caster, UnitManager unitManager, Vector2I targetCell)
    {
        foreach (var offset in SplashOffsets)
        {
            Vector2I cell = targetCell + offset;
            var unitAtCell = unitManager.GetUnitAt(cell);
            if (unitAtCell != null && unitAtCell.Data.Team != caster.Data.Team)
                unitAtCell.TakeDamage(Damage);
        }
    }
}
