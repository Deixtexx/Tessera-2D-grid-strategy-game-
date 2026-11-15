using Godot;
using System;

public partial class Unit : Node2D
{
    [Export] public UnitData Data { get; set; }

    public Vector2I GridPosition { get; set; }

    public event Action<Unit, Vector2I, Vector2I> UnitMoved;

    private Sprite2D _visual;

    public override void _Ready()
    {
        _visual = GetNode<Sprite2D>("Sprite2D");

        if (Data != null)
        {
            // Если у вас белый спрайт, SelfModulate задаст нужный цвет.
            // Если спрайт уже цветной, лучше использовать отдельный узел-рамку для выделения.
            _visual.SelfModulate = Data.UnitColor;
        }
    }

    public void MoveTo(Vector2I newCell, Vector2 worldPosition)
    {
        var oldCell = GridPosition;
        GridPosition = newCell;
        Position = worldPosition;
        UnitMoved?.Invoke(this, oldCell, newCell);
    }

    public void SetSelected(bool selected)
    {
        // Вариант 1: подсветка через SelfModulate (если спрайт белый/однотонный)
        _visual.SelfModulate = selected ? new Color(1.5f, 1.5f, 1.5f) : Colors.White;

        // Вариант 2 (если нужна рамка): включить/выключить дочерний узел
        // var frame = GetNode<Sprite2D>("SelectionFrame");
        // frame.Visible = selected;
    }
}
