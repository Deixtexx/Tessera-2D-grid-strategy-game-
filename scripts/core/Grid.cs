using Godot;
using System.Collections.Generic;

public partial class Grid : TileMapLayer
{
    [Export] public int MapWidth { get; set; } = 8;
    [Export] public int MapHeight { get; set; } = 8;

    [Export] public UnitManager UnitManager { get; set; }
    [Export] public TurnManager TurnManager { get; set; }
    [Export] public MessageUI MessageUI { get; set; }
    [Export] public TurnUI TurnUI { get; set; }

    [Export] public TileMapLayer HighlightLayer { get; set; }
    [Export] public int HighlightSourceId { get; set; } = 1;
    [Export] public Vector2I HighlightAtlasCoords { get; set; } = new Vector2I(0, 0);

    [Export] public TileMapLayer AttackHighlightLayer { get; set; }
    [Export] public int AttackHighlightSourceId { get; set; } = 0;
    [Export] public Vector2I AttackHighlightAtlasCoords { get; set; } = new Vector2I(0, 0);

    [Export] public TileMapLayer PreviewHighlightLayer { get; set; }
    [Export] public int PreviewSourceId { get; set; } = 0;
    [Export] public Vector2I PreviewAtlasCoords { get; set; } = new Vector2I(0, 0);

    [Export] public TileMapLayer AbilityRangeLayer { get; set; }
    [Export] public int AbilityRangeSourceId { get; set; } = 0;
    [Export] public Vector2I AbilityRangeAtlasCoords { get; set; } = new Vector2I(0, 0);

    private SelectionController _selection;
    private MovementController _movement;
    private AttackController _attack;
    private AbilityController _ability;
    private TurnController _turn;
    private HighlightController _highlight;

    private List<Vector2I> _reachableCells = new();
    private Dictionary<Vector2I, int> _reachableCosts = new();

    public override void _Ready()
    {
        _highlight = new HighlightController(
            HighlightLayer, HighlightSourceId, HighlightAtlasCoords,
            AttackHighlightLayer, AttackHighlightSourceId, AttackHighlightAtlasCoords,
            PreviewHighlightLayer, PreviewSourceId, PreviewAtlasCoords,
            AbilityRangeLayer, AbilityRangeSourceId, AbilityRangeAtlasCoords

        );

        _selection = new SelectionController();
        _selection.SelectionChanged += OnSelectionChanged;

        _movement = new MovementController(UnitManager, TurnManager, this, MapWidth, MapHeight);
        _attack = new AttackController(UnitManager, TurnManager);
        _ability = new AbilityController(UnitManager, TurnManager);
        _turn = new TurnController(UnitManager, TurnManager);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("end_turn"))
        {
            if (_turn.TryEndTurn())
            {
                _selection.Deselect();
            }
            else
            {
                MessageUI.ShowMessage($"У вас ещё есть AP ({TurnManager.CurrentAP}). Нажмите Space ещё раз.", Colors.Yellow);
            }
            GetViewport().SetInputAsHandled();
            return;
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            if (keyEvent.Keycode == Key.Key1) { SelectAbilityByIndex(0); GetViewport().SetInputAsHandled(); return; }
            if (keyEvent.Keycode == Key.Key2) { SelectAbilityByIndex(1); GetViewport().SetInputAsHandled(); return; }
            if (keyEvent.Keycode == Key.Key3) { SelectAbilityByIndex(2); GetViewport().SetInputAsHandled(); return; }
        }

        if (@event is not InputEventMouseButton mouseEvent || !mouseEvent.Pressed)
            return;

        _turn.CancelPending();

        Vector2 localPos = ToLocal(GetGlobalMousePosition());
        Vector2I gridCoords = LocalToMap(localPos);
        var unitAtCell = UnitManager.GetUnitAt(gridCoords);

        if (mouseEvent.ButtonIndex == MouseButton.Right && _selection.SelectedUnit != null)
        {
            _selection.SetMode(_selection.Mode == InteractionMode.Move ? InteractionMode.Attack : InteractionMode.Move);
            return;
        }

