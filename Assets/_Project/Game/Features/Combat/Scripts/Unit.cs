using System;

public sealed class Unit
{
    public string Id { get; }
    public int MaxHP { get; }
    public int CurrentHP { get; private set; }

    public bool IsAlive => CurrentHP > 0;

    public event Action<int, int> HpChanged;

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

        HpChanged?.Invoke(CurrentHP, MaxHP);
    }

    public void Heal(int amount)
    {
        CurrentHP += amount;
        CurrentHP = Math.Min(CurrentHP, MaxHP);

        HpChanged?.Invoke(CurrentHP, MaxHP);
    }
}
