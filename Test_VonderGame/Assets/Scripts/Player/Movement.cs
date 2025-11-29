using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 7f;
    Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    IInteraction interactionObjectTemp;
    public bool IsFaceRight {get; private set;} = true;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        Vector3 movement = new Vector3(horizontalInput, 0f, 0f);

        transform.position += movement * speed * Time.deltaTime;

        if(Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        }

        if (Input.GetKeyDown(KeyCode.E))
            {
                if(interactionObjectTemp != null)
                    interactionObjectTemp.Interact();
            }

        if (horizontalInput < 0)
        {
            spriteRenderer.flipX = true;
            IsFaceRight = false;
        }
        else if (horizontalInput > 0)
        {
            spriteRenderer.flipX = false;
            IsFaceRight = true;
        }

        if (movement.magnitude > 0)
        {
            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }
        
       
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Interactable"))
        {
            interactionObjectTemp = collision.gameObject.GetComponent<IInteraction>();
        }

    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Interactable"))
        {
            interactionObjectTemp = null;
        }

    }

}
