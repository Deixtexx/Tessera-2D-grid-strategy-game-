using Godot;

[GlobalClass]
public partial class AbilityEffect : Resource
{
    // Смещения относительно цели, куда применяется эффект.
    // По умолчанию только сама цель.
    public virtual Godot.Collections.Array<Vector2I> GetEffectOffsets()
    {
        return new Godot.Collections.Array<Vector2I> { Vector2I.Zero };
    }

    public virtual int Apply(Unit target, Unit caster, UnitManager unitManager, Vector2I targetCell) => 0;
}
