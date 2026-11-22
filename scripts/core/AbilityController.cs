using Godot;
using System.Collections.Generic;

public class AbilityController
{
    public enum AbilityResult
    {
        Success,
        NotEnoughAP,
        CellOccupied,
        InvalidTarget,
        OutOfRange
    }

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

    public bool CanUseAbility(Unit caster, Unit target, AbilityData ability, Vector2I targetCell)
    {
        foreach (var effect in ability.Effects)
        {
            if (effect is DashEffect && _unitManager.IsCellOccupied(targetCell))
                return false; // клетка для приземления занята
        }

        if (_turnManager.CurrentAP < ability.APCost)
            return false;

        return true;
    }

    public AbilityResult TryUseAbility(Unit caster, Unit target, AbilityData ability, Vector2I targetCell, TileMapLayer grid)
    {
        // Проверка клетки для рывка
        foreach (var effect in ability.Effects)
        {
            if (effect is DashEffect && _unitManager.IsCellOccupied(targetCell))
                return AbilityResult.CellOccupied;
        }

        // Проверка AP
        if (_turnManager.CurrentAP < ability.APCost)
            return AbilityResult.NotEnoughAP;

        // Списываем AP
        if (!_turnManager.TrySpendAP(ability.APCost))
            return AbilityResult.NotEnoughAP;

        // Применяем эффекты

        int totalDamage = 0;

        foreach (var effect in ability.Effects)
        {
            if (effect is DashEffect)
            {
                Vector2 worldPos = grid.MapToLocal(targetCell);
                caster.MoveTo(targetCell, worldPos);
            }
            else
            {
                totalDamage += effect.Apply(target, caster, _unitManager, targetCell);
            }
        }

        foreach (var passive in caster.Data.Passives)
            passive.OnDealDamage(caster, target, totalDamage);

        return AbilityResult.Success;
    }
}
