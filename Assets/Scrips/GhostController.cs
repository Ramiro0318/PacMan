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

    private GhostState currentState = GhostState.Normal;
    private float stateTimer = 0f;
    private MonoBehaviour ghostMovementScript;
    private Vector3 initialPosition;

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

        // Guardar movimiento original
        ghostMovementScript = GetComponent<MonoBehaviour>();
        initialPosition = transform.position;
        spawnPosition = initialPosition;

        SetNormal();
    }

    void Update()
    {
        // Controlar temporizadores de estado
        if (stateTimer > 0f)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                OnStateTimerEnd();
            }
        }
    }

    public void SetVulnerable(Sprite vulnerableSprite, float duration)
    {
        if (currentState == GhostState.Dead || currentState == GhostState.Respawning)
            return;

        currentState = GhostState.Vulnerable;
        stateTimer = duration;

        // Cambiar sprite
        if (spriteRenderer != null && vulnerableSprite != null)
        {
            spriteRenderer.sprite = vulnerableSprite;
        }

        // Desactivar comportamiento de persecución
        if (ghostMovementScript != null)
        {
            ghostMovementScript.enabled = false;
        }

        // Aquí podrías agregar lógica de huida
        Debug.Log(gameObject.name + " ahora es vulnerable");
    }

    public void SetNormal()
    {
        if (currentState == GhostState.Dead || currentState == GhostState.Respawning)
            return;

        currentState = GhostState.Normal;
        stateTimer = 0f;

        // Restaurar sprite normal
        if (spriteRenderer != null && normalSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
        }

        // Reactivar comportamiento normal
        if (ghostMovementScript != null)
        {
            ghostMovementScript.enabled = true;
        }

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

        // Desactivar colisión temporalmente
        if (ghostCollider != null)
        {
            ghostCollider.enabled = false;
        }

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

        // Reactivar colisión
        if (ghostCollider != null)
        {
            ghostCollider.enabled = true;
        }

        // Reactivar comportamiento
        if (ghostMovementScript != null)
        {
            ghostMovementScript.enabled = true;
        }

        Debug.Log(gameObject.name + " ha revivido");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentState == GhostState.Vulnerable)
            {
                // Fantasma comido
                PelletGenerator pelletGenerator = FindObjectOfType<PelletGenerator>();
                if (pelletGenerator != null)
                {
                    pelletGenerator.OnGhostEaten(this);
                }
            }
            else if (currentState == GhostState.Normal)
            {
                // Matar al jugador
                SimplePacmanMove pacman = other.GetComponent<SimplePacmanMove>();
                if (pacman != null && !pacman.isDead)
                {
                    pacman.Die();
                }
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
}