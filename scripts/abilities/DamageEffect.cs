using Godot;

[GlobalClass]
public partial class DamageEffect : AbilityEffect
{
    [Export] public int Damage { get; set; } = 10;

    public override void Apply(Unit target, Unit caster, UnitManager unitManager, Vector2I targetCell)
    {
        if (target != null)
            target.TakeDamage(Damage);
    }
}
