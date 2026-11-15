using Godot;
using System.Collections.Generic;

public partial class Grid : TileMapLayer
{
    [Export] public int MapWidth { get; set; } = 8;
    [Export] public int MapHeight { get; set; } = 8;
    [Export] public UnitManager UnitManager { get; set; }
    [Export] public TileMapLayer HighlightLayer { get; set; }

    private Unit _selectedUnit;
    private List<Vector2I> _reachableCells = new();

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            Vector2 localPos = ToLocal(GetGlobalMousePosition());
            Vector2I gridCoords = LocalToMap(localPos);

            var unitAtCell = UnitManager.GetUnitAt(gridCoords);

            if (unitAtCell != null)
                SelectUnit(unitAtCell);
            else
                HandleEmptyCellClick(gridCoords);
        }
    }

    private void SelectUnit(Unit unit)
    {
        _selectedUnit?.SetSelected(false);
        _selectedUnit = unit;
        _selectedUnit.SetSelected(true);

        _reachableCells = GetReachableCells(unit);
        ShowReachableCells(_reachableCells);
    }

    private void HandleEmptyCellClick(Vector2I cell)
    {
        if (_selectedUnit == null) return;

        // Клик только по подсвеченной клетке
        if (!_reachableCells.Contains(cell))
        {
            GD.Print("Клетка недоступна для хода");
            return;
        }

        if (UnitManager.IsCellOccupied(cell))
        {
            GD.Print("Клетка занята");
            return;
        }

        Vector2 worldPos = MapToLocal(cell);
        _selectedUnit.MoveTo(cell, worldPos);
        _selectedUnit.SetSelected(false);
        _selectedUnit = null;
        _reachableCells.Clear();
        ClearHighlights();
    }

    private List<Vector2I> GetReachableCells(Unit unit)
    {
        var result = new List<Vector2I>();
        if (unit.Data == null) return result;

        int budget = unit.Data.MovePoints;
        var queue = new Queue<(Vector2I cell, int remaining)>();
        var bestRemaining = new Dictionary<Vector2I, int>();

        Vector2I start = unit.GridPosition;
        queue.Enqueue((start, budget));
        bestRemaining[start] = budget;

        Vector2I[] dirs = { Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right };

        while (queue.Count > 0)
        {
            var (current, remaining) = queue.Dequeue();

            foreach (var dir in dirs)
            {
                Vector2I next = current + dir;

                if (next.X < 0 || next.X >= MapWidth || next.Y < 0 || next.Y >= MapHeight)
                    continue;

                if (UnitManager.IsCellOccupied(next))
                    continue;

                int cost = GetMoveCost(next, unit.Data.Movement);
                if (cost < 0) continue; // непроходимая клетка

                int newRemaining = remaining - cost;
                if (newRemaining < 0) continue;

                if (bestRemaining.TryGetValue(next, out int existing) && existing >= newRemaining)
                    continue;

                bestRemaining[next] = newRemaining;
                result.Add(next);
                queue.Enqueue((next, newRemaining));
            }
        }

        return result;
    }

    // Стоимость входа в клетку. -1 = непроходимо
    private int GetMoveCost(Vector2I cell, UnitData.MovementType movement)
    {
        if (movement == UnitData.MovementType.Flying)
            return 1; // летает, игнорирует местность

        // Читаем custom data из TileSet (слой "move_cost")
        TileData tileData = GetCellTileData(cell);
        if (tileData == null)
            return 1; // пустая клетка — цена 1

        // Если в TileSet нет слоя move_cost, вернёт значение по умолчанию
        int cost = (int)tileData.GetCustomData("move_cost");
        return cost <= 0 ? 1 : cost;
    }

    private void ShowReachableCells(List<Vector2I> cells)
    {
        if (HighlightLayer == null) return;
        HighlightLayer.Clear();
        foreach (var cell in cells)
        {
            HighlightLayer.SetCell(cell, 0, new Vector2I(0, 0));
        }
    }

    private void ClearHighlights()
    {
        HighlightLayer?.Clear();
    }
}
