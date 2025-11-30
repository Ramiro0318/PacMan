using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TeleportGhost : MonoBehaviour
{
    [Header("Configuración Teletransporte")]
    public float minTimeBetweenTeleports = 3f;
    public float maxTimeBetweenTeleports = 8f;

    [Header("Configuración Movimiento")]
    public float moveSpeed = 1.5f;
    public LayerMask wallLayer;

    [Header("Efectos Visuales")]
    public GameObject teleportEffect;

    private float teleportTimer;
    private Animator animator;
    private List<Vector3> pelletPositions = new List<Vector3>();
    private bool isTeleporting = false;
    private Vector2 currentDirection;
    private PelletGenerator pelletGenerator;

    void Start()
    {
        animator = GetComponent<Animator>();
        pelletGenerator = FindObjectOfType<PelletGenerator>();

        // Configurar timer rápido para primer teletransporte
        teleportTimer = 1f;
        currentDirection = Vector2.zero; // Empezar quieto

        if (wallLayer == 0)
            wallLayer = LayerMask.GetMask("Wall");

        Debug.Log($"{gameObject.name} iniciado");

        // Forzar primer teletransporte
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

        teleportTimer -= Time.deltaTime;

        // Moverse muy poco y solo ocasionalmente
        if (currentDirection != Vector2.zero)
        {
            if (IsPathBlocked())
            {
                currentDirection = Vector2.zero;
            }
            else
            {
                transform.Translate(currentDirection * moveSpeed * Time.deltaTime);
            }
        }

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

        // Ocasionalmente cambiar dirección
        if (Random.Range(0f, 1f) < 0.02f) // 2% de probabilidad por frame
        {
            ChooseRandomDirection();
        }
    }

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

        // Encontrar cualquier pellet válido (más permisivo)
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
        // Verificar que no haya paredes (más permisivo)
        Collider2D wallHit = Physics2D.OverlapCircle(position, 0.1f, wallLayer); // Radio más pequeño
        if (wallHit != null)
        {
            return false;
        }

        // Verificar que no esté en la misma posición que otro fantasma
        TeleportGhost[] otherGhosts = FindObjectsOfType<TeleportGhost>();
        foreach (TeleportGhost ghost in otherGhosts)
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
        currentDirection = Vector2.zero;

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

        // Quedarse quieto después del teletransporte
        currentDirection = Vector2.zero;
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

    void ChooseRandomDirection()
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        currentDirection = directions[Random.Range(0, directions.Length)];
    }

    bool IsPathBlocked()
    {
        if (currentDirection == Vector2.zero) return true;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            currentDirection,
            0.3f,
            wallLayer
        );
        return hit.collider != null;
    }

    // Para debugging
    void OnGUI()
    {
        int yOffset = 600;
        //GUI.Label(new Rect(10, yOffset, 500, 20), $"TELEPORT GHOST - Estado: {(isTeleporting ? "🔮 TELETRANSPORTÁNDOSE" : "👻 QUIETO")}");
        //GUI.Label(new Rect(10, yOffset + 20, 500, 20), $"Pellets: {pelletPositions.Count} - Siguiente TP: {teleportTimer:F1}s");
        //GUI.Label(new Rect(10, yOffset + 40, 500, 20), $"Movimiento: {currentDirection}");
        //GUI.Label(new Rect(10, yOffset + 60, 500, 20), $"Posición actual: {transform.position}");
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
    }
}