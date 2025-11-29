using Unity.VisualScripting;
using UnityEngine;

public abstract class Monster : Character
{
    [Header("Detection Settings (Overlap)")]
    [SerializeField] protected float detectionRadius = 8f;        
    [SerializeField] protected LayerMask playerLayer;
    protected Transform playerTarget;
    protected Rigidbody2D rb;
    protected Vector2 DirectionToGo;

    protected bool playerIsNearby = false;
    protected bool IsCanAttack = true;
    [SerializeField] protected float cooldownTimeAttack = 3f;
    protected float currentTimeCooldown = 0f;


    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        CheckForProximity();

        if (playerIsNearby)
        {
            DirectionToGo = playerTarget.position - this.transform.position;
            transform.Translate(DirectionToGo*speed*Time.deltaTime);
        }
        else
        {
            
        }

        if (!IsCanAttack)
        {
            currentTimeCooldown += Time.deltaTime;
            if (currentTimeCooldown >= cooldownTimeAttack)
            {
                IsCanAttack = true;
                currentTimeCooldown = 0f;
            }
        }

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<Character>(out Character player))
            {
                if (IsCanAttack)
                {
                    Attack(player);
                    IsCanAttack = false;
                }
            }
        }
    }

    public override void Attack(Character target)
    {
        target.TakeDamage(Damage);
    }

    protected virtual void CheckForProximity()
    {
        
        Collider2D hitCollider = Physics2D.OverlapCircle(
            transform.position,     
            detectionRadius,        
            playerLayer             
        );

        if (hitCollider != null)
        {
            playerIsNearby = true;
        
            if (playerTarget == null)
            {
                playerTarget = hitCollider.transform;
            }
        
            Debug.DrawRay(transform.position, Vector2.right * detectionRadius, Color.yellow); 
        }
        else
        {
            playerIsNearby = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }


}
