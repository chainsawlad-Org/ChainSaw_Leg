using UnityEngine;

public class CombatResolver
{
    public void Resolve(BattleAction action)
    {
        switch (action.Type)
        {
            case ActionType.Attack:
                DealDamage(action.Actor, action.Target);
                break;

            case ActionType.Block:
                ApplyBlock(action.Actor);
                break;

            case ActionType.Heal:
                Heal(action.Actor);
                break;
        }
    }

    private void DealDamage(Unit attacker, Unit target)
    {
        int damage = 10;
        target.CurrentHP -= damage;
        Debug.Log($"{attacker.Id} hits {target.Id} for {damage}");
    }

    private void ApplyBlock(Unit unit)
    {
        Debug.Log($"{unit.Id} is blocking");
    }

    private void Heal(Unit unit)
    {
        int heal = 5;
        unit.CurrentHP = Mathf.Min(unit.CurrentHP + heal, unit.MaxHP);
        Debug.Log($"{unit.Id} heals {heal}");
    }
}