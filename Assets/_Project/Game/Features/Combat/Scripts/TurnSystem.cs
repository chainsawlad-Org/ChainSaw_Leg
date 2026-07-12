using System.Collections.Generic;

public class TurnSystem
{
    private readonly List<Unit> playerTeam;
    private readonly List<Unit> enemyTeam;
    private int turnIndex;

    public TurnSystem(List<Unit> playerTeam, List<Unit> enemyTeam)
    {
        this.playerTeam = playerTeam;
        this.enemyTeam = enemyTeam;
    }

    public Unit GetCurrentUnit()
    {
        // пока 1х1
        return turnIndex % 2 == 0 ? playerTeam[0] : enemyTeam[0];
    }

    public Unit GetTarget(Unit actor)
    {
        return playerTeam.Contains(actor) ? enemyTeam[0] : playerTeam[0];
    }

    public void NextTurn()
    {
        turnIndex++;
    }

    public bool IsBattleOver()
    {
        return !playerTeam[0].IsAlive || enemyTeam[0].IsAlive;
    }
}