using Godot;

[GlobalClass]
public partial class DashEffect : AbilityEffect
{
    public override int Apply(Unit target, Unit caster, UnitManager unitManager, Vector2I targetCell) => 0;
}
