using UnityEngine;

public class GhostController : MonoBehaviour
{
    [Header("Estados del Fantasma")]
    public Sprite normalSprite;
    public Sprite vulnerableSprite;
    public Sprite deadSprite;
    public Vector3 spawnPosition;

    [Header("Componentes")]
    public SpriteRenderer spriteRenderer;
    public Collider2D ghostCollider;
    public Animator ghostAnimator;

    [Header("Configuración")]
    public float vulnerableSpeedMultiplier = 0.5f;
    public float respawnTime = 5f;

    private GhostState currentState = GhostState.Normal;
    private float stateTimer = 0f;
    private MonoBehaviour[] ghostMovementScripts;
    private Vector3 initialPosition;
    private float originalSpeed;
    private GohstMove ghostMove;
    private MirrorGhost mirrorGhost;

    public enum GhostState
    {
        Normal,
        Vulnerable,
        Dead,
        Respawning
    }

    void Start()
    {
        // Obtener componentes
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (ghostCollider == null)
            ghostCollider = GetComponent<Collider2D>();
        if (ghostAnimator == null)
            ghostAnimator = GetComponent<Animator>();

        // Obtener scripts de movimiento
        ghostMove = GetComponent<GohstMove>();
        mirrorGhost = GetComponent<MirrorGhost>();

        // Guardar movimiento original
        ghostMovementScripts = GetComponents<MonoBehaviour>();
        initialPosition = transform.position;
        spawnPosition = initialPosition;

        // Guardar velocidad original
        if (ghostMove != null)
            originalSpeed = ghostMove.moveSpeed;
        else if (mirrorGhost != null)
            originalSpeed = mirrorGhost.moveSpeed;

        SetNormal();
    }

    void Update()
    {
        // Controlar temporizadores de estado
        if (stateTimer > 0f)
        {
            stateTimer -= Time.deltaTime;

            // Parpadeo cuando el power-up está por terminar
            if (currentState == GhostState.Vulnerable && stateTimer <= 3f)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = Mathf.PingPong(Time.time * 5f, 1f) > 0.5f;
                }
            }

