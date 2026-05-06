using UnityEngine;

public class CombatResolver
{
    public void Resolve(BattleAction action)
    {
        switch (action.Type)
        {
            case ActionType.Attack:
                action.Target.TakeDamage(10);
                break;

            case ActionType.Block:
                ApplyBlock(action.Actor);
                break;

            case ActionType.Heal:
                action.Actor.Heal(5);
                break;
        }
    }

    private void ApplyBlock(Unit unit)
    {
        Debug.Log($"{unit.Id} is blocking");
    }
}