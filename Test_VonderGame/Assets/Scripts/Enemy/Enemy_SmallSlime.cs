using UnityEngine;

public class Enemy_SmallSlime : Monster
{

    protected override void Start()
    {
        base.Start();
    }

    public override void Attack(Character target)
    {
        target.TakeDamage(Damage);
    }

}
