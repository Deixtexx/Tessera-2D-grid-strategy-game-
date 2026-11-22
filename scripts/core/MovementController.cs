using Godot;
using System.Collections.Generic;

public class MovementController
{
    private UnitManager _unitManager;
    private TurnManager _turnManager;
    private TileMapLayer _grid;
    private int _mapWidth, _mapHeight;

    public MovementController(UnitManager unitManager, TurnManager turnManager, TileMapLayer grid, int mapWidth, int mapHeight)
    {
        _unitManager = unitManager;
        _turnManager = turnManager;
        _grid = grid;
        _mapWidth = mapWidth;
        _mapHeight = mapHeight;
    }

    public List<Vector2I> GetReachableCells(Unit unit, out Dictionary<Vector2I, int> costs)
    {
        costs = new Dictionary<Vector2I, int>();
        var result = new List<Vector2I>();
        if (unit.Data == null) return result;

        int budget = _turnManager.CurrentAP;
        var queue = new Queue<(Vector2I cell, int remaining)>();
        var bestRemaining = new Dictionary<Vector2I, int>();

        Vector2I start = unit.GridPosition;
        queue.Enqueue((start, budget));
        bestRemaining[start] = budget;
        costs[start] = 0;

        Vector2I[] dirs = { Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right };

        while (queue.Count > 0)
        {
            var (current, remaining) = queue.Dequeue();
            foreach (var dir in dirs)
            {
                Vector2I next = current + dir;
                if (next.X < 0 || next.X >= _mapWidth || next.Y < 0 || next.Y >= _mapHeight) continue;
                if (_unitManager.IsCellOccupied(next)) continue;

                int cost = GetMoveCost(next, unit.Data.Movement) * unit.Data.MoveAPCostPerCell;
                if (cost < 0) continue;

                int newRemaining = remaining - cost;
                if (newRemaining < 0) continue;
                if (bestRemaining.TryGetValue(next, out int existing) && existing >= newRemaining) continue;

                bestRemaining[next] = newRemaining;
                costs[next] = budget - newRemaining;
                result.Add(next);
                queue.Enqueue((next, newRemaining));
            }
        }
        return result;
    }

    public bool TryMove(Unit unit, Vector2I targetCell, Dictionary<Vector2I, int> costs)
    {
        if (!costs.TryGetValue(targetCell, out int apCost)) return false;
        if (!_turnManager.TrySpendAP(apCost)) return false;

        Vector2 worldPos = _grid.MapToLocal(targetCell);
        unit.MoveTo(targetCell, worldPos);
        return true;
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