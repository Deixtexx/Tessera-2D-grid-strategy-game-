using Godot;

[GlobalClass]
public partial class PassiveAbility : Resource
{
    [Export] public string PassiveName { get; set; } = "Passive";

    // Хук: вызывается, когда юнит получает урон (до вычета)
    public virtual int ModifyIncomingDamage(int damage, Unit unit) => damage;

    // Вызывается перед нанесением урона, позволяет изменить его
    public virtual int ModifyOutgoingDamage(int damage, Unit attacker, Unit target) => damage;

    // Хук: вызывается, когда юнит должен получить статус
    public virtual bool CanReceiveStatus(StatusInstance status, Unit unit) => true;

    // Хук: вызывается в начале хода юнита (или при смене хода)
    public virtual void OnTurnStart(Unit unit) { }

    // Хук: вызывается при нанесении урона
    public virtual void OnDealDamage(Unit attacker, Unit target, int damageDealt) { }
}
