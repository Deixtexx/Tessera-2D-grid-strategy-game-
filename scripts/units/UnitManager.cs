using Godot;
using System.Collections.Generic;

public partial class UnitManager : Node
{
    [Export] public PackedScene UnitScene { get; set; }
    [Export] public Grid _grid { get; set; }
    private Dictionary<Vector2I, Unit> _unitsOnGrid = new();

    public override void _Ready()
    {
        GD.Print("TEST");
        var unit = UnitScene.Instantiate<Unit>();
        AddChild(unit);

        PlaceUnit(unit, new Vector2I(2, 2));

    }

    // Добавить юнита на поле
    public void PlaceUnit(Unit unit, Vector2I cell)
    {
        if (_unitsOnGrid.ContainsKey(cell))
        {
            GD.PrintErr($"Клетка {cell} уже занята!");
            return;
        }
        unit.GridPosition = cell;
        _unitsOnGrid[cell] = unit;

        unit.Position = _grid.MapToLocal(cell); 

        unit.UnitMoved += OnUnitMoved; // подписка на событие перемещения
    }

    // Убрать юнита с поля
    public void RemoveUnit(Unit unit)
    {
        _unitsOnGrid.Remove(unit.GridPosition);
        unit.UnitMoved -= OnUnitMoved;
    }

    // Проверка занятости
    public bool IsCellOccupied(Vector2I cell) => _unitsOnGrid.ContainsKey(cell);

    // Получить юнита на клетке (или null)
    public Unit GetUnitAt(Vector2I cell) => _unitsOnGrid.GetValueOrDefault(cell);

    // Обработчик события перемещения юнита
    private void OnUnitMoved(Unit unit, Vector2I oldCell, Vector2I newCell)
    {
        _unitsOnGrid.Remove(oldCell);
        _unitsOnGrid[newCell] = unit;
    }
}
