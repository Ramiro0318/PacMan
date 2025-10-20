using UnityEngine;
using System.Collections.Generic;

public class ImprovedGhostMove : MonoBehaviour
{
    [Header("Configuración")]
    public float moveSpeed = 2f;
    public LayerMask wallLayer;
    public float directionChangeTime = 3f;
    public float raycastDistance = 0.5f;

    [Header("Debug")]
    public bool showDebugRays = true;

    private Vector2 currentDirection;
    private float timer;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Forzar inicialización del layer mask si es 0
        if (wallLayer.value == 0)
        {
            wallLayer = LayerMask.GetMask("Wall");
            Debug.Log("Wall layer set to: " + wallLayer.value);
        }

        ChooseRandomDirection();
        timer = directionChangeTime;

        Debug.Log("Ghost started. Wall layer: " + wallLayer);
    }

    void Update()
    {
        timer -= Time.deltaTime;

        // Cambiar dirección periódicamente
        if (timer <= 0f)
        {
            ChooseRandomDirection();
            timer = directionChangeTime;
        }

        // Verificar si choca con pared
        if (IsPathBlocked())
        {
            Debug.Log("Path blocked! Changing direction");
            ChooseRandomDirection();
            timer = directionChangeTime;
        }

        // Mover
        Move();
    }

    void ChooseRandomDirection()
    {
        Vector2[] allDirections = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        List<Vector2> validDirections = new List<Vector2>();

        Debug.Log("Choosing direction. Testing all directions:");

        // Probar todas las direcciones
        foreach (Vector2 dir in allDirections)
        {
            bool isBlocked = IsDirectionBlocked(dir);
            Debug.Log($"Direction {dir} - Blocked: {isBlocked}");

            if (!isBlocked)
            {
                validDirections.Add(dir);
            }
        }

        // Elegir aleatoriamente entre direcciones válidas
        if (validDirections.Count > 0)
        {
            currentDirection = validDirections[Random.Range(0, validDirections.Count)];
            Debug.Log($"Chose valid direction: {currentDirection}. Valid options: {validDirections.Count}");
        }
        else
        {
            // Si todas están bloqueadas, retroceder
            currentDirection = -currentDirection;
            Debug.Log($"All directions blocked! Reversing to: {currentDirection}");
        }
    }

    bool IsDirectionBlocked(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            (Vector2)transform.position + direction * 0.1f, // Pequeño offset desde el centro
            direction,
            raycastDistance,
            wallLayer
        );

        // Debug visual
        if (showDebugRays)
        {
            Debug.DrawRay(transform.position, direction * raycastDistance,
                         hit.collider != null ? Color.red : Color.green, 1f);
        }

        return hit.collider != null;
    }

    bool IsPathBlocked()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            (Vector2)transform.position + currentDirection * 0.1f,
            currentDirection,
            0.3f,
            wallLayer
        );

        if (showDebugRays)
        {
            Debug.DrawRay(transform.position, currentDirection * 0.3f,
                         hit.collider != null ? Color.magenta : Color.blue, 1f);
        }

        return hit.collider != null;
    }

    void Move()
    {
        if (rb != null)
        {
            rb.linearVelocity = currentDirection * moveSpeed;
        }
        else
        {
            transform.Translate(currentDirection * moveSpeed * Time.deltaTime);
        }
    }

    void OnGUI()
    {
        // Mostrar información de debug en pantalla
        GUI.Label(new Rect(10, 130, 400, 20), $"Ghost Direction: {currentDirection}");
        GUI.Label(new Rect(10, 150, 400, 20), $"Timer: {timer:F2}");
        GUI.Label(new Rect(10, 170, 400, 20), $"Wall Layer: {wallLayer.value}");
        GUI.Label(new Rect(10, 190, 400, 20), $"Position: {transform.position}");
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Dibujar información en el editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, currentDirection * 0.5f);
    }
}