using UnityEngine;
using System.Collections.Generic;

public class AutoPacmanCollision : MonoBehaviour
{
    public enum RouteMode
    {
        Predefined,     // Ruta fija que tú defines
        CollisionBased  // Cambia dirección al chocar
    }

    [Header("Modo de Ruta")]
    public RouteMode routeMode = RouteMode.Predefined;

    [Header("Ruta Predefinida")]
    public List<Vector2> directions = new List<Vector2>() {
        Vector2.up, Vector2.right, Vector2.down, Vector2.left
    };
    public float moveSpeed = 2f;
    public float changeTime = 3f; // Cambiar después de tiempo si no choca

    [Header("Configuración")]
    public LayerMask wallLayer;
    public float collisionCheckDistance = 0.1f;

    private int currentDirectionIndex = 0;
    private Vector2 currentDirection;
    private float timer;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (directions.Count > 0)
        {
            currentDirection = directions[0];
        }
        else
        {
            // Direcciones por defecto: arriba, derecha, abajo, izquierda
            directions = new List<Vector2>() {
                Vector2.up, Vector2.right, Vector2.down, Vector2.left
            };
            currentDirection = Vector2.up;
        }

        timer = changeTime;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (routeMode == RouteMode.CollisionBased)
        {
            MoveWithCollisionDetection();
        }
        else
        {
            MovePredefined();
        }

        UpdateAnimations();
    }

    void MovePredefined()
    {
        // Mover en la dirección actual
        transform.Translate(currentDirection * moveSpeed * Time.deltaTime);

        // Cambiar dirección después de tiempo
        if (timer <= 0f)
        {
            NextDirection();
            timer = changeTime;
        }
    }

    void MoveWithCollisionDetection()
    {
        // Verificar colisión
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            currentDirection,
            collisionCheckDistance,
            wallLayer
        );

        Debug.DrawRay(transform.position, currentDirection * collisionCheckDistance,
                     hit.collider ? Color.red : Color.green);

        if (hit.collider != null)
        {
            // Cambiar dirección al chocar
            NextDirection();
            Debug.Log($"Chocó con {hit.collider.name}. Nueva dirección: {currentDirection}");
        }
        else
        {
            // Moverse normalmente
            transform.Translate(currentDirection * moveSpeed * Time.deltaTime);
        }
    }

    void NextDirection()
    {
        currentDirectionIndex++;
        if (currentDirectionIndex >= directions.Count)
        {
            currentDirectionIndex = 0;
        }

        currentDirection = directions[currentDirectionIndex];
    }

    void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetInteger("DireccionX", Mathf.RoundToInt(currentDirection.x));
            animator.SetInteger("DireccionY", Mathf.RoundToInt(currentDirection.y));
        }
    }

    // Métodos para configurar la ruta en tiempo de ejecución
    public void SetRoute(List<Vector2> newDirections)
    {
        directions = newDirections;
        currentDirectionIndex = 0;
        if (directions.Count > 0)
        {
            currentDirection = directions[0];
        }
    }

    public void SetSimpleRoute()
    {
        // Ruta simple: arriba → derecha → abajo → izquierda
        directions = new List<Vector2>() {
            Vector2.up, Vector2.right, Vector2.down, Vector2.left
        };
        currentDirectionIndex = 0;
        currentDirection = directions[0];
    }

    public void SetCustomRoute(Vector2[] customDirections)
    {
        directions = new List<Vector2>(customDirections);
        currentDirectionIndex = 0;
        currentDirection = directions[0];
    }

    void OnGUI()
    {
        int yOffset = 370;
        GUI.Label(new Rect(10, yOffset, 400, 20), $"AutoPacman - Modo: {routeMode}");
        GUI.Label(new Rect(10, yOffset + 20, 400, 20), $"Dirección: {currentDirection} ({currentDirectionIndex + 1}/{directions.Count})");
        GUI.Label(new Rect(10, yOffset + 40, 400, 20), $"Timer: {timer:F1}");
    }
}