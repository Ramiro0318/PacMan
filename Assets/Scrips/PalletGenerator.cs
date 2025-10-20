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

        // Temporal: Verificar pellets con tecla P (para debugging)
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            CheckPelletsManually();
        }
    }

    void FindAllGhosts()
    {
        allGhosts.Clear(); // Limpiar lista primero
        GhostController[] ghosts = FindObjectsOfType<GhostController>();
        allGhosts.AddRange(ghosts);
        Debug.Log($"Encontrados {allGhosts.Count} fantasmas:");

        foreach (GhostController ghost in allGhosts)
        {
            Debug.Log($" - {ghost.gameObject.name} en estado: {ghost.GetCurrentState()}");
        }
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

        // Primero recolectar todas las posiciones disponibles
        foreach (var position in pelletTilemap.cellBounds.allPositionsWithin)
        {
            if (pelletTilemap.HasTile(position))
            {
                availablePositions.Add(position);
            }
        }

        Debug.Log($"Posiciones disponibles encontradas: {availablePositions.Count}");

        // Generar big pellets primero (para evitar superposición)
        GenerateBigPellets(pelletsContainer.transform);

        // Luego generar pellets normales en las posiciones restantes
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

    public void CheckPelletsManually()
    {
        // Buscar todos los pellets en la escena
        Pellet[] allPelletsInScene = FindObjectsOfType<Pellet>();
        int actualPelletsCount = allPelletsInScene.Length;

        Debug.Log($"VERIFICACIÓN MANUAL:");
        Debug.Log($"- Pellets en escena: {actualPelletsCount}");
        Debug.Log($"- Contador interno: {pelletsEaten}/{totalPellets}");
        Debug.Log($"- Diferencia: {actualPelletsCount - (totalPellets - pelletsEaten)}");

        if (actualPelletsCount == 0 && pelletsEaten < totalPellets)
        {
            Debug.LogError("¡HAY UNA DISCREPANCIA EN EL CONTEO! Forzando game over...");
            GameOver();
        }
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

            // Verificar que no esté cerca de otro big pellet
            if (IsPositionValidForBigPellet(currentPos, usedPositions))
            {
                if (Random.Range(0f, 1f) < 0.3f)
                {
                    CreateBigPelletAtPosition(currentPos, parent);
                    usedPositions.Add(currentPos);
                    positionsToRemove.Add(currentPos); // Marcar para remover
                    pelletsCreated++;
                    totalPellets++; // Contar cada big pellet una sola vez
                }
            }
        }

        // Si no se crearon suficientes, crear en posiciones válidas
        while (pelletsCreated < maxBigPellets)
        {
            bool createdNew = false;
            for (int i = 0; i < availablePositions.Count && pelletsCreated < maxBigPellets; i++)
            {
                Vector3Int currentPos = availablePositions[i];
                if (!usedPositions.Contains(currentPos) && IsPositionValidForBigPellet(currentPos, usedPositions))
                {
                    CreateBigPelletAtPosition(currentPos, parent);
                    usedPositions.Add(currentPos);
                    positionsToRemove.Add(currentPos); // Marcar para remover
                    pelletsCreated++;
                    totalPellets++; // Contar cada big pellet una sola vez
                    createdNew = true;
                    break;
                }
            }

            // Si no se pudo crear ningún pellet nuevo, salir del loop
            if (!createdNew) break;
        }

        // Remover las posiciones usadas para big pellets de availablePositions
        foreach (Vector3Int posToRemove in positionsToRemove)
        {
            availablePositions.Remove(posToRemove);
        }

        Debug.Log($"Big pellets creados: {pelletsCreated}");
    }

    bool IsPositionValidForBigPellet(Vector3Int position, List<Vector3Int> usedPositions)
    {
        // Verificar que no haya otro big pellet en posiciones adyacentes
        foreach (Vector3Int usedPos in usedPositions)
        {
            // Calcular distancia Manhattan (más eficiente para grid)
            int distance = Mathf.Abs(position.x - usedPos.x) + Mathf.Abs(position.y - usedPos.y);
            if (distance <= 2) // Distancia mínima de 2 celdas
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
            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
            }
        }

        // Actualizar puntuación
        totalScore += points;

        // Notificar cambio de puntuación
        OnScoreChanged?.Invoke(totalScore);

        pelletsEaten++;

        Debug.Log($"¡Pellet {(isBigPellet ? "grande" : "normal")} comido! +{points} puntos. Puntuación total: {totalScore}");
        Debug.Log($"Pellets comidos: {pelletsEaten}/{totalPellets}");

        // Verificar si se comieron todos los pellets
        CheckAllPelletsEaten();

        // Activar power-up si es pellet grande
        if (isBigPellet)
        {
            ActivatePower();
        }
    }


    void CheckAllPelletsEaten()
    {
        Debug.Log($"=== VERIFICANDO PELLETS ===");
        Debug.Log($"Contador interno: {pelletsEaten}/{totalPellets}");

        // Verificación MANUAL de todos los pellets en escena
        Pellet[] remainingPellets = FindObjectsOfType<Pellet>();
        int actualPelletsCount = remainingPellets.Length;

        Debug.Log($"Pellets reales en escena: {actualPelletsCount}");
        Debug.Log($"Big pellets en lista: {bigPellets.Count}");

        // Verificar si realmente no quedan pellets
        if (actualPelletsCount == 0)
        {
            Debug.Log("¡CONFIRMADO: No quedan pellets en la escena!");
            Debug.Log("¡NIVEL COMPLETADO!");
            GameOver();
        }
        else if (pelletsEaten >= totalPellets)
        {
            Debug.Log("Contador interno indica nivel completado");
            Debug.Log($"Pero aún hay {actualPelletsCount} pellets en escena");
            Debug.Log("Forzando game over por contador interno...");
            GameOver();
        }
        else
        {
            Debug.Log($"Quedan {actualPelletsCount} pellets por comer");
            Debug.Log($"Contador interno: {pelletsEaten}/{totalPellets}");
        }
    }


    void GameOver()
    {
        Debug.Log("=== GAME OVER ACTIVADO ===");

        // Buscar el script de Pacman y activar su game over
        SimplePacmanMove pacman = FindObjectOfType<SimplePacmanMove>();
        if (pacman != null)
        {
            Debug.Log("Activando Game Over en Pacman...");
            pacman.GameOver();
        }
        else
        {
            Debug.LogError("No se encontró SimplePacmanMove en la escena!");

            // Fallback: cargar escena de game over
            if (!string.IsNullOrEmpty(gameOverSceneName))
            {
                Debug.Log($"Cargando escena: {gameOverSceneName}");
                SceneManager.LoadScene(gameOverSceneName);
            }
        }
    }

    void ActivatePower()
    {
        // DEBUG: Verificar qué está pasando
        Debug.Log("=== ACTIVANDO POWER-UP ===");
        Debug.Log($"isPowerActive: {isPowerActive}");
        Debug.Log($"Número de fantasmas: {allGhosts.Count}");

        // Si ya hay un power-up activo, reiniciar el timer
        if (isPowerActive)
        {
            powerTimer = ghostVulnerableTime;
            Debug.Log("Power-up reiniciado");
        }
        else
        {
            isPowerActive = true;
            powerTimer = ghostVulnerableTime;
            OnPowerStateChanged?.Invoke(true);

            Debug.Log("Nuevo power-up activado");

            // Hacer todos los fantasmas vulnerables
            foreach (GhostController ghost in allGhosts)
            {
                if (ghost != null)
                {
                    Debug.Log($"Procesando fantasma: {ghost.gameObject.name}");
                    Debug.Log($" - Está vivo: {ghost.IsAlive()}");
                    Debug.Log($" - Estado actual: {ghost.GetCurrentState()}");

                    if (ghost.IsAlive())
                    {
                        ghost.SetVulnerable(vulnerableGhostSprite, ghostVulnerableTime);
                        Debug.Log($" - {ghost.gameObject.name} hecho vulnerable");
                    }
                    else
                    {
                        Debug.Log($" - {ghost.gameObject.name} no está vivo, ignorando");
                    }
                }
                else
                {
                    Debug.LogWarning("Fantasma nulo encontrado en la lista");
                }
            }
        }

        Debug.Log("¡POWER-UP ACTIVADO! Fantasmas vulnerables por " + ghostVulnerableTime + " segundos");
    }


    void DeactivatePower()
    {
        isPowerActive = false;
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

        // Notificar cambio de puntuación
        OnScoreChanged?.Invoke(totalScore);

        Debug.Log($"¡Fantasma comido! +{ghostEatenPoints} puntos. Puntuación total: {totalScore}");

        ghost.StartRespawn(deadGhostSprite, ghostRespawnTime);
    }

    public bool IsPowerActive()
    {
        return isPowerActive;
    }

    public float GetPowerTimeLeft()
    {
        return powerTimer;
    }

    // Métodos para obtener información de pellets (útil para UI)
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
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 20), $"Pellets: {pelletsEaten}/{totalPellets} - En escena: {FindObjectsOfType<Pellet>().Length}");
        GUI.Label(new Rect(10, 30, 400, 20), $"Power Active: {isPowerActive} - Time: {powerTimer:F1}");
        GUI.Label(new Rect(10, 50, 400, 20), $"Fantasmas: {allGhosts.Count} - Vulnerables: {CountVulnerableGhosts()}");

        if (GUI.Button(new Rect(10, 80, 200, 30), "Forzar Power-Up"))
        {
            ActivatePower();
        }

        if (GUI.Button(new Rect(10, 120, 200, 30), "Buscar Fantasmas"))
        {
            FindAllGhosts();
        }
    }

    int CountVulnerableGhosts()
    {
        int count = 0;
        foreach (GhostController ghost in allGhosts)
        {
            if (ghost != null && ghost.IsVulnerable())
                count++;
        }
        return count;
    }

}