using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AttackController
{
    private UnitManager _unitManager;
    private TurnManager _turnManager;

    public AttackController(UnitManager unitManager, TurnManager turnManager)
    {
        _unitManager = unitManager;
        _turnManager = turnManager;
    }

    public List<Vector2I> GetAttackCells(Unit attacker, int mapWidth, int mapHeight)
    {
        var result = new List<Vector2I>();
        foreach (var offset in attacker.Data.AttackPattern)
        {
            Vector2I cell = attacker.GridPosition + offset;
            if (cell.X < 0 || cell.X >= mapWidth || cell.Y < 0 || cell.Y >= mapHeight) continue;
            result.Add(cell);
        }
        return result;
    }

    public bool IsInAttackPattern(Vector2I targetCell, Unit attacker)
    {
        Vector2I offset = targetCell - attacker.GridPosition;
        foreach (var p in attacker.Data.AttackPattern)
            if (p == offset) return true;
        return false;
    }

    public async Task<bool> TryAttack(Unit attacker, Unit target)
    {
        if (!_turnManager.TrySpendAP(attacker.Data.AttackAPCost)) return false;

        GD.Print("there");
        await attacker.PlayAttackAnimationAsync();

        GD.Print("there 3");
        // Наносим урон (в этот момент меч уже опустился)
        int damageDealt = target.TakeDamage(attacker.Data.AttackDamage);

        // Пассивки
        foreach (var passive in attacker.Data.Passives)
            passive.OnDealDamage(attacker, target, damageDealt);

        return true;

    }
}
