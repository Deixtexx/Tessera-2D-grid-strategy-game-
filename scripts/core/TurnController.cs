using Godot;

public class TurnController
{
    private UnitManager _unitManager;
    private TurnManager _turnManager;

    private bool _endTurnPending = false;
    private ulong _endTurnPendingTime = 0;
    private const ulong EndTurnConfirmWindowMs = 2000;

    public TurnController(UnitManager unitManager, TurnManager turnManager)
    {
        _unitManager = unitManager;
        _turnManager = turnManager;
    }

    public bool TryEndTurn()
    {
        if (_turnManager.CurrentAP <= 0)
        {
            EndTurn();
            return true;
        }

        ulong now = Time.GetTicksMsec();
        if (_endTurnPending && now - _endTurnPendingTime <= EndTurnConfirmWindowMs)
        {
            _endTurnPending = false;
            EndTurn();
            return true;
        }

        _endTurnPending = true;
        _endTurnPendingTime = now;
        return false;
    }

    public void CancelPending() => _endTurnPending = false;

    private void EndTurn()
    {
        foreach (var unit in _unitManager.GetAllUnits())
        {
            unit.OnTurnStart();
            unit.TickStatuses();
        }

        _turnManager.EndTurn();
        _unitManager.UpdateTeamFrames(_turnManager.CurrentTeam);
    }
}
