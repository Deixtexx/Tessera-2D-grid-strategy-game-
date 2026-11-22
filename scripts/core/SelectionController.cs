using Godot;

public class SelectionController
{
    public Unit SelectedUnit { get; private set; }
    public AbilityData SelectedAbility { get; private set; }
    public InteractionMode Mode { get; private set; } = InteractionMode.Move;

    public event System.Action SelectionChanged;

    public void SelectUnit(Unit unit)
    {
        SelectedUnit?.SetSelected(false);
        SelectedUnit = unit;
        SelectedUnit?.SetSelected(true);
        SelectedAbility = null;
        Mode = InteractionMode.Move;
        SelectionChanged?.Invoke();
    }

    public void Deselect()
    {
        SelectedUnit?.SetSelected(false);
        SelectedUnit = null;
        SelectedAbility = null;
        Mode = InteractionMode.Move;
        SelectionChanged?.Invoke();
    }

    public void SelectAbility(AbilityData ability)
    {
        if (SelectedUnit == null) return;
        SelectedAbility = ability;
        Mode = InteractionMode.Ability;
        SelectionChanged?.Invoke();
    }

    public void SetMode(InteractionMode mode)
    {
        Mode = mode;
        if (mode != InteractionMode.Ability) SelectedAbility = null;
        SelectionChanged?.Invoke();
    }
}

public enum InteractionMode { Move, Attack, Ability }
