using Godot;

[GlobalClass]
public partial class VampirismPassive : PassiveAbility
{
    [Export] public float LifestealPercent { get; set; } = 0.3f; // 30% от урона

    public override void OnDealDamage(Unit attacker, Unit target, int damageDealt)
    {
        int healAmount = Mathf.RoundToInt(damageDealt * LifestealPercent);
        if (healAmount > 0)
        {
            attacker.Heal(healAmount);
            GD.Print($"{attacker.Data.UnitName} вампиризм: +{healAmount} HP");
        }
    }
}
