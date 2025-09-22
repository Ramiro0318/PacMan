using UnityEngine;
using System.Collections.Generic;

public class GohstMove : MonoBehaviour
{
    public float moveSpeed = 3f;
    public LayerMask wallLayer;
    public string playerTag = "Player";
    public string pelletTag = "Pellets";

    // Configuración de IA
    public float directionChangeInterval = 1.5f;
    public float randomDirectionChance = 0.2f;
    public int maxStuckChecks = 5;

    private Vector2 currentDirection;
    private Vector2[] possibleDirections = {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };

    private Rigidbody2D rb;
    private bool isChasing = true;
    private Transform player;
    private float lastDirectionChangeTime;
    private Vector3 lastPosition;
    private float stuckTimer;
    private int consecutiveHorizontalMoves = 0;
    private int consecutiveVerticalMoves = 0;
    private Vector2 lastValidDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag(playerTag).transform;

        ChooseRandomDirection();
        lastDirectionChangeTime = Time.time;
        lastPosition = transform.position;
        lastValidDirection = currentDirection;

        if (wallLayer == 0)
            wallLayer = LayerMask.GetMask("Default");
    }

    void Update()
    {
        // Si el jugador está muerto, dejar de perseguir
        SimplePacmanMove pacman = player?.GetComponent<SimplePacmanMove>();
        if (pacman != null && pacman.isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        CheckIfStuck();

        if (isChasing)
        {
            ChasePlayer();
        }

        MoveGhost();
        CheckForDirectionChange();
    }

    void CheckIfStuck()
    {
        // Verificar si está atascado (no se mueve)
        if (Vector3.Distance(transform.position, lastPosition) < 0.01f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 1f) // Atascado por más de 1 segundo
            {
                Debug.Log("Fantasma atascado, forzando cambio de dirección");
                ForceDirectionChange();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = transform.position;

        // Prevenir ciclos horizontales/verticales excesivos
        if (IsHorizontalDirection(currentDirection))
        {
            consecutiveHorizontalMoves++;
            consecutiveVerticalMoves = 0;
        }
        else
        {
            consecutiveVerticalMoves++;
            consecutiveHorizontalMoves = 0;
        }

        // Forzar cambio de dirección si hay demasiados movimientos en la misma orientación
        if (consecutiveHorizontalMoves > 3 || consecutiveVerticalMoves > 3)
        {
            ForceDirectionChange();
        }
    }

    void MoveGhost()
    {
        rb.linearVelocity = currentDirection * moveSpeed;
    }

    void ChasePlayer()
    {
        if (player == null) return;

        // Cambio de dirección por intervalo o aleatorio
        if (Time.time - lastDirectionChangeTime > directionChangeInterval ||
            Random.value < randomDirectionChance)
        {
            ChooseSmartDirection();
            lastDirectionChangeTime = Time.time;
            return;
        }

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        Vector2 bestDirection = currentDirection;
        float bestScore = -Mathf.Infinity;

        // Obtener todas las direcciones válidas
        List<Vector2> validDirections = GetValidDirections();

        if (validDirections.Count == 0)
        {
            ForceDirectionChange();
            return;
        }

        // Evaluar cada dirección válida
        foreach (Vector2 direction in validDirections)
        {
            float score = CalculateDirectionScore(direction, directionToPlayer, validDirections.Count);

            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = direction;
            }
        }

        currentDirection = bestDirection;
        lastValidDirection = bestDirection;
    }

    List<Vector2> GetValidDirections()
    {
        List<Vector2> validDirections = new List<Vector2>();

        foreach (Vector2 direction in possibleDirections)
        {
            if (IsValidDirection(direction) && !IsOppositeDirection(direction, currentDirection))
            {
                validDirections.Add(direction);
            }
        }

        // Si no hay direcciones válidas, permitir opuestas como último recurso
        if (validDirections.Count == 0)
        {
            foreach (Vector2 direction in possibleDirections)
            {
                if (IsValidDirection(direction))
                {
                    validDirections.Add(direction);
                }
            }
        }

        return validDirections;
    }

    float CalculateDirectionScore(Vector2 direction, Vector2 directionToPlayer, int validDirectionCount)
    {
        float score = 0f;

        // Puntuación por alineación con el jugador (35%)
        score += Vector2.Dot(direction, directionToPlayer) * 0.35f;

        // Puntuación por distancia al jugador (25%)
        Vector2 projectedPosition = (Vector2)transform.position + direction;
        float distanceToPlayer = Vector2.Distance(projectedPosition, player.position);
        score += (1f / (distanceToPlayer + 0.1f)) * 0.25f;

        // Penalizar cambios de dirección opuestos (15%)
        if (IsOppositeDirection(direction, lastValidDirection))
        {
            score -= 0.15f;
        }

        // Favorecer cambio de orientación si hay muchos movimientos en la misma (15%)
        if (consecutiveHorizontalMoves > 2 && !IsHorizontalDirection(direction))
        {
            score += 0.15f;
        }
        else if (consecutiveVerticalMoves > 2 && IsHorizontalDirection(direction))
        {
            score += 0.15f;
        }

        // Aleatoriedad (10%)
        score += Random.Range(0f, 0.1f);

        return score;
    }

    void ChooseSmartDirection()
    {
        if (player == null) return;

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        List<Vector2> validDirections = GetValidDirections();

        if (validDirections.Count > 0)
        {
            Vector2 bestDirection = validDirections[0];
            float bestScore = CalculateDirectionScore(validDirections[0], directionToPlayer, validDirections.Count);

            for (int i = 1; i < validDirections.Count; i++)
            {
                float score = CalculateDirectionScore(validDirections[i], directionToPlayer, validDirections.Count);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = validDirections[i];
                }
            }
            currentDirection = bestDirection;
        }
        else
        {
            ChooseRandomDirection();
        }
    }

    void ForceDirectionChange()
    {
        Debug.Log("Forzando cambio de dirección");

        List<Vector2> validDirections = new List<Vector2>();

        // Priorizar direcciones que cambien la orientación actual
        foreach (Vector2 direction in possibleDirections)
        {
            if (IsValidDirection(direction))
            {
                if ((IsHorizontalDirection(currentDirection) && !IsHorizontalDirection(direction)) ||
                    (!IsHorizontalDirection(currentDirection) && IsHorizontalDirection(direction)))
                {
                    validDirections.Add(direction);
                }
            }
        }

        if (validDirections.Count > 0)
        {
            currentDirection = validDirections[Random.Range(0, validDirections.Count)];
        }
        else
        {
            ChooseRandomDirection();
        }

        consecutiveHorizontalMoves = 0;
        consecutiveVerticalMoves = 0;
        lastDirectionChangeTime = Time.time;
    }

    bool IsValidDirection(Vector2 direction)
    {
        float rayLength = 0.8f; // Aumentado para mejor detección
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction,
            rayLength,
            wallLayer
        );

        return hit.collider == null;
    }

    bool IsOppositeDirection(Vector2 dir1, Vector2 dir2)
    {
        return Vector2.Dot(dir1, dir2) < -0.9f;
    }

    bool IsHorizontalDirection(Vector2 direction)
    {
        return Mathf.Abs(direction.x) > Mathf.Abs(direction.y);
    }

    void ChooseRandomDirection()
    {
        List<Vector2> validDirections = new List<Vector2>();

        foreach (Vector2 direction in possibleDirections)
        {
            if (IsValidDirection(direction))
            {
                validDirections.Add(direction);
            }
        }

        if (validDirections.Count > 0)
        {
            currentDirection = validDirections[Random.Range(0, validDirections.Count)];
        }
    }

    void CheckForDirectionChange()
    {
        // Verificar obstáculos más cercanos
        float obstacleCheckDistance = 0.4f;
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            currentDirection,
            obstacleCheckDistance,
            wallLayer
        );

        if (hit.collider != null)
        {
            ChooseSmartDirection();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            SimplePacmanMove pacman = other.GetComponent<SimplePacmanMove>();
            if (pacman != null && !pacman.isDead)
            {
                Debug.Log("Fantasma atrapó al jugador!");
                pacman.Die();
            }
        }

        if (other.CompareTag(pelletTag))
        {
            Physics2D.IgnoreCollision(other, GetComponent<Collider2D>());
        }
    }

    void OnDrawGizmos()
    {
        // Dirección actual
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, currentDirection * 1f);

        // Detección de obstáculos
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, currentDirection * 0.4f);

        // Rayos en todas las direcciones para debug
        Gizmos.color = Color.blue;
        foreach (Vector2 dir in possibleDirections)
        {
            Gizmos.DrawRay(transform.position, dir * 0.3f);
        }
    }
}