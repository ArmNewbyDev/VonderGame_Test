using UnityEngine;

public class Player : Character
{
    Movement movement;
    [SerializeField] GameObject BulletPrefab;

    protected override void Start()
    {
        base.Start();
        movement = GetComponent<Movement>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Bullet bullet = Instantiate(BulletPrefab, this.transform.position, this.transform.rotation).GetComponent<Bullet>();
            bullet.player = this;
            switch (movement.IsFaceRight)
            {
                case true:
                    bullet.Direction = Vector3.right;
                break;
                case false:
                    bullet.Direction = Vector3.left;
                break;
            }
        }    
    }

    public override void Attack(Character target)
    {
        target.TakeDamage(Damage);
    }
    
}
