using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class NavegacionMenu : MonoBehaviour
{
    [Header("Referencias")]
    public EventSystem eventSystem;
    public GameObject primerBoton;

    private Keyboard keyboard;
    private GameObject botonSeleccionado;

    void Start()
    {
        keyboard = Keyboard.current;

        if (eventSystem == null)
            eventSystem = EventSystem.current;

        // Seleccionar el primer botón al activarse
        if (primerBoton != null && eventSystem != null)
        {
            eventSystem.SetSelectedGameObject(primerBoton);
            botonSeleccionado = primerBoton;
        }
    }

    void Update()
    {
        if (keyboard == null || eventSystem == null) return;

        ProcesarNavegacion();
    }

    void ProcesarNavegacion()
    {
        // Navegación con flechas o WASD
        if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
        {
            NavegarArriba();
        }
        else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
        {
            NavegarAbajo();
        }
        else if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
        {
            ActivarBotonSeleccionado();
        }
    }

    void NavegarArriba()
    {
        if (botonSeleccionado != null)
        {
            GameObject siguiente = botonSeleccionado.GetComponent<UnityEngine.UI.Selectable>()
                .FindSelectableOnUp()?.gameObject;

            if (siguiente != null)
            {
                eventSystem.SetSelectedGameObject(siguiente);
                botonSeleccionado = siguiente;
            }
        }
    }

    void NavegarAbajo()
    {
        if (botonSeleccionado != null)
        {
            GameObject siguiente = botonSeleccionado.GetComponent<UnityEngine.UI.Selectable>()
                .FindSelectableOnDown()?.gameObject;

            if (siguiente != null)
            {
                eventSystem.SetSelectedGameObject(siguiente);
                botonSeleccionado = siguiente;
            }
        }
    }

    void ActivarBotonSeleccionado()
    {
        if (botonSeleccionado != null)
        {
            ExecuteEvents.Execute(botonSeleccionado, new BaseEventData(eventSystem),
                ExecuteEvents.submitHandler);
        }
    }

    // Para cuando el mouse selecciona un botón
    public void OnBotonMouseEnter(GameObject boton)
    {
        if (eventSystem != null)
        {
            eventSystem.SetSelectedGameObject(boton);
            botonSeleccionado = boton;
        }
    }
}