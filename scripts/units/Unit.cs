using Godot;
using System;

public partial class Unit : Node2D
{
    private bool _isSelected = false;
    public Vector2I GridPosition { get; set; }

    // Событие перемещения: (юнит, новая клетка)
    public event Action<Unit, Vector2I, Vector2I> UnitMoved;

    public void setSelected(bool selected)
    {
        _isSelected = selected;

        var visual = GetNode<CanvasItem>("Sprite2D");
        visual.Modulate = selected ? new Color(1.5f, 1.5f, 1.5f) : Colors.White;
    }
    public void MoveTo(Vector2I newCell, Vector2 worldPosition)
    {
        var oldCell = GridPosition;
        GridPosition = newCell;

        Position = worldPosition;
        UnitMoved?.Invoke(this, oldCell, newCell); // передаём обе
    }
}
