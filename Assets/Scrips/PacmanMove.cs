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

    // Variables para la muerte
    public GameObject gameOverPanel; // Opcional: panel de game over
    public bool isDead = false;
    public float respawnTime = 3f; // Tiempo para respawn

    private Animator _animator;
    private Vector2 movementInput = Vector2.zero;
    private Vector2 lastDirection = Vector2.right;
    private bool hasInputThisFrame = false;
    private Vector3 initialPosition; // Posición inicial para respawn

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        initialPosition = transform.position; // Guardar posición inicial
    }

    void Update()
    {
        // Si está muerto, no procesar movimiento
        if (isDead) return;

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
        if (isDead) return;

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
        if (isDead) return; // Evitar múltiples llamadas

        isDead = true;

        // Activar animación de muerte
        _animator.SetBool("Muerto", true);

        // Detener movimiento
        movementInput = Vector2.zero;
        lastDirection = Vector2.zero;

        // Opcional: Mostrar game over
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Debug.Log("Pacman ha muerto!");

      
    }

    void Respawn()
    {
        isDead = false;
        transform.position = initialPosition;

        // Reactivar movimiento
        lastDirection = Vector2.right;
        movementInput = Vector2.zero;

        // Ocultar game over panel si existe
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        _animator.SetBool("Muerto", false);

        Debug.Log("Pacman ha resucitado!");
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Movimiento: {movementInput}");
        GUI.Label(new Rect(10, 30, 300, 20), $"Última Dirección: {lastDirection}");
        GUI.Label(new Rect(10, 50, 300, 20), $"Input este frame: {hasInputThisFrame}");
        GUI.Label(new Rect(10, 70, 300, 20), $"Estado: {(isDead ? "MUERTO" : "VIVO")}");
    }
}