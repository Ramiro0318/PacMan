using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MirrorTeleportGhost : MonoBehaviour
{
    [Header("Configuración del Espejo")]
    public float moveSpeed = 1f;
    public float collisionCheckDistance = 0.09f;
    public LayerMask wallLayer = LayerMask.GetMask();

    [Header("Comportamiento Espejo")]
    public bool mirrorX = true;
    public bool mirrorY = false;
    public float mirrorDelay = 0.3f;

    [Header("Configuración Teletransporte")]
    public float minTimeBetweenTeleports = 3f;
    public float maxTimeBetweenTeleports = 8f;

    [Header("Efectos Visuales")]
    public GameObject teleportEffect;

    private Transform player;
    private SimplePacmanMove playerMovement;
    private Animator animator;

    // Variables del sistema de espejo
    private Vector2 movementInput = Vector2.zero;
    private Vector2 lastDirection = Vector2.right;
    private Vector2 playerLastDirection = Vector2.right;
    private Vector3 lastPlayerPosition;
    private float lastInputTime;
    private bool isMoving = false;

    // Variables del teletransporte
    private float teleportTimer;
    private List<Vector3> pelletPositions = new List<Vector3>();
    private bool isTeleporting = false;
    private PelletGenerator pelletGenerator;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerMovement = player.GetComponent<SimplePacmanMove>();
        animator = GetComponent<Animator>();
        pelletGenerator = FindObjectOfType<PelletGenerator>();

        lastPlayerPosition = player.position;

        if (wallLayer == 0)
            wallLayer = LayerMask.GetMask("Default");

        // Configurar timer para teletransporte
        teleportTimer = Random.Range(minTimeBetweenTeleports, maxTimeBetweenTeleports);

        Debug.Log($"{gameObject.name} iniciado - Modo Espejo + Teletransporte");

        // Forzar primer teletransporte después de un delay
        Invoke("ForceFirstTeleport", 1f);
    }

    void ForceFirstTeleport()
    {
        UpdatePelletPositions();
        if (pelletPositions.Count > 0)
        {
            Debug.Log("Forzando primer teletransporte...");
            TryTeleport();
        }
    }

    void Update()
    {
        if (isTeleporting) return;

        // Actualizar timer de teletransporte
        teleportTimer -= Time.deltaTime;

        // Comportamiento normal de espejo cuando no se está teletransportando
        if (player == null || playerMovement == null || playerMovement.isDead)
        {
            movementInput = Vector2.zero;
            return;
        }

        MirrorPlayerMovement();
        TryMove();
        UpdateAnimations();

        // Verificar teletransporte
        if (teleportTimer <= 0f)
        {
            UpdatePelletPositions();

            if (pelletPositions.Count > 0)
            {
                TryTeleport();
            }
            teleportTimer = Random.Range(minTimeBetweenTeleports, maxTimeBetweenTeleports);
        }
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
        // Método 1: Calcular basado en cambio de posición (más confiable)
        Vector3 positionDiff = player.position - lastPlayerPosition;

        if (positionDiff.magnitude > 0.01f)
        {
            return new Vector2(positionDiff.x, positionDiff.y).normalized;
        }

        // Método 2: Usar reflexión para acceder a variables privadas (último recurso)
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

    // ========== SISTEMA DE TELETRANSPORTE ==========

    void UpdatePelletPositions()
    {
        pelletPositions.Clear();

        // Buscar todos los pellets activos
        Pellet[] allPellets = FindObjectsOfType<Pellet>();
        foreach (Pellet pellet in allPellets)
        {
            if (pellet != null && pellet.gameObject.activeInHierarchy)
            {
                pelletPositions.Add(pellet.transform.position);
            }
        }

        Debug.Log($"Pellets encontrados: {pelletPositions.Count}");
    }

    void TryTeleport()
    {
        if (pelletPositions.Count == 0)
        {
            Debug.LogWarning("No hay pellets disponibles");
            return;
        }

        // Encontrar cualquier pellet válido
        Vector3 teleportPosition = FindAnyValidPelletPosition();

        if (teleportPosition != Vector3.zero)
        {
            StartTeleport(teleportPosition);
        }
        else
        {
            Debug.LogError("NO SE PUDO ENCONTRAR NINGUNA POSICIÓN VÁLIDA");
            // Fallback: teletransportarse a un pellet aleatorio sin verificar
            Vector3 randomPellet = pelletPositions[Random.Range(0, pelletPositions.Count)];
            StartTeleport(randomPellet);
        }
    }

    Vector3 FindAnyValidPelletPosition()
    {
        if (pelletPositions.Count == 0) return Vector3.zero;

        // Mezclar pellets
        List<Vector3> shuffledPellets = new List<Vector3>(pelletPositions);
        ShuffleList(shuffledPellets);

        // Intentar posiciones exactas de pellets
        foreach (Vector3 pelletPos in shuffledPellets)
        {
            if (IsPositionValid(pelletPos))
            {
                Debug.Log($"✅ POSICIÓN VÁLIDA ENCONTRADA: {pelletPos}");
                return pelletPos;
            }
        }

        // Intentar posiciones cercanas (más permisivo)
        foreach (Vector3 pelletPos in shuffledPellets)
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * 2f;
                Vector3 testPosition = pelletPos + new Vector3(randomOffset.x, randomOffset.y, 0);

                if (IsPositionValid(testPosition))
                {
                    Debug.Log($"✅ POSICIÓN CERCANA VÁLIDA: {testPosition}");
                    return testPosition;
                }
            }
        }

        return Vector3.zero;
    }

    bool IsPositionValid(Vector3 position)
    {
        // Verificar que no haya paredes
        Collider2D wallHit = Physics2D.OverlapCircle(position, 0.1f, wallLayer);
        if (wallHit != null)
        {
            return false;
        }

        // Verificar que no esté en la misma posición que otro fantasma
        MirrorTeleportGhost[] otherGhosts = FindObjectsOfType<MirrorTeleportGhost>();
        foreach (MirrorTeleportGhost ghost in otherGhosts)
        {
            if (ghost != null && ghost.gameObject != this.gameObject)
            {
                if (Vector3.Distance(position, ghost.transform.position) < 0.5f)
                {
                    return false;
                }
            }
        }

        // Verificar que no esté demasiado cerca de Pacman
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && Vector3.Distance(position, player.transform.position) < 1f)
        {
            return false;
        }

        return true;
    }

    void StartTeleport(Vector3 targetPosition)
    {
        isTeleporting = true;
        movementInput = Vector2.zero; // Detener movimiento durante teletransporte

        Debug.Log($"🔮 INICIANDO TELETRANSPORTE a: {targetPosition}");

        // Efecto visual de salida
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, transform.position, Quaternion.identity);
        }

        // Teletransportarse inmediatamente
        ExecuteTeleport(targetPosition);
    }

    void ExecuteTeleport(Vector3 targetPosition)
    {
        // Realizar teletransporte
        transform.position = targetPosition;

        Debug.Log($"✅✅✅ TELETRANSPORTE COMPLETADO a: {transform.position}");

        // Efecto visual de entrada
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, transform.position, Quaternion.identity);
        }

        isTeleporting = false;

        // Reiniciar variables de movimiento después del teletransporte
        movementInput = Vector2.zero;
        lastPlayerPosition = player.position; // Actualizar referencia de posición del jugador
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SimplePacmanMove pacman = other.GetComponent<SimplePacmanMove>();
            if (pacman != null && !pacman.isDead)
            {
                Debug.Log("Fantasma espejo-teletransporte atrapó al jugador!");
                pacman.Die();
            }
        }
    }

    void OnGUI()
    {
        int yOffset = 90;
        GUI.Label(new Rect(10, yOffset, 400, 20), $"Fantasma Espejo-TP - Movimiento: {movementInput}");
        GUI.Label(new Rect(10, yOffset + 20, 400, 20), $"Última Dir: {lastDirection}");
        GUI.Label(new Rect(10, yOffset + 40, 400, 20), $"Estado: {(isTeleporting ? "🔮 TELETRANSPORTÁNDOSE" : "👻 ACTIVO")}");
        GUI.Label(new Rect(10, yOffset + 60, 400, 20), $"Pellets: {pelletPositions.Count} - Siguiente TP: {teleportTimer:F1}s");
        GUI.Label(new Rect(10, yOffset + 80, 400, 20), $"Posición actual: {transform.position}");
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Dibujar pellets disponibles
        Gizmos.color = Color.yellow;
        foreach (Vector3 pelletPos in pelletPositions)
        {
            Gizmos.DrawWireSphere(pelletPos, 0.2f);
        }

        // Indicador de estado
        Gizmos.color = isTeleporting ? Color.magenta : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        // Dirección de movimiento actual
        if (movementInput.magnitude > 0.1f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, movementInput.normalized * 0.5f);
        }
    }
}