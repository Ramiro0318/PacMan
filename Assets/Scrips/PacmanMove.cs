using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Text.RegularExpressions;

public class SimplePacmanMove : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float collisionCheckDistance = 0.09f;
    public LayerMask wallLayer = LayerMask.GetMask("Wall");


    public GameObject gameOverPanel;
    public bool isDead = false;
    public float respawnTime = 3f;

    private Animator _animator;
    private Vector2 movementInput = Vector2.zero;
    private Vector2 lastDirection = Vector2.right;
    private bool hasInputThisFrame = false;
    private Vector3 initialPosition;

    // Sistema de vidas
    public int maxLives = 3;
    private int currentLives;
    private bool isRespawning = false;

    public System.Action OnPacmanDeath; // Nuevo evento

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        initialPosition = transform.position;
        currentLives = maxLives; // Inicializar con todas las vidas

        // Notificar el estado inicial de vidas
        OnLivesChanged?.Invoke(currentLives);
    }

    void Update()
    {
        HandleInput();
        TryMove();
    }

    void LateUpdate()
    {
        if (isDead) return;

        _animator.SetInteger("DireccionX", (int)movementInput.x);
        _animator.SetInteger("DireccionY", (int)movementInput.y);
        _animator.SetBool("Muerto", false);
    }

    void HandleInput()
    {
        hasInputThisFrame = false;
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        string upPattern = @"^(w|up)$";
        string downPattern = @"^(s|down)$";
        string rightPattern = @"^(d|right)$";
        string leftPattern = @"^(a|left)$";

        var pressedKeysThisFrame = keyboard.allKeys
            .Where(key => key != null && key.wasPressedThisFrame);

        foreach (var key in pressedKeysThisFrame)
        {
            if (key == null || key.keyCode == null) continue;

            string keyName = key.keyCode.ToString().ToLower();
            hasInputThisFrame = true;

            if (Regex.IsMatch(keyName, upPattern))
                lastDirection = Vector2.up;
            else if (Regex.IsMatch(keyName, downPattern))
                lastDirection = Vector2.down;
            else if (Regex.IsMatch(keyName, rightPattern))
                lastDirection = Vector2.right;
            else if (Regex.IsMatch(keyName, leftPattern))
                lastDirection = Vector2.left;
        }

        movementInput = lastDirection;

        if (movementInput.magnitude > 1)
            movementInput.Normalize();
    }

    void TryMove()
    {
        if (isDead || isRespawning) return;

        if (movementInput.magnitude > 0.1f)
        {
            Vector2 moveDirection = movementInput.normalized;
            float distance = moveSpeed * Time.deltaTime + collisionCheckDistance;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, distance, wallLayer);

            Debug.DrawRay(transform.position, moveDirection * distance, hit.collider ? Color.red : Color.green);

            if (hit.collider == null)
            {
                transform.Translate(new Vector3(movementInput.x, movementInput.y, 0) * moveSpeed * Time.deltaTime);
            }
            else
            {
                movementInput = Vector2.zero;
                lastDirection = Vector2.zero;
            }
        }
    }

    public void Die()
    {
        if (isDead || isRespawning) return;

        // Reducir una vida
        currentLives--;

        // Notificar cambio de vida
        OnLivesChanged?.Invoke(currentLives);

        // NUEVO: Notificar que Pacman murió
        OnPacmanDeath?.Invoke();

        isDead = true;

        // Actualizar animación
        _animator.SetBool("Muerto", true);

        movementInput = Vector2.zero;
        lastDirection = Vector2.zero;

        Debug.Log($"Pacman ha muerto! Vidas restantes: {currentLives}");

        // Verificar si quedan vidas
        if (currentLives <= 0)
        {
            GameOver();
        }
        else
        {
            // Respawn después de un tiempo
            Invoke("Respawn", respawnTime);
        }
    }

    void Respawn()
    {
        if (currentLives <= 0) return;

        isDead = false;
        isRespawning = true;
        transform.position = initialPosition;

        // Resetear dirección
        lastDirection = Vector2.right;
        movementInput = Vector2.zero;

        _animator.SetBool("Muerto", false);

        Debug.Log($"Pacman ha resucitado! Vidas restantes: {currentLives}");

        // Pequeño delay de invencibilidad después del respawn
        Invoke("EndRespawn", 1f);
    }

    void EndRespawn()
    {
        isRespawning = false;
    }

    // Agregar este método a la clase SimplePacmanMove
    public void GameOver()
    {
        Debug.Log("GameOver() llamado en SimplePacmanMove");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("Panel de Game Over activado");
        }
        else
        {
            Debug.LogWarning("gameOverPanel no asignado en el inspector");
        }

        isDead = true;
        movementInput = Vector2.zero;
        lastDirection = Vector2.zero;

        // Detener el tiempo del juego
        Time.timeScale = 0f;

        Debug.Log("Game Over completado");
    }

    // Método público para obtener las vidas actuales (útil para UI)
    public int GetCurrentLives()
    {
        return currentLives;
    }

    // Método público para obtener las vidas máximas (útil para UI)
    public int GetMaxLives()
    {
        return maxLives;
    }

    public System.Action<int> OnLivesChanged; // Evento para cambios de vida
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Movimiento: {movementInput}");
        GUI.Label(new Rect(10, 30, 300, 20), $"Última Dirección: {lastDirection}");
        GUI.Label(new Rect(10, 50, 300, 20), $"Input este frame: {hasInputThisFrame}");
        GUI.Label(new Rect(10, 70, 300, 20), $"Estado: {(isDead ? "MUERTO" : "VIVO")}");
        GUI.Label(new Rect(10, 90, 300, 20), $"Vidas: {currentLives}/{maxLives}");
        GUI.Label(new Rect(10, 110, 300, 20), $"Respawning: {isRespawning}");
    }
}