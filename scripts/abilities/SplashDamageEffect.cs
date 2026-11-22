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

    public override int Apply(Unit target, Unit caster, UnitManager unitManager, Vector2I targetCell)
    {
        int total = 0;
        foreach (var offset in SplashOffsets)
        {
            var unitAtCell = unitManager.GetUnitAt(targetCell + offset);
            if (unitAtCell != null && unitAtCell.Data.Team != caster.Data.Team)
            {
                int damage = Damage;
                foreach (var passive in caster.Data.Passives)
                    damage = passive.ModifyOutgoingDamage(damage, caster, unitAtCell);
                total += unitAtCell.TakeDamage(damage);
            }
        }
        return total;
    }
}