            if (stateTimer <= 0f)
            {
                OnStateTimerEnd();
            }
        }
    }

    public void SetVulnerable(Sprite vulnerableSprite, float duration)
    {
        Debug.Log($"SetVulnerable llamado en {gameObject.name}");
        Debug.Log($" - Estado actual: {currentState}");
        Debug.Log($" - Duración: {duration}");

        if (currentState == GhostState.Dead || currentState == GhostState.Respawning)
        {
            Debug.Log($" - {gameObject.name} está muerto o respawneando, ignorando");
            return;
        }

        currentState = GhostState.Vulnerable;
        stateTimer = duration;

        // Cambiar sprite y animación
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            if (vulnerableSprite != null)
            {
                spriteRenderer.sprite = vulnerableSprite;
                Debug.Log($" - Sprite cambiado a vulnerable");
            }
            else
            {
                Debug.LogWarning($" - vulnerableSprite es nulo!");
            }
        }
        else
        {
            Debug.LogWarning($" - spriteRenderer es nulo!");
        }

        // Actualizar animator
        if (ghostAnimator != null)
        {
            ghostAnimator.SetBool("Vulnerable", true);
            ghostAnimator.SetBool("Normal", false);
            ghostAnimator.SetBool("Dead", false);
            Debug.Log($" - Animator actualizado a vulnerable");
        }

        // Reducir velocidad - CORREGIDO: No desactivar los scripts
        if (ghostMove != null)
        {
            ghostMove.moveSpeed = originalSpeed * vulnerableSpeedMultiplier;
            ghostMove.enabled = true; // Asegurar que esté activo
            Debug.Log($" - Velocidad reducida a {ghostMove.moveSpeed}");
        }

        if (mirrorGhost != null)
        {
            mirrorGhost.moveSpeed = originalSpeed * vulnerableSpeedMultiplier;
            mirrorGhost.enabled = true; // Asegurar que esté activo
            Debug.Log($" - Velocidad reducida a {mirrorGhost.moveSpeed}");
        }

        Debug.Log($"{gameObject.name} ahora es vulnerable por {duration} segundos");
    }


    public void SetNormal()
    {
        currentState = GhostState.Normal;
        stateTimer = 0f;

        // Restaurar sprite normal
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            if (normalSprite != null)
            {
                spriteRenderer.sprite = normalSprite;
            }
        }

        // Actualizar animator
        if (ghostAnimator != null)
        {
            ghostAnimator.SetBool("Vulnerable", false);
            ghostAnimator.SetBool("Normal", true);
            ghostAnimator.SetBool("Dead", false);
        }

        // Restaurar velocidad normal
        if (ghostMove != null)
            ghostMove.moveSpeed = originalSpeed;
        if (mirrorGhost != null)
            mirrorGhost.moveSpeed = originalSpeed;

        // Restaurar comportamiento normal
        SetFleeBehavior(false);

        Debug.Log(gameObject.name + " vuelve a la normalidad");
    }

    public void StartRespawn(Sprite deadGhostSprite, float respawnTime)
    {
        currentState = GhostState.Dead;
        stateTimer = 0f;

        // Cambiar a sprite de fantasma muerto
        if (spriteRenderer != null && deadGhostSprite != null)
        {
            spriteRenderer.sprite = deadGhostSprite;
        }

        // Actualizar animator
        if (ghostAnimator != null)
        {
            ghostAnimator.SetBool("Vulnerable", false);
            ghostAnimator.SetBool("Normal", false);
            ghostAnimator.SetBool("Dead", true);
        }

        // Desactivar colisión temporalmente
        if (ghostCollider != null)
        {
            ghostCollider.enabled = false;
        }

        // Detener movimiento
        if (ghostMove != null)
            ghostMove.moveSpeed = 0f;
        if (mirrorGhost != null)
            mirrorGhost.moveSpeed = 0f;

        // Mover al punto de origen
        transform.position = spawnPosition;

        // Iniciar cuenta regresiva para respawn
        currentState = GhostState.Respawning;
        stateTimer = respawnTime;

        Debug.Log(gameObject.name + " murió. Respawn en " + respawnTime + " segundos");
    }

    void OnStateTimerEnd()
    {
        switch (currentState)
        {
            case GhostState.Vulnerable:
                SetNormal();
                break;
            case GhostState.Respawning:
                CompleteRespawn();
                break;
        }
    }

    void CompleteRespawn()
    {
        currentState = GhostState.Normal;

        // Restaurar sprite normal
        if (spriteRenderer != null && normalSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
        }

        // Actualizar animator
        if (ghostAnimator != null)
        {
            ghostAnimator.SetBool("Vulnerable", false);
            ghostAnimator.SetBool("Normal", true);
            ghostAnimator.SetBool("Dead", false);
        }

        // Reactivar colisión
        if (ghostCollider != null)
        {
            ghostCollider.enabled = true;
        }

        // Restaurar velocidad y comportamiento
        if (ghostMove != null)
            ghostMove.moveSpeed = originalSpeed;
        if (mirrorGhost != null)
            mirrorGhost.moveSpeed = originalSpeed;

        SetFleeBehavior(false);

        Debug.Log(gameObject.name + " ha revivido");
    }

    void SetFleeBehavior(bool shouldFlee)
    {
        // En lugar de desactivar scripts, podrías cambiar su comportamiento
        // Por ahora, solo cambiamos la velocidad y el estado
        // Los scripts permanecen activos pero con diferente configuración

        Debug.Log($"SetFleeBehavior: {shouldFlee}");

        // Aquí puedes agregar lógica específica de huida si la necesitas
        // Por ejemplo, cambiar el target de movimiento, etc.
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Fantasma {gameObject.name} colisionó con jugador");
            Debug.Log($" - Estado del fantasma: {currentState}");

            SimplePacmanMove pacman = other.GetComponent<SimplePacmanMove>();
            if (pacman == null || pacman.isDead)
            {
                Debug.Log($" - Pacman nulo o muerto, ignorando");
                return;
            }

            if (currentState == GhostState.Vulnerable)
            {
                Debug.Log($" - Fantasma vulnerable, será comido");

                // Fantasma comido
                PelletGenerator pelletGenerator = FindObjectOfType<PelletGenerator>();
                if (pelletGenerator != null)
                {
                    pelletGenerator.OnGhostEaten(this);
                    Debug.Log($" - Notificado a PelletGenerator");
                }
                else
                {
                    Debug.LogError($" - No se encontró PelletGenerator!");
                }
            }
            else if (currentState == GhostState.Normal)
            {
                Debug.Log($" - Fantasma normal, mata al jugador");
                // Matar al jugador
                pacman.Die();
            }
        }
    }

    public bool IsVulnerable()
    {
        return currentState == GhostState.Vulnerable;
    }

    public bool IsAlive()
    {
        return currentState != GhostState.Dead && currentState != GhostState.Respawning;
    }

    public GhostState GetCurrentState()
    {
        return currentState;
    }

    public float GetStateTimeLeft()
    {
        return stateTimer;
    }
}