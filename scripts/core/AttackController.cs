using Godot;
using System.Collections.Generic;

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

    public bool TryAttack(Unit attacker, Unit target)
    {
        if (!_turnManager.TrySpendAP(attacker.Data.AttackAPCost)) return false;

        int damage = attacker.Data.AttackDamage;
        foreach (var passive in attacker.Data.Passives)
            damage = passive.ModifyOutgoingDamage(damage, attacker, target);

        int damageDealt = target.TakeDamage(damage);
        foreach (var passive in attacker.Data.Passives)
            passive.OnDealDamage(attacker, target, damageDealt);
        return true;
    }
}
