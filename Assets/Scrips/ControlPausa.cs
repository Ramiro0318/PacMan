using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ControlPausa : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject canvasPausa;
    public GameObject btnDefault;

    [Header("Configuración")]
    public string tagPersonaje = "Personaje";

    private Keyboard keyboard;
    private bool estaEnPausa = false;
    private Animator[] animadoresPersonajes;

    void Start()
    {
        keyboard = Keyboard.current;

        // Inicializar UI
        if (canvasPausa != null)
            canvasPausa.SetActive(false);

        // Buscar y almacenar todos los animadores de personajes al inicio
        BuscarAnimadoresPersonajes();

        // Asegurar que el juego empiece sin pausa
        ReanudarJuego();
    }

    void Update()
    {
        ProcesarInputPausa();
    }

    void BuscarAnimadoresPersonajes()
    {
        // Buscar una vez al inicio para mejor performance
        Animator[] todosAnimadores = FindObjectsOfType<Animator>();
        System.Collections.Generic.List<Animator> personajes = new System.Collections.Generic.List<Animator>();

        foreach (Animator anim in todosAnimadores)
        {
            if (anim.CompareTag(tagPersonaje))
            {
                personajes.Add(anim);
            }
        }

        animadoresPersonajes = personajes.ToArray();
        Debug.Log($"Encontrados {animadoresPersonajes.Length} animadores de personajes");
    }

    void ProcesarInputPausa()
    {
        if (keyboard == null)
        {
            keyboard = Keyboard.current;
            return;
        }

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            if (estaEnPausa)
            {
                DesactivarPausa();
            }
            else
            {
<<<<<<< HEAD
                ActivarPausa();
=======
                Debug.Log(CanvasPausa);
                // Activar Pausa
                CanvasPausa.SetActive(true);
                EventSystem.current.SetSelectedGameObject(btnDefault);

                foreach (Animator anim in FindObjectsByType<Animator>(FindObjectsSortMode.None))
                {
                    if (anim.CompareTag("Personaje"))
                    {
                        anim.enabled = false;
                    }
                }
                Time.timeScale = 0f;

                pausa = true;
                Debug.Log($"Pausa = {pausa}");
>>>>>>> origin/Desarrollo
            }
        }
    }

    public void ActivarPausa()
    {
        if (estaEnPausa) return;

        estaEnPausa = true;

        // Activar UI de pausa
        if (canvasPausa != null)
        {
            canvasPausa.SetActive(true);
        }

        // Configurar botón por defecto
        if (btnDefault != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(btnDefault);
        }

        // Pausar animaciones de personajes
        PausarAnimacionesPersonajes();

        // Pausar el tiempo del juego
        Time.timeScale = 0f;

        Debug.Log("Juego en pausa");
    }

    public void DesactivarPausa()
    {
        if (!estaEnPausa) return;

        estaEnPausa = false;

        // Desactivar UI de pausa
        if (canvasPausa != null)
        {
            canvasPausa.SetActive(false);
        }

        // Reanudar animaciones
        ReanudarAnimacionesPersonajes();

        // Reanudar el tiempo del juego
        Time.timeScale = 1f;

        Debug.Log("Juego reanudado");
    }

    void PausarAnimacionesPersonajes()
    {
        if (animadoresPersonajes == null) return;

        foreach (Animator anim in animadoresPersonajes)
        {
            if (anim != null)
            {
                anim.enabled = false;
            }
        }
    }

    void ReanudarAnimacionesPersonajes()
    {
        if (animadoresPersonajes == null) return;

        foreach (Animator anim in animadoresPersonajes)
        {
            if (anim != null)
            {
                anim.enabled = true;
            }
        }
    }

    void ReanudarJuego()
    {
        Time.timeScale = 1f;
        estaEnPausa = false;

        if (canvasPausa != null)
            canvasPausa.SetActive(false);

        ReanudarAnimacionesPersonajes();
    }

    // Métodos públicos para los botones UI
    public void BotonReanudar()
    {
        DesactivarPausa();
    }

    public void BotonReiniciar()
    {
        // Reanudar el tiempo antes de reiniciar
        Time.timeScale = 1f;

        // Recargar la escena actual
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public void BotonMenuPrincipal()
    {
        // Reanudar el tiempo antes de cambiar de escena
        Time.timeScale = 1f;

        // Cambiar a la escena del menú principal (ajusta el nombre)
        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuPrincipal");
    }

    public void BotonSalir()
    {
        Debug.Log("Saliendo del juego...");

        // En el editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // En build
        Application.Quit();
#endif
    }

    // Manejar cuando la aplicación pierde/gana foco
    void OnApplicationFocus(bool hasFocus)
    {
        // Opcional: Pausar automáticamente cuando pierde foco
        if (!hasFocus && !estaEnPausa)
        {
            ActivarPausa();
        }
    }

    void OnDestroy()
    {
        // Asegurar que el tiempo se reanude al destruir el objeto
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }
}