using System;
using UnityEngine;

[Serializable]
public abstract class Character : MonoBehaviour
{
    [SerializeField] protected int MaxLifePoint = 100;
    [SerializeField] protected int MaxArcanePoint = 100;
    [SerializeField] protected int Damage = 5;
    [SerializeField] protected float speed = 5f;
    

    public int CurrentLifePoint{get; private set;}
    public int CurrentArcanePoint{get; private set;}

    protected virtual void Start()
    {
        CurrentLifePoint = MaxLifePoint;
        CurrentArcanePoint = MaxArcanePoint;
    }

    public abstract void Attack(Character target);


    public virtual void TakeDamage(int LP)
    {
        Debug.Log("Take " + LP + " Damage.");
        CurrentLifePoint -= LP;
        if (CurrentLifePoint <= 0)
        {
            Dead();
        }
        Debug.Log("LP : " + CurrentLifePoint + " left.");
    }
    public virtual void HealLifePoint(int LP)
    {
        CurrentLifePoint += LP;
    }
    public virtual void UseArcanePoint(int AP)
    {
        CurrentArcanePoint -= AP;
    }
    public virtual void RegainArcanePoint(int AP)
    {
        CurrentArcanePoint += AP;
    }

    public virtual void Dead()
    {
        Destroy(gameObject);
    }
}
