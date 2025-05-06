using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent (typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    private PlayerInputHandler inputHandler => PlayerInputHandler.Instance;


    [Header("Movement Stats")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float jumpForce = 5f;

    private float horizontal;
    private bool isTowardsRight = true;
    private bool isDashing, isJumping;

    [Header("References")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Animator animator;


    void Update()
    {
        if (isDashing) return;

        horizontal = inputHandler.moveInput.x;

        if (inputHandler.jumpTrigger && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
        }

        if (isJumping && IsGrounded() && rb.linearVelocity.y < 0.01f)
        {
            StartCoroutine(EndJump());
        }

        if (inputHandler.jumpTrigger && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.4f);
        }

        Flip();
    }

    private IEnumerator EndJump()
    {
        yield return new WaitForSeconds(1f);
        isJumping = false;
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        float walkSpeed = inputHandler.sprintSpeed > 0f ? speed * sprintMultiplier : speed;
        rb.linearVelocity = new Vector2(horizontal * walkSpeed, rb.linearVelocity.y);
    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        if (isTowardsRight && horizontal < 0f || !isTowardsRight && horizontal > 0f)
        {
            isTowardsRight = !isTowardsRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}
