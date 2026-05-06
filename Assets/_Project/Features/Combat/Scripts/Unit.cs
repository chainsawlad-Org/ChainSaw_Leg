using UnityEngine;

public class Unit
{
    public string Id;
    public int MaxHP;
    public int CurrentHP;

    public bool IsAlive => CurrentHP > 0;

    public Unit(string id, int hp)
    {
        Id = id;
        MaxHP = hp;
        CurrentHP = hp;
    }
}
