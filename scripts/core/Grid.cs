using Godot;
using System;

public partial class Grid : TileMapLayer
{
	private UnitManager _unitManager;

	private Unit _selectedUnit;

    [Export] public int MapWidth { get; set; } = 8;
    [Export] public int MapHeight { get; set; } = 8;
    
	// Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		_unitManager = GetNode<UnitManager>("../UnitManager");
		GD.Print("скрипт сетки успешно загружен!");
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			Vector2 localPos = ToLocal(GetGlobalMousePosition());
			Vector2I gridCoords = LocalToMap(localPos);
			GD.Print($"Вы нажали на клетку: {gridCoords}");

			var unitAtCell = _unitManager.GetUnitAt(gridCoords);

			if (unitAtCell != null)
			{

				_selectedUnit?.setSelected(false);

				_selectedUnit = unitAtCell;
				_selectedUnit.setSelected(true);

				GD.Print($"Выделен юнит на клетке {gridCoords}");

			}
			else
			{
                if (_selectedUnit != null)
                {
                    // проверка границ карты
                    bool isInsideX = gridCoords.X >= 0 && gridCoords.X < MapWidth;
                    bool isInsideY = gridCoords.Y >= 0 && gridCoords.Y < MapHeight;

                    if (!isInsideX || !isInsideY)
                    {
                        GD.Print($"Клетка {gridCoords} за пределами поля!");
                        return; // Выходим, ничего не делаем
                    }

                    if (!_unitManager.IsCellOccupied(gridCoords))
                    {
                        Vector2 worldPos = MapToLocal(gridCoords);
                        _selectedUnit.MoveTo(gridCoords, worldPos);

						_selectedUnit.setSelected(false);
						_selectedUnit = null;
                        GD.Print($"Юнит перемещён на {gridCoords}");
                    }
                    else
                    {
                        GD.Print("Клетка занята");
                    }
                }
                else
                {
                    GD.Print($"Пустая клетка {gridCoords}, юнит не выбран");
                }
            }
		}
	}
}