        if (mouseEvent.ButtonIndex != MouseButton.Left) return;

        switch (_selection.Mode)
        {
            case InteractionMode.Move:
                HandleMoveClick(gridCoords, unitAtCell);
                break;
            case InteractionMode.Attack:
                HandleAttackClick(gridCoords, unitAtCell);
                break;
            case InteractionMode.Ability:
                HandleAbilityClick(gridCoords, unitAtCell);
                break;
        }
    }

    public override void _Process(double delta)
    {
        if (_selection.Mode != InteractionMode.Ability || _selection.SelectedAbility == null || _selection.SelectedUnit == null)
        {
            _highlight.ClearPreview();
            return;
        }

        Vector2 localPos = ToLocal(GetGlobalMousePosition());
        Vector2I hoveredCell = LocalToMap(localPos);

        Vector2I offset = hoveredCell - _selection.SelectedUnit.GridPosition;
        bool inTargeting = false;
        foreach (var p in _selection.SelectedAbility.TargetingPattern)
            if (p == offset) { inTargeting = true; break; }

        if (!inTargeting)
        {
            _highlight.ClearPreview();
            return;
        }

        var previewCells = _ability.GetPreviewCells(hoveredCell, _selection.SelectedAbility, MapWidth, MapHeight);
        _highlight.ShowPreviewCells(previewCells);
    }

    private void HandleMoveClick(Vector2I cell, Unit unitAtCell)
    {
        if (unitAtCell != null)
        {
            if (unitAtCell.Data.Team != TurnManager.CurrentTeam)
            {
                MessageUI.ShowMessage("Это юнит другой команды", Colors.Red);
                return;
            }
            if (unitAtCell.IsStunned())
            {
                MessageUI.ShowMessage($"{unitAtCell.Data.UnitName} оглушён", Colors.Yellow);
                return;
            }
            if (_selection.SelectedUnit == unitAtCell)
            {
                _selection.Deselect();
                return;
            }
            _selection.SelectUnit(unitAtCell);
            return;
        }

        if (_selection.SelectedUnit == null) return;
        if (!_reachableCells.Contains(cell))
        {
            MessageUI.ShowMessage("Клетка недоступна", Colors.Red);
            return;
        }

        if (_movement.TryMove(_selection.SelectedUnit, cell, _reachableCosts))
        {
            _selection.Deselect();
        }
        else
        {
            MessageUI.ShowMessage("Недостаточно AP", Colors.Red);
        }
    }

    private void HandleAttackClick(Vector2I cell, Unit targetUnit)
    {
        if (_selection.SelectedUnit == null)
        {
            MessageUI.ShowMessage("Сначала выберите юнита", Colors.Red);
            return;
        }
        if (_selection.SelectedUnit.IsStunned())
        {
            MessageUI.ShowMessage("Юнит оглушён", Colors.Red);
            return;
        }
        if (targetUnit == null)
        {
            MessageUI.ShowMessage("Нет цели", Colors.Red);
            return;
        }
        if (targetUnit.Data.Team == _selection.SelectedUnit.Data.Team)
        {
            MessageUI.ShowMessage("Нельзя атаковать союзника", Colors.Red);
            return;
        }
        if (!_attack.IsInAttackPattern(cell, _selection.SelectedUnit))
        {
            MessageUI.ShowMessage("Цель вне паттерна атаки", Colors.Red);
            return;
        }
        if (_attack.TryAttack(_selection.SelectedUnit, targetUnit))
        {
            _selection.Deselect();
        }
        else
        {
            MessageUI.ShowMessage("Недостаточно AP", Colors.Red);
        }
    }

    private void HandleAbilityClick(Vector2I cell, Unit targetUnit)
    {
        var caster = _selection.SelectedUnit;
        var ability = _selection.SelectedAbility;

        if (caster == null || ability == null)
        {
            MessageUI.ShowMessage("Способность не выбрана", Colors.Red);
            return;
        }
        if (caster.IsStunned())
        {
            MessageUI.ShowMessage("Юнит оглушён", Colors.Red);
            return;
        }
        if (!_ability.IsInTargetingPattern(cell, caster, ability))
        {
            MessageUI.ShowMessage("Цель вне зоны способности", Colors.Red);
            return;
        }

        switch (ability.Target)
        {
            case AbilityTargetType.Enemy:
                if (targetUnit == null || targetUnit.Data.Team == caster.Data.Team)
                { MessageUI.ShowMessage("Нужна вражеская цель", Colors.Red); return; }
                break;
            case AbilityTargetType.Ally:
                if (targetUnit == null || targetUnit.Data.Team != caster.Data.Team)
                { MessageUI.ShowMessage("Нужна союзная цель", Colors.Red); return; }
                break;
            case AbilityTargetType.EmptyCell:
                if (targetUnit != null)
                { MessageUI.ShowMessage("Нужна пустая клетка", Colors.Red); return; }
                break;
            case AbilityTargetType.AnyCell:
                // Разрешено и на юнита, и на пустую клетку - ничего не проверяем
                break;
        }

        var result = _ability.TryUseAbility(caster, targetUnit, ability, cell, this);

        switch (result)
        {
            case AbilityController.AbilityResult.Success:
                _selection.Deselect();
                break;
            case AbilityController.AbilityResult.NotEnoughAP:
                MessageUI.ShowMessage("Недостаточно AP", Colors.Red);
                break;
            case AbilityController.AbilityResult.CellOccupied:
                MessageUI.ShowMessage("Клетка занята", Colors.Red);
                break;
            case AbilityController.AbilityResult.InvalidTarget:
                MessageUI.ShowMessage("Недопустимая цель", Colors.Red);
                break;
            case AbilityController.AbilityResult.OutOfRange:
                MessageUI.ShowMessage("Цель вне зоны", Colors.Red);
                break;
        }
    }

    private void OnSelectionChanged()
    {
        _highlight.ClearAll();
        _reachableCells.Clear();
        _reachableCosts.Clear();

        if (_selection.SelectedUnit == null) return;

        if (_selection.Mode == InteractionMode.Move)
        {
            _reachableCells = _movement.GetReachableCells(_selection.SelectedUnit, out _reachableCosts);
            _highlight.ShowMoveCells(_reachableCells);
        }
        else if (_selection.Mode == InteractionMode.Attack)
        {
            var cells = _attack.GetAttackCells(_selection.SelectedUnit, MapWidth, MapHeight);
            _highlight.ShowAttackCells(cells);
        }
        else if (_selection.Mode == InteractionMode.Ability && _selection.SelectedAbility != null)
        {
            var rangeCells = GetTargetingCells(_selection.SelectedUnit, _selection.SelectedAbility);
            _highlight.ShowAbilityRange(rangeCells);
        }
    }

    private void SelectAbilityByIndex(int index)
    {
        if (_selection.SelectedUnit == null)
        {
            MessageUI.ShowMessage("Сначала выберите юнита", Colors.Red);
            return;
        }

        var abilities = _selection.SelectedUnit.Data.Abilities;
        if (index < 0 || index >= abilities.Count)
        {
            MessageUI.ShowMessage("У этого юнита нет такой способности", Colors.Red);
            return;
        }

        _selection.SelectAbility(abilities[index]);
        MessageUI.ShowMessage($"Выбрана способность: {abilities[index].AbilityName}", Colors.Yellow);
    }

    private List<Vector2I> GetTargetingCells(Unit caster, AbilityData ability)
    {
        var result = new List<Vector2I>();
        foreach (var offset in ability.TargetingPattern)
        {
            Vector2I cell = caster.GridPosition + offset;
            if (cell.X >= 0 && cell.X < MapWidth && cell.Y >= 0 && cell.Y < MapHeight)
                result.Add(cell);
        }
        return result;
    }
}
