using Godot;

public partial class TurnUI : Label
{
    [Export] public TurnManager TurnManager { get; set; }

    public override void _Ready()
    {
        if (TurnManager == null) return;
        TurnManager.TurnChanged += UpdateText;
        UpdateText();
    }

    public void UpdateText()
    {
        if (TurnManager == null) return;
        Text = $"Ход: {TurnManager.CurrentTeam} | AP: {TurnManager.CurrentAP}/{TurnManager.APPerTurn}";
    }
}
