using Godot;
using System.Collections.Generic;

public class AbilityController
{
    private UnitManager _unitManager;
    private TurnManager _turnManager;

    public AbilityController(UnitManager unitManager, TurnManager turnManager)
    {
        _unitManager = unitManager;
        _turnManager = turnManager;
    }

    public bool IsInTargetingPattern(Vector2I targetCell, Unit caster, AbilityData ability)
    {
        Vector2I offset = targetCell - caster.GridPosition;
        foreach (var p in ability.TargetingPattern)
            if (p == offset) return true;
        return false;
    }

    public List<Vector2I> GetPreviewCells(Vector2I hoveredCell, AbilityData ability, int mapWidth, int mapHeight)
    {
        var result = new List<Vector2I>();
        foreach (var effect in ability.Effects)
            foreach (var offset in effect.GetEffectOffsets())
            {
                Vector2I cell = hoveredCell + offset;
                if (cell.X >= 0 && cell.X < mapWidth && cell.Y >= 0 && cell.Y < mapHeight)
                    result.Add(cell);
            }
        return result;
    }

    public bool TryUseAbility(Unit caster, Unit target, AbilityData ability)
    {
        if (!_turnManager.TrySpendAP(ability.APCost)) return false;
        foreach (var effect in ability.Effects)
            effect.Apply(target, caster, _unitManager);
        return true;
    }
}
