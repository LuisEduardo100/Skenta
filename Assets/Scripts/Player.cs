using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 6f;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public float playerScale = 5f;

    private Rigidbody2D rb;
    private Animator animator;

    private float horizontalInput;
    private bool isGrounded;

    private string currentAnimation = "";

    // Evento de vida para notificar a UI ou outros sistemas
    public int maxHealth = 100;
    private int currentHealth;
    public event Action<int, int> OnHealthChanged;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        transform.localScale = new Vector3(
            playerScale,
            playerScale,
            playerScale
        );

        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        horizontalInput = 0f;

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
        {
            horizontalInput = 1f;
        }

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
        {
            horizontalInput = -1f;
        }


        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );
        }

        if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(
                playerScale,
                playerScale,
                playerScale
            );
        }
        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(
                -playerScale,
                playerScale,
                playerScale
            );
        }


        SetAnimation();
        Debug.Log(isGrounded);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            horizontalInput * moveSpeed,
            rb.linearVelocity.y
        );
    }


    void SetAnimation()
    {
        string newAnimation;


        if (!isGrounded)
        {
            if (rb.linearVelocity.y > 0.1f)
            {
                newAnimation = "jump";
            }
            else
            {
                newAnimation = "fall";
            }
        }

        else
        {
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                newAnimation = "walk";
            }
            else
            {
                newAnimation = "idle";
            }
        }


        if (currentAnimation != newAnimation)
        {
            animator.Play(newAnimation);
            currentAnimation = newAnimation;
        }
    }

    // Parte de Vida: Gerencia o dano e dispara o evento para a UI
    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // Parte de Vida: Gerencia a cura e dispara o evento para a UI
    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // Parte de Inventario: Chamado ao consumir um item do inventario (ex: pocao)
    public void UseItem(string itemName)
    {
        if (itemName == "Potion")
        {
            Heal(20);
        }
    }

    // Parte de Salvamento: Retorna os dados do jogador para serem salvos
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // Parte de Salvamento: Carrega e aplica os dados de vida salvos
    public void LoadHealth(int savedHealth)
    {
        currentHealth = savedHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }
    }
}