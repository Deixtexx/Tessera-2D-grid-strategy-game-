using Godot;

[GlobalClass]
public partial class HealEffect : AbilityEffect
{
    [Export] public int HealAmount { get; set; } = 10;

    public override void Apply(Unit target, Unit caster, UnitManager unitManager)
    {
        if (target != null)
            target.Heal(HealAmount);
    }
}
