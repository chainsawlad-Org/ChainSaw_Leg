using System.Collections.Generic;

public class TurnSystem
{
    private readonly List<Unit> _playerTeam;
    private readonly List<Unit> _enemyTeam;
    private int _turnIndex;

    public TurnSystem(List<Unit> playerTeam, List<Unit> enemyTeam)
    {
        _playerTeam = playerTeam;
        _enemyTeam = enemyTeam;
    }

    public Unit GetCurrentUnit()
    {
        // пока 1х1
        return _turnIndex % 2 == 0 ? _playerTeam[0] : _enemyTeam[0];
    }

    public Unit GetTarget(Unit actor)
    {
        return _playerTeam.Contains(actor)
            ? _enemyTeam[0]
            : _playerTeam[0];
    }

    public void NextTurn()
    {
        _turnIndex++;
    }
}