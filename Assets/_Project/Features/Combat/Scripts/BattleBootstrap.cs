using System.Collections.Generic;
using UnityEngine;

public class BattleBootstrap : MonoBehaviour
{
    private BattleManager battleManager;
    private float timer;

    void Start()
    {
        Debug.Log("Bootstrap started");

        var player = new Unit("Player", 100);
        var enemy = new Unit("Enemy", 100);

        var playerTeam = new List<Unit> { player };
        var enemyTeam = new List<Unit> { enemy };

        var turnSystem = new TurnSystem(playerTeam, enemyTeam);
        var resolver = new CombatResolver();
        var ai = new SimpleAI();

        var controller = new PlayerActionController();
        BattleContext.PlayerController = controller;


        battleManager = new BattleManager(turnSystem, resolver, ai, controller);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            battleManager?.Update();
            timer = 0f;
        }
    }
}