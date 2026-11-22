using Godot;
using System.Collections.Generic;

public partial class UnitManager : Node
{
    [Export] public PackedScene UnitScene { get; set; }
    [Export] public Grid Grid { get; set; }
    [Export] public TurnManager TurnManager { get; set; }

    private Dictionary<Vector2I, Unit> _unitsOnGrid = new();

    public override void _Ready()
    {
        // Пример спавна трёх разных юнитов
        SpawnUnit(GD.Load<UnitData>("res://assets/data/knight.tres"), new Vector2I(1, 1));
        SpawnUnit(GD.Load<UnitData>("res://assets/data/archer.tres"), new Vector2I(3, 1));
        SpawnUnit(GD.Load<UnitData>("res://assets/data/thug.tres"), new Vector2I(5, 1));
        SpawnUnit(GD.Load<UnitData>("res://assets/data/mage.tres"), new Vector2I(2, 6));
        SpawnUnit(GD.Load<UnitData>("res://assets/data/healer.tres"), new Vector2I(2, 1));
    }

    public void UpdateTeamFrames(UnitData.Faction currentTeam)
    {
        foreach (var unit in _unitsOnGrid.Values)
        {
            bool isCurrent = unit.Data.Team == currentTeam;
            unit.SetTeamFrameVisible(isCurrent, isCurrent ? Colors.Green : Colors.Gray);
        }
    }

    public List<Unit> GetAllUnits() => new List<Unit>(_unitsOnGrid.Values);

    public Unit SpawnUnit(UnitData data, Vector2I cell)
    {
        var unit = UnitScene.Instantiate<Unit>();
        unit.Data = data;
        AddChild(unit);
        PlaceUnit(unit, cell);
        return unit;
    }

    public void PlaceUnit(Unit unit, Vector2I cell)
    {
        if (_unitsOnGrid.ContainsKey(cell))
        {
            GD.PrintErr($"Клетка {cell} уже занята!");
            return;
        }
        unit.GridPosition = cell;
        unit.Position = Grid.ToGlobal(Grid.MapToLocal(cell));
        _unitsOnGrid[cell] = unit;
        unit.UnitMoved += OnUnitMoved;
        unit.UnitDied += OnUnitDied;

        bool isCurrent = unit.Data.Team == TurnManager.CurrentTeam;
        unit.SetTeamFrameVisible(isCurrent, isCurrent ? Colors.Green : Colors.Gray);
    }

    public bool IsCellOccupied(Vector2I cell) => _unitsOnGrid.ContainsKey(cell);
    public Unit GetUnitAt(Vector2I cell) => _unitsOnGrid.GetValueOrDefault(cell);

    private void OnUnitMoved(Unit unit, Vector2I oldCell, Vector2I newCell)
    {
        _unitsOnGrid.Remove(oldCell);
        _unitsOnGrid[newCell] = unit;
    }

    private void OnUnitDied(Unit unit)
    {
        _unitsOnGrid.Remove(unit.GridPosition);
        GD.Print($"Юнит на {unit.GridPosition} умер...");
    }
}
