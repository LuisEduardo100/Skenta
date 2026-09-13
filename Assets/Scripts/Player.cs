using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Health;

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

    // A regra de vida não mora mais aqui dentro. Ela vem do pacote Game.Health,
    // que é um módulo separado e coberto por testes. Este arquivo virou fachada:
    // os métodos públicos continuam exatamente os mesmos, apenas repassam.
    // Quem já usava Player.TakeDamage, Heal, GetCurrentHealth, LoadHealth
    // ou OnHealthChanged não precisa mudar uma linha.

    // Vida máxima do jogador. Continua sendo a fonte da verdade deste Inspector.
    public int maxHealth = 100;

    private HealthComponent health;

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

        // Liga o módulo de vida. Se o componente não foi colocado pelo Inspector,
        // ele é criado aqui, então nenhuma cena precisa ser ajustada na mão.
        health = GetComponent<HealthComponent>();

        if (health == null)
        {
            health = gameObject.AddComponent<HealthComponent>();
        }

        health.Model.Changed += AoMudarVida;

        // Aplica o maxHealth deste Inspector e começa com a vida cheia.
        health.Restore(new HealthState(maxHealth, maxHealth));
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
    // O clamp, a morte e o disparo dos eventos são responsabilidade do módulo.
    public void TakeDamage(int amount)
    {
        health.TakeDamage(amount);
    }


    // Recupera vida do jogador.
    // Atenção: pelo módulo, quem está morto não recupera vida com Heal.
    // Para trazer de volta existe Revive, que é intenção diferente de curar.
    public void Heal(int amount)
    {
        health.Heal(amount);
    }


    // Traz o jogador de volta com a vida cheia.
    public void Revive()
    {
        health.Revive();
    }


    // Repassa toda mudança de vida para quem escuta.
    private void AoMudarVida(HealthChange mudanca)
    {
        // Quem escutava o Player direto continua recebendo, como antes.
        OnHealthChanged?.Invoke(mudanca.Current, mudanca.Max);

        // E agora a mudança também sai no barramento central do projeto,
        // para quem não conhece o Player.
        EventManager.TriggerHealthChanged(mudanca.Current, mudanca.Max);
    }


    private void OnDestroy()
    {
        if (health != null && health.Model != null)
        {
            health.Model.Changed -= AoMudarVida;
        }
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
        return health.Current;
    }


    // SISTEMA DE CARREGAMENTO
    // Recebe a vida salva e aplica ao jogador.
    public void LoadHealth(int savedHealth)
    {
        // Restore dispara o evento de mudança, então a UI se atualiza sozinha
        // depois de carregar. O sistema de save não precisa avisar ninguém.
        health.Restore(new HealthState(savedHealth, health.Max));
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