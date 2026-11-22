using Godot;
using System.Collections.Generic;

public class MovementController
{
    private UnitManager _unitManager;
    private TurnManager _turnManager;
    private TileMapLayer _grid;
    private int _mapWidth, _mapHeight;

    public enum MoveResult { Success, NotEnoughAP, Unreachable, CellOccupied, Stunned }

    public MovementController(UnitManager unitManager, TurnManager turnManager, TileMapLayer grid, int mapWidth, int mapHeight)
    {
        _unitManager = unitManager;
        _turnManager = turnManager;
        _grid = grid;
        _mapWidth = mapWidth;
        _mapHeight = mapHeight;
    }

    public void GetReachableCells(Unit unit, out Dictionary<Vector2I, int> costs)
    {
        costs = new Dictionary<Vector2I, int>();
        if (unit?.Data == null) return;

        var queue = new Queue<(Vector2I cell, int cost)>();
        Vector2I start = unit.GridPosition;
        costs[start] = 0;
        queue.Enqueue((start, 0));

        Vector2I[] dirs = { Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right };

        while (queue.Count > 0)
        {
            var (current, currentCost) = queue.Dequeue();
            if (currentCost > costs[current]) continue;      // устаревшая запись в очереди

            foreach (var dir in dirs)
            {
                Vector2I next = current + dir;
                if (next.X < 0 || next.X >= _mapWidth || next.Y < 0 || next.Y >= _mapHeight) continue;
                if (_unitManager.IsCellOccupied(next)) continue;

                int step = GetMoveCost(next, unit.Data.Movement) * unit.Data.MoveAPCostPerCell;
                if (step < 0) continue;

                int newCost = currentCost + step;
                if (costs.TryGetValue(next, out int known) && known <= newCost) continue;

                costs[next] = newCost;
                queue.Enqueue((next, newCost));
            }
        }

        costs.Remove(start);   // своя клетка — не цель перемещения
    }

    public MoveResult TryMove(Unit unit, Vector2I targetCell, Dictionary<Vector2I, int> costs)
    {
        if (unit == null) return MoveResult.Unreachable;
        if (unit.IsStunned()) return MoveResult.Stunned;
        if (_unitManager.IsCellOccupied(targetCell)) return MoveResult.CellOccupied;
        if (!costs.TryGetValue(targetCell, out int apCost)) return MoveResult.Unreachable;
        if (apCost > _turnManager.CurrentAP) return MoveResult.NotEnoughAP;
        if (!_turnManager.TrySpendAP(apCost)) return MoveResult.NotEnoughAP;

        Vector2 worldPos = _grid.ToGlobal(_grid.MapToLocal(targetCell));
        unit.MoveTo(targetCell, worldPos);
        return MoveResult.Success;
    }

    private int GetMoveCost(Vector2I cell, UnitData.MovementType movement)
    {
        if (movement == UnitData.MovementType.Flying) return 1;
        var tileData = _grid.GetCellTileData(cell);
        if (tileData == null) return 1;
        int cost = (int)tileData.GetCustomData("move_cost");
        return cost <= 0 ? 1 : cost;
    }
}