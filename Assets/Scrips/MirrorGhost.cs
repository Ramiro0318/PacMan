using UnityEngine;
using System.Collections.Generic;

public class MirrorGhost : MonoBehaviour
{
    [Header("Configuración del Espejo")]
    public float moveSpeed = 1f;
    public float collisionCheckDistance = 0.09f;
    public LayerMask wallLayer = LayerMask.GetMask("Wall");

    [Header("Comportamiento Espejo")]
    public bool mirrorX = true;
    public bool mirrorY = false;
    public float mirrorDelay = 0.3f;

    private Transform player;
    private SimplePacmanMove playerMovement;
    private Animator animator;

    // Réplica del sistema de movimiento del jugador
    private Vector2 movementInput = Vector2.zero;
    private Vector2 lastDirection = Vector2.right;
    private Vector2 playerLastDirection = Vector2.right;
    private Vector3 lastPlayerPosition;
    private float lastInputTime;
    private bool isMoving = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerMovement = player.GetComponent<SimplePacmanMove>();
        animator = GetComponent<Animator>();

        lastPlayerPosition = player.position;

        if (wallLayer == 0)
            wallLayer = LayerMask.GetMask("Default");
    }

    void Update()
    {
        if (player == null || playerMovement == null || playerMovement.isDead)
        {
            movementInput = Vector2.zero;
            return;
        }

        MirrorPlayerMovement();
        TryMove();
        UpdateAnimations();
    }

    void MirrorPlayerMovement()
    {
        // Obtener la última dirección del jugador directamente desde su script
        Vector2 playerCurrentDirection = GetPlayerCurrentDirection();

        // Aplicar espejo a la dirección
        Vector2 mirroredDirection = playerCurrentDirection;

        if (mirrorX) mirroredDirection.x = -mirroredDirection.x;
        if (mirrorY) mirroredDirection.y = -mirroredDirection.y;

        // Solo actualizar si el jugador realmente cambió de dirección
        if (playerCurrentDirection.magnitude > 0.1f && playerCurrentDirection != playerLastDirection)
        {
            playerLastDirection = playerCurrentDirection;
            lastInputTime = Time.time;

            // Verificar si la dirección espejada es válida
            if (IsValidDirection(mirroredDirection))
            {
                lastDirection = mirroredDirection;
                movementInput = mirroredDirection;
                isMoving = true;
            }
            else
            {
                // Si no es válida, buscar alternativa
                FindAlternativeDirection(mirroredDirection);
            }
        }
        else if (playerCurrentDirection.magnitude < 0.1f && isMoving)
        {
            // Si el jugador se detiene
            movementInput = Vector2.zero;
            isMoving = false;
        }

        lastPlayerPosition = player.position;
    }

    Vector2 GetPlayerCurrentDirection()
    {
        // Método 1: Acceder directamente a las variables del jugador (si son públicas)
        // return playerMovement.lastDirection;

        // Método 2: Calcular basado en cambio de posición (más confiable)
        Vector3 positionDiff = player.position - lastPlayerPosition;

        if (positionDiff.magnitude > 0.01f)
        {
            return new Vector2(positionDiff.x, positionDiff.y).normalized;
        }

        // Método 3: Usar reflexión para acceder a variables privadas (último recurso)
        return GetPlayerLastDirectionViaReflection();
    }

    Vector2 GetPlayerLastDirectionViaReflection()
    {
        try
        {
            System.Reflection.FieldInfo field = typeof(SimplePacmanMove).GetField("lastDirection",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                return (Vector2)field.GetValue(playerMovement);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("No se pudo acceder a lastDirection: " + e.Message);
        }

        return Vector2.zero;
    }

    void FindAlternativeDirection(Vector2 desiredDirection)
    {
        // Buscar la dirección válida más cercana a la deseada
        Vector2 bestDirection = Vector2.zero;
        float bestDot = -Mathf.Infinity;

        Vector2[] testDirections = {
            desiredDirection,
            new Vector2(desiredDirection.x, 0),
            new Vector2(0, desiredDirection.y),
            new Vector2(-desiredDirection.x, 0),
            new Vector2(0, -desiredDirection.y)
        };

        foreach (Vector2 direction in testDirections)
        {
            if (direction.magnitude > 0.1f && IsValidDirection(direction))
            {
                float dot = Vector2.Dot(desiredDirection.normalized, direction.normalized);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestDirection = direction;
                }
            }
        }

        if (bestDirection.magnitude > 0.1f)
        {
            lastDirection = bestDirection;
            movementInput = bestDirection;
        }
        else
        {
            movementInput = Vector2.zero;
        }
    }

    void TryMove()
    {
        if (movementInput.magnitude > 0.1f)
        {
            Vector2 moveDirection = movementInput.normalized;
            float distance = moveSpeed * Time.deltaTime + collisionCheckDistance;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, distance, wallLayer);

            Debug.DrawRay(transform.position, moveDirection * distance, hit.collider ? Color.red : Color.green);

            if (hit.collider == null)
            {
                transform.Translate(new Vector3(movementInput.x, movementInput.y, 0) * moveSpeed * Time.deltaTime);
            }
            else
            {
                // Si hay colisión, buscar nueva dirección
                movementInput = Vector2.zero;
                FindAlternativeDirection(lastDirection);
            }
        }
    }

    void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetInteger("DireccionX", (int)movementInput.x);
            animator.SetInteger("DireccionY", (int)movementInput.y);
            animator.SetBool("Muerto", false);
        }
    }

    bool IsValidDirection(Vector2 direction)
    {
        if (direction.magnitude < 0.1f) return false;

        float distance = moveSpeed * Time.deltaTime + collisionCheckDistance;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, distance, wallLayer);

        return hit.collider == null;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SimplePacmanMove pacman = other.GetComponent<SimplePacmanMove>();
            if (pacman != null && !pacman.isDead)
            {
                Debug.Log("Fantasma espejo atrapó al jugador!");
                pacman.Die();
            }
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 90, 300, 20), $"Fantasma - Movimiento: {movementInput}");
        GUI.Label(new Rect(10, 110, 300, 20), $"Fantasma - Última Dir: {lastDirection}");
    }
}