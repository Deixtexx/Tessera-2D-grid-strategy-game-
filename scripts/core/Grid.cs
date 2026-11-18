using Godot;
using System.Collections.Generic;
using static Grid;

public partial class Grid : TileMapLayer
{
    public enum InteractionMode { Move, Attack }

    [Export] public int MapWidth { get; set; } = 8;
    [Export] public int MapHeight { get; set; } = 8;
    [Export] public UnitManager UnitManager { get; set; }
    [Export] public TileMapLayer HighlightLayer { get; set; }
    [Export] public TurnManager TurnManager { get; set; }
    [Export] public int HighlightSourceId { get; set; } = 1;
    [Export] public Vector2I HighlightAtlasCoords { get; set; } = new Vector2I(0, 0);

    private Unit _selectedUnit;
    private List<Vector2I> _reachableCells = new();
    private InteractionMode _mode = InteractionMode.Move;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("end_turn"))
        {
            GD.Print("Конец хода.");
            EndTurn();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (@event is not InputEventMouseButton mouseEvent || !mouseEvent.Pressed)
            return;

        Vector2 localPos = ToLocal(GetGlobalMousePosition());
        Vector2I gridCoords = LocalToMap(localPos);

        // Правый клик — переключение режима
        if (mouseEvent.ButtonIndex == MouseButton.Right && _selectedUnit != null)
        {
            _mode = _mode == InteractionMode.Move ? InteractionMode.Attack : InteractionMode.Move;
            GD.Print($"Режим: {_mode}");
            //if (_selectedUnit != null)
            ShowPatternForSelectedUnit();
            return;
        }

        if (mouseEvent.ButtonIndex != MouseButton.Left)
            return;

        var unitAtCell = UnitManager.GetUnitAt(gridCoords);

        if (_mode == InteractionMode.Move)
            HandleMoveClick(gridCoords, unitAtCell);
        else
            HandleAttackClick(gridCoords, unitAtCell);
    }

    private void HandleMoveClick(Vector2I cell, Unit unitAtCell)
    {
        if (unitAtCell != null)
        {
            if (unitAtCell.Data.Team != TurnManager.CurrentTeam)
            {
                GD.Print("Это юнит другой команды");
                return;
            }
            // если кликнули по уже выделенному
            if (_selectedUnit == unitAtCell)
            {
                DeselectUnit();
                return;
            }
            SelectUnit(unitAtCell);
            return;
        }

        if (_selectedUnit == null) return;

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

        int distance = Mathf.Abs(cell.X - _selectedUnit.GridPosition.X)
             + Mathf.Abs(cell.Y - _selectedUnit.GridPosition.Y);
        int apCost = distance * _selectedUnit.Data.MoveAPCostPerCell;

        if (distance > _selectedUnit.Data.MovePoints)
        {
            GD.Print($"Слишком далеко для этого юнита (макс {_selectedUnit.Data.MovePoints})");
            return;
        }

        if (!TurnManager.TrySpendAP(apCost))
        {
            GD.Print("Недостаточно AP для хода");
            return;
        }

        _selectedUnit.MoveTo(cell, MapToLocal(cell));
        DeselectUnit();
    }

    private void SelectUnit(Unit unit)
    {
        DeselectUnit();
        _selectedUnit = unit;
        _selectedUnit.SetSelected(true);
        ShowPatternForSelectedUnit();
    }

    private void DeselectUnit()
    {
        _selectedUnit?.SetSelected(false);
        _selectedUnit = null;
        _reachableCells.Clear();
        ClearHighlights();
    }


    private void HandleAttackClick(Vector2I cell, Unit targetUnit)
    {
        if (_selectedUnit == null)
        {
            GD.Print("Сначала выберите юнита");
            return;
        }
        if (targetUnit == null)
        {
            GD.Print("Нет цели для атаки");
            return;
        }
        if (targetUnit == _selectedUnit)
        {
            GD.Print("Нельзя атаковать себя");
            return;
        }
        if (!IsInAttackPattern(cell, _selectedUnit))
        {
            GD.Print("Цель вне паттерна атаки");
            return;
        }
        if (targetUnit.Data.Team == TurnManager.CurrentTeam)
        {
            GD.Print("Нельзя атаковать союзника");
            return;
        }
        if (!TurnManager.TrySpendAP(_selectedUnit.Data.AttackAPCost))
        {
            GD.Print("Недостаточно AP для атаки");
            return;
        }

        targetUnit.TakeDamage(_selectedUnit.Data.AttackDamage);
        GD.Print($"{_selectedUnit.Data.UnitName} атакует {targetUnit.Data.UnitName}");

        _mode = InteractionMode.Move;

        _selectedUnit.SetSelected(false);
        _selectedUnit = null;
        ClearHighlights();
    }

    private bool IsInAttackPattern(Vector2I targetCell, Unit attacker)
    {
        Vector2I offset = targetCell - attacker.GridPosition;
        foreach (var patternOffset in attacker.Data.AttackPattern)
        {
            if (offset == patternOffset) return true;
        }
        return false;
    }


    private void ShowPatternForSelectedUnit()
    {
        if (_selectedUnit == null) return;

        ClearHighlights();
        _reachableCells.Clear();

        if (_mode == InteractionMode.Move)
        {
            _reachableCells = GetReachableCells(_selectedUnit);
            HighlightCells(_reachableCells);
        }
        else
        {
            var attackCells = GetAttackCells(_selectedUnit);
            HighlightCells(attackCells);
        }
    }

    private List<Vector2I> GetAttackCells(Unit attacker)
    {
        var result = new List<Vector2I>();
        foreach (var offset in attacker.Data.AttackPattern)
        {
            Vector2I cell = attacker.GridPosition + offset;
            if (cell.X < 0 || cell.X >= MapWidth || cell.Y < 0 || cell.Y >= MapHeight)
                continue;
            result.Add(cell);
        }
        return result;
    }

    private void HighlightCells(List<Vector2I> cells)
    {
        if (HighlightLayer == null) return;
        HighlightLayer.Clear();
        foreach (var cell in cells)
            HighlightLayer.SetCell(cell, HighlightSourceId, HighlightAtlasCoords);
    }

    private void ClearHighlights() => HighlightLayer?.Clear();

    private List<Vector2I> GetReachableCells(Unit unit)
    {
        var result = new List<Vector2I>();
        if (unit.Data == null) return result;

        int budget = Mathf.Min(unit.Data.MovePoints, TurnManager.CurrentAP);

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
                if (next.X < 0 || next.X >= MapWidth || next.Y < 0 || next.Y >= MapHeight) continue;
                if (UnitManager.IsCellOccupied(next)) continue;

                int cost = GetMoveCost(next, unit.Data.Movement);
                if (cost < 0) continue;

                int newRemaining = remaining - cost;
                if (newRemaining < 0) continue;
                if (bestRemaining.TryGetValue(next, out int existing) && existing >= newRemaining) continue;

                bestRemaining[next] = newRemaining;
                result.Add(next);
                queue.Enqueue((next, newRemaining));
            }
        }
        return result;
    }

    private int GetMoveCost(Vector2I cell, UnitData.MovementType movement)
    {
        if (movement == UnitData.MovementType.Flying) return 1;

        TileData tileData = GetCellTileData(cell);
        if (tileData == null) return 1;

        int cost = (int)tileData.GetCustomData("move_cost");
        return cost <= 0 ? 1 : cost;
    }

    private void EndTurn()
    {
        TurnManager.EndTurn();
        DeselectUnit();
    }
}
