using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public Vector3 Direction = Vector3.right;
    public Character player;

    void Start()
    {
        Destroy(gameObject,5f);
    }

    void Update()
    {
        transform.position += Direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if(collision.TryGetComponent<Character>( out Character enemy))
            {
                player.Attack(enemy);
                Destroy(gameObject);
            }
            
        }
    }

}
