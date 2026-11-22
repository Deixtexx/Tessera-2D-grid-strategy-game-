using Godot;

[GlobalClass]
public partial class StatusEffect : AbilityEffect
{
    public enum StatusType { Poison, Burn, Stun }

    [Export] public StatusType Status { get; set; } = StatusType.Poison;
    [Export] public int Duration { get; set; } = 2;
    [Export] public int Power { get; set; } = 3;

    public override int Apply(Unit target, Unit caster, UnitManager unitManager, Vector2I targetCell)
    {
        if (target != null)
            target.ApplyStatus(new StatusInstance(Status, Duration, Power));
        return 0;
    }
}

// Обычный C#-класс, не Resource
public class StatusInstance
{
    public StatusEffect.StatusType Type;
    public int RemainingTurns;
    public int Power;

    public StatusInstance(StatusEffect.StatusType type, int duration, int power)
    {
        Type = type;
        RemainingTurns = duration;
        Power = power;
    }
}
