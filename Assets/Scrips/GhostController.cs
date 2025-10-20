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

    [Header("Configuración de Animaciones")]
    public float vulnerableFlashInterval = 0.2f;

    private GhostState currentState = GhostState.Normal;
    private float stateTimer = 0f;
    private MonoBehaviour[] ghostMovementScripts;
    private Vector3 initialPosition;
    private float flashTimer = 0f;
    private bool isFlashing = false;

    // NUEVO: Para detección manual
    private float collisionCheckTimer = 0f;
    private float collisionCheckInterval = 0.1f;

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

        ghostMovementScripts = GetComponents<MonoBehaviour>();
        initialPosition = transform.position;
        spawnPosition = initialPosition;

        // NUEVO: Configuración forzada de collider
        if (ghostCollider != null)
        {
            ghostCollider.isTrigger = true; // FORZAR a trigger
        }

        SetNormal();
    }

    void Update()
    {
        if (stateTimer > 0f)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                OnStateTimerEnd();
            }
        }

        // Manejar animación de vulnerable
        if (currentState == GhostState.Vulnerable)
        {
            HandleVulnerableAnimation();

            // NUEVO: Detección manual de colisión
            collisionCheckTimer -= Time.deltaTime;
            if (collisionCheckTimer <= 0f)
            {
                ManualCollisionCheck();
                collisionCheckTimer = collisionCheckInterval;
            }
        }
    }

    // NUEVO: Detección manual de colisión con Pacman
    void ManualCollisionCheck()
    {
        if (currentState != GhostState.Vulnerable) return;

        // Buscar todos los objetos con tag Player
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player == null) continue;

            // Calcular distancia
            float distance = Vector2.Distance(transform.position, player.transform.position);

            // Radio de detección (ajustable)
            float detectionRadius = 0.25f;

            if (distance < detectionRadius)
            {
                Debug.Log($"🎯 COLISIÓN MANUAL DETECTADA - Distancia: {distance}");
                Debug.Log($"   - Fantasma: {gameObject.name}, Estado: {currentState}");
                Debug.Log($"   - Pacman: {player.name}");

                // Ejecutar la lógica de colisión
                ExecutePacmanCollision(player);
                return; // Solo procesar una colisión por frame
            }
        }
    }

    // NUEVO: Ejecutar lógica de colisión
    void ExecutePacmanCollision(GameObject pacmanObject)
    {
        if (currentState == GhostState.Vulnerable)
        {
            Debug.Log($"✅ FANTASMA COMIBLE - {gameObject.name} fue comido!");

            // Notificar al PelletGenerator
            PelletGenerator pelletGenerator = FindObjectOfType<PelletGenerator>();
            if (pelletGenerator != null)
            {
                pelletGenerator.OnGhostEaten(this);
            }

            // Iniciar respawn
            StartRespawn(deadSprite, 5f);
        }
        else if (currentState == GhostState.Normal)
        {
            Debug.Log($"❌ FANTASMA NORMAL - {gameObject.name} mata a Pacman");

            // Matar a Pacman
            SimplePacmanMove pacman = pacmanObject.GetComponent<SimplePacmanMove>();
            if (pacman != null && !pacman.isDead)
            {
                pacman.Die();
            }
        }
    }

    void HandleVulnerableAnimation()
    {
        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0f)
        {
            isFlashing = !isFlashing;
            flashTimer = vulnerableFlashInterval;

            if (isFlashing)
            {
                SetSpriteManually(vulnerableSprite);
            }
            else
            {
                SetSpriteManually(normalSprite);
            }

            spriteRenderer.color = isFlashing ? Color.white : new Color(0.3f, 0.3f, 1f, 1f);
        }

        if (stateTimer < 3f)
        {
            vulnerableFlashInterval = 0.1f;
        }
    }

    void SetSpriteManually(Sprite sprite)
    {
        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    public void SetVulnerable(Sprite vulnerableSprite, float duration)
    {
        if (currentState == GhostState.Dead || currentState == GhostState.Respawning)
            return;

        Debug.Log($"🔵 SetVulnerable en {gameObject.name}");

        currentState = GhostState.Vulnerable;
        stateTimer = duration;
        flashTimer = vulnerableFlashInterval;
        isFlashing = true;

        // Desactivar Animator para control manual
        if (ghostAnimator != null)
        {
            ghostAnimator.enabled = false;
        }

        // Forzar sprite vulnerable
        SetSpriteManually(vulnerableSprite);
        spriteRenderer.color = new Color(0.3f, 0.3f, 1f, 1f);

        // Asegurar que el collider sea trigger
        if (ghostCollider != null)
        {
            ghostCollider.isTrigger = true;
        }

        // Desactivar movimiento
        if (ghostMovementScripts != null)
        {
            foreach (var script in ghostMovementScripts)
            {
                if (script != null && script != this && script.enabled)
                {
                    if (script.GetType().Name.Contains("Movement"))
                    {
                        script.enabled = false;
                    }
                }
            }
        }
    }

    // MANTENER el OnTriggerEnter2D original también



public void SetNormal()
    {
        if (currentState == GhostState.Dead || currentState == GhostState.Respawning)
            return;

        Debug.Log($"SetNormal llamado en {gameObject.name}");

        currentState = GhostState.Normal;
        stateTimer = 0f;

        // NUEVO: Reactivar Animator
        if (ghostAnimator != null)
        {
            ghostAnimator.enabled = true;
        }

        // Restaurar sprite normal
        SetSpriteManually(normalSprite);
        spriteRenderer.color = Color.white;

        // NUEVO: Restaurar layer original
        gameObject.layer = LayerMask.NameToLayer("Ghost");

        // Reactivar scripts de movimiento
        if (ghostMovementScripts != null)
        {
            foreach (var script in ghostMovementScripts)
            {
                if (script != null && script != this)
                {
                    script.enabled = true;
                }
            }
        }

        Debug.Log($"{gameObject.name} vuelve a NORMAL. Layer: {LayerMask.LayerToName(gameObject.layer)}");
    }

    public void StartRespawn(Sprite deadGhostSprite, float respawnTime)
    {
        Debug.Log($"StartRespawn llamado en {gameObject.name}");

        currentState = GhostState.Dead;
        stateTimer = 0f;

        // NUEVO: Desactivar Animator para control manual
        if (ghostAnimator != null)
        {
            ghostAnimator.enabled = false;
        }

        // Cambiar a sprite de fantasma muerto
        SetSpriteManually(deadSprite != null ? deadSprite : deadGhostSprite);
        spriteRenderer.color = Color.white;

        // NUEVO: Cambiar layer para evitar colisiones
        gameObject.layer = LayerMask.NameToLayer("Default");

        // Desactivar colisión
        if (ghostCollider != null)
        {
            ghostCollider.enabled = false;
        }

        // Desactivar movimiento
        if (ghostMovementScripts != null)
        {
            foreach (var script in ghostMovementScripts)
            {
                if (script != null && script != this && script.enabled)
                {
                    script.enabled = false;
                }
            }
        }

        // Mover al spawn
        transform.position = spawnPosition;

        // Iniciar respawn
        currentState = GhostState.Respawning;
        stateTimer = respawnTime;

        Debug.Log($"{gameObject.name} en RESPAWN. Posición: {spawnPosition}");
    }

    void CompleteRespawn()
    {
        Debug.Log($"CompleteRespawn llamado en {gameObject.name}");

        currentState = GhostState.Normal;

        // NUEVO: Reactivar Animator
        if (ghostAnimator != null)
        {
            ghostAnimator.enabled = true;
        }

        // Restaurar sprite normal
        SetSpriteManually(normalSprite);
        spriteRenderer.color = Color.white;

        // NUEVO: Restaurar layer original
        gameObject.layer = LayerMask.NameToLayer("Ghost");

        // Reactivar colisión
        if (ghostCollider != null)
        {
            ghostCollider.enabled = true;
        }

        // Reactivar movimiento
        if (ghostMovementScripts != null)
        {
            foreach (var script in ghostMovementScripts)
            {
                if (script != null && script != this)
                {
                    script.enabled = true;
                }
            }
        }

        Debug.Log($"{gameObject.name} ha revivido completamente");
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

    public void ResetToSpawn()
    {
        Debug.Log($"🔄 Resetando {gameObject.name} a spawn position");

        // Detener cualquier temporizador
        stateTimer = 0f;

        // Forzar estado normal
        currentState = GhostState.Normal;

        // Reactivar Animator si estaba desactivado
        if (ghostAnimator != null)
        {
            ghostAnimator.enabled = true;
        }

        // Restaurar sprite normal
        if (spriteRenderer != null && normalSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
            spriteRenderer.color = Color.white;
        }

        // Reactivar colisión
        if (ghostCollider != null)
        {
            ghostCollider.enabled = true;
        }

        // Mover a posición de spawn
        transform.position = spawnPosition;

        // Reactivar todos los scripts de movimiento
        if (ghostMovementScripts != null)
        {
            foreach (var script in ghostMovementScripts)
            {
                if (script != null && script != this)
                {
                    script.enabled = true;
                }
            }
        }

        // Restaurar layer original si es necesario
        gameObject.layer = LayerMask.NameToLayer("Ghost");

        Debug.Log($"✅ {gameObject.name} resetado completamente");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"OnTriggerEnter2D en {gameObject.name}. Estado: {currentState}. Con: {other.gameObject.name}");

        if (other.CompareTag("Player"))
        {
            if (currentState == GhostState.Vulnerable)
            {
                Debug.Log($"Fantasma vulnerable {gameObject.name} fue comido por Pacman");

                // Fantasma comido
                PelletGenerator pelletGenerator = FindObjectOfType<PelletGenerator>();
                if (pelletGenerator != null)
                {
                    pelletGenerator.OnGhostEaten(this);
                }

                StartRespawn(deadSprite, 5f); // Usar 5 segundos como respawn time
            }
            else if (currentState == GhostState.Normal)
            {
                Debug.Log($"Fantasma normal {gameObject.name} mata a Pacman");

                // Matar al jugador
                SimplePacmanMove pacman = other.GetComponent<SimplePacmanMove>();
                if (pacman != null && !pacman.isDead)
                {
                    pacman.Die();
                }
            }
        }
    }

    // NUEVO: Para debugging en el Editor
 

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
}