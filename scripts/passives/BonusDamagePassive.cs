using Godot;

[GlobalClass]
public partial class BonusDamagePassive : PassiveAbility
{
    [Export] public float BonusPercent { get; set; } = 0.2f;

    public override int ModifyOutgoingDamage(int damage, Unit attacker, Unit target)
    {
        return Mathf.RoundToInt(damage * (1 + BonusPercent));
    }
}
