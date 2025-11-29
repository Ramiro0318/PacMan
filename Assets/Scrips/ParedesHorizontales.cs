using UnityEngine;

public class MovingWall : MonoBehaviour
{
    [Header("Configuración Movimiento")]
    public float moveSpeed = 2f;
    public float moveDistance = 3f;
    public bool startMovingRight = true;

    [Header("Daño")]
    public bool killInstantly = true;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool movingToEnd = true;
    private SimplePacmanMove playerReference;

    void Start()
    {
        startPosition = transform.position;

        // Calcular posición final basado en dirección horizontal
        if (startMovingRight)
        {
            endPosition = startPosition + Vector3.right * moveDistance;
        }
        else
        {
            endPosition = startPosition + Vector3.left * moveDistance;
        }

        // Buscar referencia al jugador una vez
        FindPlayer();
    }

    void Update()
    {
        MoveWall();

        // Si no tenemos referencia al jugador, intentar encontrarlo
        if (playerReference == null)
        {
            FindPlayer();
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerReference = playerObj.GetComponent<SimplePacmanMove>();
        }
    }

    void MoveWall()
    {
        Vector3 targetPosition = movingToEnd ? endPosition : startPosition;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            movingToEnd = !movingToEnd;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerCollision(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerCollision(collision.gameObject);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // También verificar si el jugador está dentro del trigger
        HandlePlayerCollision(other.gameObject);
    }

    void HandlePlayerCollision(GameObject playerObject)
    {
        if (!playerObject.CompareTag("Player")) return;

        // Usar la referencia guardada o obtenerla del objeto
        SimplePacmanMove pacman = playerReference != null ? playerReference : playerObject.GetComponent<SimplePacmanMove>();

        if (pacman == null)
        {
            pacman = playerObject.GetComponent<SimplePacmanMove>();
            if (pacman != null) playerReference = pacman;
        }

        if (pacman != null && !pacman.isDead)
        {
            Debug.Log($"💀 Pared móvil mató a Pacman! {gameObject.name}");
            pacman.Die();
        }
    }

    // Para debugging
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            startPosition = transform.position;
            if (startMovingRight)
            {
                endPosition = startPosition + Vector3.right * moveDistance;
            }
            else
            {
                endPosition = startPosition + Vector3.left * moveDistance;
            }
        }

        // Dibujar ruta de movimiento horizontal
        Gizmos.color = Color.red;
        Gizmos.DrawLine(startPosition, endPosition);

        // Dibujar puntos de inicio y fin
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(startPosition, Vector3.one * 0.3f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(endPosition, Vector3.one * 0.3f);

        // Dibujar dirección actual
        Gizmos.color = Color.yellow;
        Vector3 direction = (endPosition - startPosition).normalized;
        Gizmos.DrawRay(transform.position, direction * 0.5f);

        // Dibujar rango de muerte
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}