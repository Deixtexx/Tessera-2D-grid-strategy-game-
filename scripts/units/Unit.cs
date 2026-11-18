using Godot;
using System;
using System.Collections.Generic;

public partial class Unit : Node2D
{
    [Export] public UnitData Data { get; set; }

    public Vector2I GridPosition { get; set; }
    public int CurrentHealth { get; private set; }

    public event Action<Unit, Vector2I, Vector2I> UnitMoved;
    public event Action<Unit> UnitDied;

    // статусы юнита
    private List<StatusInstance> _statuses = new();

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

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, Data.MaxHealth);
        GD.Print($"{Data.UnitName}: +{amount} HP, теперь {CurrentHealth}/{Data.MaxHealth}");
    }

    public void ApplyStatus(StatusInstance status)
    {
        _statuses.Add(status);
        GD.Print($"{Data.UnitName} получил статус {status.Type} на {status.RemainingTurns} ходов");
    }

    public void TickStatuses()
    {
        foreach (var status in _statuses.ToArray())
        {
            switch (status.Type)
            {
                case StatusEffect.StatusType.Poison:
                    TakeDamage(status.Power);
                    GD.Print($"{Data.UnitName}: яд наносит {status.Power} урона");
                    break;
                case StatusEffect.StatusType.Burn:
                    TakeDamage(status.Power);
                    GD.Print($"{Data.UnitName}: огонь наносит {status.Power} урона");
                    break;
                case StatusEffect.StatusType.Stun:
                    GD.Print($"{Data.UnitName} оглушён");
                    break;
            }
            status.RemainingTurns--;
            if (status.RemainingTurns <= 0)
                _statuses.Remove(status);
        }
    }

    public bool IsStunned() => _statuses.Exists(s => s.Type == StatusEffect.StatusType.Stun);
}