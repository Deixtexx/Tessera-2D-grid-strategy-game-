using Godot;

[GlobalClass]
public partial class DashEffect : AbilityEffect
{
    public override void Apply(Unit target, Unit caster, UnitManager unitManager, Vector2I targetCell)
    {
        // Рывок не применяет эффект к цели, он перемещает кастера.
        // Логика перемещения будет в AbilityController, потому что нужен доступ к Grid.
    }
}
