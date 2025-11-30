using UnityEngine;

public class ReliableMovingWall : MonoBehaviour
{
    [Header("Configuración Movimiento")]
    public float moveSpeed = 2f;
    public float moveDistance = 3f;
    public bool startMovingUp = true;

    [Header("Detección de Jugador")]
    public float killRange = 0.7f;
    public float checkInterval = 0.1f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool movingToEnd = true;
    private SimplePacmanMove player;
    private float checkTimer;

    void Start()
    {
        startPosition = transform.position;
        endPosition = startMovingUp ?
            startPosition + Vector3.up * moveDistance :
            startPosition + Vector3.down * moveDistance;

        FindPlayer();
    }

    void Update()
    {
        MoveWall();
        CheckPlayerContinuous();
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

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<SimplePacmanMove>();
            Debug.Log($"Jugador encontrado: {player != null}");
        }
    }

    void CheckPlayerContinuous()
    {
        checkTimer -= Time.deltaTime;

        if (checkTimer <= 0f)
        {
            // Si no tenemos jugador, buscarlo
            if (player == null)
            {
                FindPlayer();
                return;
            }

            // Si el jugador está muerto, no hacer nada
            if (player.isDead)
            {
                checkTimer = checkInterval;
                return;
            }

            // Verificar distancia con el jugador
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance < killRange)
            {
                Debug.Log($"💀 Pared mató a Pacman! Distancia: {distance}");
                player.Die();
            }

            checkTimer = checkInterval;
        }
    }

    // También mantener las colisiones por si acaso
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SimplePacmanMove pacman = other.GetComponent<SimplePacmanMove>();
            if (pacman != null && !pacman.isDead)
            {
                Debug.Log("💀 Matado por trigger enter");
                pacman.Die();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SimplePacmanMove pacman = collision.gameObject.GetComponent<SimplePacmanMove>();
            if (pacman != null && !pacman.isDead)
            {
                Debug.Log("💀 Matado por collision enter");
                pacman.Die();
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            startPosition = transform.position;
            endPosition = startMovingUp ?
                startPosition + Vector3.up * moveDistance :
                startPosition + Vector3.down * moveDistance;
        }

        // Ruta de movimiento
        Gizmos.color = Color.red;
        Gizmos.DrawLine(startPosition, endPosition);

        // Puntos de inicio y fin
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(startPosition, Vector3.one * 0.3f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(endPosition, Vector3.one * 0.3f);

        // Rango de muerte
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawSphere(transform.position, killRange);
    }

    void OnGUI()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            //GUI.Label(new Rect(10, 400, 400, 20), $"Distancia a jugador: {distance:F2}");
            //GUI.Label(new Rect(10, 420, 400, 20), $"Jugador muerto: {player.isDead}");
        }
        else
        {
            //GUI.Label(new Rect(10, 400, 400, 20), "Jugador no encontrado");
        }
    }
}