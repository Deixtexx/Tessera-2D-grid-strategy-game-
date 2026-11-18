using Godot;

public partial class TurnManager : Node
{
    [Export] public int APPerTurn { get; set; } = 10;

    public UnitData.Faction CurrentTeam { get; private set; } = UnitData.Faction.Player;

    private int _playerAP;
    private int _enemyAP;

    public int CurrentAP => CurrentTeam == UnitData.Faction.Player ? _playerAP : _enemyAP;

    public override void _Ready()
    {
        _playerAP = APPerTurn;
        _enemyAP = APPerTurn;
    }

    public bool TrySpendAP(int cost)
    {
        if (CurrentAP < cost) return false;
        if (CurrentTeam == UnitData.Faction.Player) _playerAP -= cost;
        else _enemyAP -= cost;
        GD.Print($"AP ({CurrentTeam}): {CurrentAP}/{APPerTurn}");
        return true;
    }

    public void EndTurn()
    {
        CurrentTeam = CurrentTeam == UnitData.Faction.Player
            ? UnitData.Faction.Enemy
            : UnitData.Faction.Player;

        // Сброс AP для команды, которая начинает ход
        if (CurrentTeam == UnitData.Faction.Player) _playerAP = APPerTurn;
        else _enemyAP = APPerTurn;

        GD.Print($"Ход команды: {CurrentTeam}, AP: {CurrentAP}");
    }
}
