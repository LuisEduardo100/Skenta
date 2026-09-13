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


    // SISTEMA DE VIDA

    // Vida máxima e vida atual do jogador.
    public int maxHealth = 100;
    private int currentHealth;

    // Evento que avisa outros sistemas quando a vida muda.
    // Envia: vida atual e vida máxima.
    public event Action<int, int> OnHealthChanged;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Define o tamanho inicial do jogador.
        transform.localScale = new Vector3(
            playerScale,
            playerScale,
            playerScale
        );

        // Inicializa a vida.
        currentHealth = maxHealth;

        // Avisa a UI que a vida foi inicializada.
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


    // SISTEMA DE VIDA

    // Causa dano ao jogador.
    public void TakeDamage(int amount)
    {
        // Impede a vida de ficar abaixo de 0.
        currentHealth = Mathf.Clamp(
            currentHealth - amount,
            0,
            maxHealth
        );

        // Avisa a UI e outros sistemas que a vida mudou.
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }


    // Recupera vida do jogador.
    public void Heal(int amount)
    {
        // Impede a vida de ultrapassar o máximo.
        currentHealth = Mathf.Clamp(
            currentHealth + amount,
            0,
            maxHealth
        );

        // Avisa a UI que a vida mudou.
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }


    // SISTEMA DE INVENTÁRIO

    // Método chamado quando o jogador utiliza um item.
    public void UseItem(string itemName)
    {
        // Se o item for uma poção, recupera 20 de vida.
        if (itemName == "Potion")
        {
            Heal(20);
        }
    }

    // SISTEMA DE SALVAMENTO

    // Retorna a vida atual para que o SaveSystem
    // possa armazená-la.
    public int GetCurrentHealth()
    {
        return currentHealth;
    }


    // SISTEMA DE CARREGAMENTO
    // Recebe a vida salva e aplica ao jogador.
    public void LoadHealth(int savedHealth)
    {
        currentHealth = savedHealth;

        // Avisa a UI que a vida foi alterada.
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