using Godot;

[GlobalClass]
public partial class DamageEffect : AbilityEffect
{
    [Export] public int Damage { get; set; } = 10;

    public override int Apply(Unit target, Unit caster, UnitManager unitManager, Vector2I targetCell)
    {
        if (target == null) return 0;

        int damage = Damage;
        foreach (var passive in caster.Data.Passives)
            damage = passive.ModifyOutgoingDamage(damage, caster, target);

        return target.TakeDamage(damage);
    }
}
