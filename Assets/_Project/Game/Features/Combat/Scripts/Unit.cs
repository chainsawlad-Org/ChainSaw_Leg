using System;
using UnityEngine;

public class Unit
{
    public string Id;
    public int MaxHP;
    public int CurrentHP;

    public bool IsAlive => CurrentHP > 0;

    public event Action<int, int> OnHPChanged;

    public Unit(string id, int hp)
    {
        Id = id;
        MaxHP = hp;
        CurrentHP = hp;
    }

    public void TakeDamage(int amount)
    {
        CurrentHP -= amount;
        CurrentHP = Math.Max(CurrentHP, 0);

        OnHPChanged?.Invoke(CurrentHP, MaxHP);
    }

    public void Heal(int amount)
    {
        CurrentHP += amount;
        CurrentHP = Math.Min(CurrentHP, MaxHP);

        OnHPChanged?.Invoke(CurrentHP, MaxHP);
    }
}
