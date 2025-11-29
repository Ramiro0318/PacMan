using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PelletGenerator : MonoBehaviour
{
    [Header("Configuración Básica")]
    public Tilemap pelletTilemap;
    public Sprite pelletSprite;

    [Header("Pellets Grandes")]
    public Sprite bigPelletSprite;
    public int maxBigPellets = 4;
    public int bigPelletPoints = 50;

    [Header("Sonidos")]
    public AudioClip pelletSound;
    public AudioClip bigPelletSound;
    public AudioClip ghostEatenSound;

    [Header("Sistema de Fantasmas")]
    public float ghostVulnerableTime = 10f;
    public float ghostRespawnTime = 5f;
    public Sprite vulnerableGhostSprite;
    public Sprite deadGhostSprite;

    [Header("Puntuación")]
    public int totalScore = 0;
    public int ghostEatenPoints = 200;

    [Header("Game Over")]
    public string gameOverSceneName = "GameOverScene";

    private AudioSource audioSource;
    private List<Vector3Int> availablePositions = new List<Vector3Int>();
    private List<GameObject> bigPellets = new List<GameObject>();
    private List<GhostController> allGhosts = new List<GhostController>();
    private bool isPowerActive = false;
    private float powerTimer = 0f;

    private SimplePacmanMove pacman;

    // Contadores de pellets
    private int totalPellets = 0;
    private int pelletsEaten = 0;

    // Eventos para UI
    public System.Action<int> OnScoreChanged;
    public System.Action<bool> OnPowerStateChanged;
    public System.Action<float> OnPowerTimeChanged;

    void Start()
    {
        // Obtener o crear AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Encontrar todos los fantasmas INMEDIATAMENTE
        FindAllGhosts();

        Invoke("GeneratePelletsFromTilemap", 0.1f);

        pacman = FindObjectOfType<SimplePacmanMove>();
        if (pacman != null)
        {
            pacman.OnPacmanDeath += OnPacmanDeath;
        }
    }

    void OnPacmanDeath()
    {
        Debug.Log("PelletGenerator: Resetando fantasmas por muerte de Pacman");

        foreach (GhostController ghost in allGhosts)
        {
            if (ghost != null)
            {
                ghost.ResetToSpawn();
            }
        }

        // CORRECCIÓN: Reactivar el power-up si estaba activo
        if (isPowerActive)
        {
            DeactivatePower();
        }
    }

    private void OnDestroy()
    {
        if (pacman != null)
        {
            pacman.OnPacmanDeath -= OnPacmanDeath;
        }
    }

    void Update()
    {
        if (isPowerActive)
        {
            powerTimer -= Time.deltaTime;
            OnPowerTimeChanged?.Invoke(powerTimer);

            if (powerTimer <= 0f)
            {
                DeactivatePower();
            }
        }
    }

    void FindAllGhosts()
    {
        allGhosts.Clear();
        GhostController[] ghosts = FindObjectsOfType<GhostController>();
        allGhosts.AddRange(ghosts);
        Debug.Log($"Encontrados {allGhosts.Count} fantasmas:");
    }

    void GeneratePelletsFromTilemap()
    {
        if (pelletTilemap == null)
        {
            Debug.LogError("Pellet Tilemap no está asignado!");
            return;
        }

        GameObject pelletsContainer = new GameObject("Pellets Container");
        pelletsContainer.transform.SetParent(null);
        pelletsContainer.transform.position = Vector3.zero;

        // Resetear contadores
        totalPellets = 0;
        pelletsEaten = 0;
        availablePositions.Clear();
        bigPellets.Clear();

        // Recolectar todas las posiciones disponibles
        foreach (var position in pelletTilemap.cellBounds.allPositionsWithin)
        {
            if (pelletTilemap.HasTile(position))
            {
                availablePositions.Add(position);
            }
        }

        Debug.Log($"Posiciones disponibles encontradas: {availablePositions.Count}");

        // Generar big pellets primero
        GenerateBigPellets(pelletsContainer.transform);

        // Generar pellets normales
        foreach (Vector3Int position in availablePositions)
        {
            CreateNormalPelletAtPosition(position, pelletsContainer.transform);
            totalPellets++;
        }

        // Deshabilitar el tilemap visual
        if (pelletTilemap.TryGetComponent<TilemapRenderer>(out var renderer))
            renderer.enabled = false;

        if (pelletTilemap.TryGetComponent<TilemapCollider2D>(out var collider))
            collider.enabled = false;

        Debug.Log($"Generados: {totalPellets - bigPellets.Count} pellets normales y {bigPellets.Count} pellets grandes. Total: {totalPellets}");
    }

    void CreateNormalPelletAtPosition(Vector3Int cellPosition, Transform parent)
    {
        Vector3 worldPosition = pelletTilemap.GetCellCenterWorld(cellPosition);

        GameObject pellet = new GameObject("Pellet");
        pellet.transform.position = worldPosition;
        pellet.transform.SetParent(parent);

        SpriteRenderer sr = pellet.AddComponent<SpriteRenderer>();
        sr.sprite = pelletSprite;
        sr.sortingOrder = 1;

        CircleCollider2D col = pellet.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.1f;

        Pellet p = pellet.AddComponent<Pellet>();
        p.points = 10;
        p.isBigPellet = false;
        p.pelletGenerator = this;
    }

    void GenerateBigPellets(Transform parent)
    {
        if (bigPelletSprite == null) return;

        ShufflePositions();

        int pelletsCreated = 0;
        List<Vector3Int> usedPositions = new List<Vector3Int>();
        List<Vector3Int> positionsToRemove = new List<Vector3Int>();

        for (int i = 0; i < availablePositions.Count && pelletsCreated < maxBigPellets; i++)
        {
            Vector3Int currentPos = availablePositions[i];

            if (IsPositionValidForBigPellet(currentPos, usedPositions))
            {
                CreateBigPelletAtPosition(currentPos, parent);
                usedPositions.Add(currentPos);
                positionsToRemove.Add(currentPos);
                pelletsCreated++;
                totalPellets++;
            }
        }

        // Remover las posiciones usadas
        foreach (Vector3Int posToRemove in positionsToRemove)
        {
            availablePositions.Remove(posToRemove);
        }

        Debug.Log($"Big pellets creados: {pelletsCreated}");
    }

    bool IsPositionValidForBigPellet(Vector3Int position, List<Vector3Int> usedPositions)
    {
        foreach (Vector3Int usedPos in usedPositions)
        {
            int distance = Mathf.Abs(position.x - usedPos.x) + Mathf.Abs(position.y - usedPos.y);
            if (distance <= 2)
            {
                return false;
            }
        }
        return true;
    }

    void CreateBigPelletAtPosition(Vector3Int cellPosition, Transform parent)
    {
        Vector3 worldPosition = pelletTilemap.GetCellCenterWorld(cellPosition);

        GameObject bigPellet = new GameObject("BigPellet");
        bigPellet.transform.position = worldPosition;
        bigPellet.transform.SetParent(parent);

        SpriteRenderer sr = bigPellet.AddComponent<SpriteRenderer>();
        sr.sprite = bigPelletSprite;
        sr.sortingOrder = 2;

        CircleCollider2D col = bigPellet.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.15f;

        Pellet p = bigPellet.AddComponent<Pellet>();
        p.points = bigPelletPoints;
        p.isBigPellet = true;
        p.pelletGenerator = this;

        bigPellets.Add(bigPellet);
    }

    void ShufflePositions()
    {
        for (int i = 0; i < availablePositions.Count; i++)
        {
            Vector3Int temp = availablePositions[i];
            int randomIndex = Random.Range(i, availablePositions.Count);
            availablePositions[i] = availablePositions[randomIndex];
            availablePositions[randomIndex] = temp;
        }
    }

    public void OnPelletEaten(bool isBigPellet, int points)
    {
        // Reproducir sonido
        if (audioSource != null)
        {
            AudioClip soundToPlay = isBigPellet ? bigPelletSound : pelletSound;
            if (!audioSource.isPlaying || (audioSource.isPlaying && audioSource.time > 0.6f))
            {
                audioSource.clip = soundToPlay;
                audioSource.Play();
            }
        }

        // Actualizar puntuación
        totalScore += points;
        OnScoreChanged?.Invoke(totalScore);

        pelletsEaten++;

        Debug.Log($"¡Pellet {(isBigPellet ? "grande" : "normal")} comido! +{points} puntos. Puntuación total: {totalScore}");
        Debug.Log($"Pellets comidos: {pelletsEaten}/{totalPellets}");

        // Verificar si se comieron todos los pellets
        CheckAllPelletsEaten();

        // CORRECCIÓN IMPORTANTE: Activar power-up si es pellet grande
        if (isBigPellet)
        {
            ActivatePower();
        }
    }

    void CheckAllPelletsEaten()
    {
        Pellet[] remainingPellets = FindObjectsOfType<Pellet>();
        int actualPelletsCount = remainingPellets.Length;

        if (actualPelletsCount == 0 || pelletsEaten >= totalPellets)
        {
            Debug.Log("¡NIVEL COMPLETADO!");
            GameOver();
        }
    }

    void GameOver()
    {
        Debug.Log("=== GAME OVER ACTIVADO ===");

        SimplePacmanMove pacman = FindObjectOfType<SimplePacmanMove>();
        if (pacman != null)
        {
            Debug.Log("Activando Game Over en Pacman...");
            pacman.GameOver();
        }
        else if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
    }

    void ActivatePower()
    {
        Debug.Log("=== ACTIVANDO POWER-UP ===");

        // CORRECCIÓN: Siempre reiniciar el timer cuando se come un big pellet
        isPowerActive = true;
        powerTimer = ghostVulnerableTime;
        OnPowerStateChanged?.Invoke(true);

        Debug.Log("Nuevo power-up activado");

        // Hacer todos los fantasmas vulnerables
        foreach (GhostController ghost in allGhosts)
        {
          
            if (ghost != null && ghost.IsAlive())
            {
                ghost.SetVulnerable(vulnerableGhostSprite, ghostVulnerableTime);
                Debug.Log($" - {ghost.gameObject.name} hecho vulnerable");
            }
        }

        Debug.Log("¡POWER-UP ACTIVADO! Fantasmas vulnerables por " + ghostVulnerableTime + " segundos");
    }

    void DeactivatePower()
    {
        Debug.Log("=== DESACTIVANDO POWER-UP ===");

        isPowerActive = false;
        powerTimer = 0f;
        OnPowerStateChanged?.Invoke(false);

        foreach (GhostController ghost in allGhosts)
        {
            if (ghost != null)
            {
                ghost.SetNormal();
            }
        }

        Debug.Log("Power-up desactivado");
    }

    public void OnGhostEaten(GhostController ghost)
    {
        if (audioSource != null && ghostEatenSound != null)
        {
            audioSource.PlayOneShot(ghostEatenSound);
        }

        totalScore += ghostEatenPoints;
        OnScoreChanged?.Invoke(totalScore);

        Debug.Log($"¡Fantasma comido! +{ghostEatenPoints} puntos. Puntuación total: {totalScore}");

        ghost.StartRespawn(deadGhostSprite, ghostRespawnTime);
    }

    // CORRECCIÓN: Método para verificar si un fantasma puede moverse
    public bool CanGhostMove(GhostController ghost)
    {
        if (ghost == null) return false;

        GhostController.GhostState state = ghost.GetCurrentState();
        return state != GhostController.GhostState.Respawning && state != GhostController.GhostState.Dead;
    }

    public bool IsPowerActive()
    {
        return isPowerActive;
    }

    public float GetPowerTimeLeft()
    {
        return powerTimer;
    }

    public int GetPelletsEaten()
    {
        return pelletsEaten;
    }

    public int GetTotalPellets()
    {
        return totalPellets;
    }

    public int GetPelletsRemaining()
    {
        return totalPellets - pelletsEaten;
    }

    public void ForceFindGhosts()
    {
        FindAllGhosts();
    }
}