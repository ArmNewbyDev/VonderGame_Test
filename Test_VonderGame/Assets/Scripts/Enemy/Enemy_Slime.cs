using UnityEngine;

public class Enemy_Slime : Monster
{

    public GameObject SmallSlime;
    public float ForceToLunchSmallSlime = 20f;


    protected override void Start()
    {
        base.Start();
    }


    public override void Attack(Character target)
    {
        target.TakeDamage(Damage);
    }

    public override void Dead()
    {
        Rigidbody2D slime1 = Instantiate(SmallSlime, this.transform.position + new Vector3(0.5f,1,0), this.transform.rotation).GetComponent<Rigidbody2D>();
        Rigidbody2D slime2 = Instantiate(SmallSlime, this.transform.position + new Vector3(-0.5f,1,0), this.transform.rotation).GetComponent<Rigidbody2D>();

        slime1.AddForce(new Vector2(1f,1.5f) * ForceToLunchSmallSlime);
        slime2.AddForce(new Vector2(-1f,1.5f) * ForceToLunchSmallSlime);

        base.Dead();
    }

}
