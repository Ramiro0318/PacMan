using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ControlPausa : MonoBehaviour
{
    public GameObject CanvasPausa;
    public GameObject btnDefault;

    Keyboard keyboard;
    Animator anim;
    private bool pausa = false;

    private void Start()
    {
        keyboard = Keyboard.current; // Mover aquí para evitar null reference

        if (CanvasPausa != null)
            CanvasPausa.SetActive(false);

        foreach (Animator anim in FindObjectsByType<Animator>(FindObjectsSortMode.None))
        {
            anim.enabled = true;
        }
        Time.timeScale = 1f;
    }

    private void Update()
    {
        CambiarPausa();
    }

    public void CambiarPausa()
    {
        if (keyboard == null) return;

        // CORREGIDO: Estructura condicional correcta
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            if (pausa)
            {
                DesactivarPausa();
            }
            else
            {
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
            }
        }
    }

    public void DesactivarPausa()
    {
        CanvasPausa.SetActive(false);
        Time.timeScale = 1f;
        foreach (Animator anim in FindObjectsByType<Animator>(FindObjectsSortMode.None))
        {
            if (anim.CompareTag("Personaje"))
            {
                anim.enabled = true;
            }
        }

        pausa = false;
        Debug.Log($"Pausa = {pausa}");
    }
}