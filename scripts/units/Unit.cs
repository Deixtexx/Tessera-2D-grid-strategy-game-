using Godot;
using System;

public partial class Unit : Node2D
{
    [Export] public UnitData Data { get; set; }

    public Vector2I GridPosition { get; set; }
    public int CurrentHealth { get; private set; }

    public event Action<Unit, Vector2I, Vector2I> UnitMoved;
    public event Action<Unit> UnitDied;

    private Sprite2D _visual;
    private Sprite2D _selectionFrame;

    private Sprite2D _teamFrame;

    public override void _Ready()
    {
        _visual = GetNode<Sprite2D>("Sprite2D");
        _selectionFrame = GetNode<Sprite2D>("SelectionFrame");
        _teamFrame = GetNode<Sprite2D>("TeamFrame");
        _selectionFrame.Visible = false;

        if (Data != null)
        {
            _visual.SelfModulate = Data.UnitColor;
            CurrentHealth = Data.MaxHealth;
        }
    }

    public void SetTeamFrameVisible(bool visible, Color? color = null)
    {
        if (_teamFrame == null) return;
        _teamFrame.Visible = visible;
        if (color.HasValue)
            _teamFrame.SelfModulate = color.Value;
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
        _selectionFrame.Visible = selected;
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        GD.Print($"{Data.UnitName}: {CurrentHealth}/{Data.MaxHealth} HP");

        if (CurrentHealth <= 0)
        {
            UnitDied?.Invoke(this);
            QueueFree();
        }
    }
}