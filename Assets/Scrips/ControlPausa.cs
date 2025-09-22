using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ControlPausa : MonoBehaviour
{
    public GameObject CanvasPausa;
    public GameObject btnDefault;
    
    Keyboard keyboard = Keyboard.current;
    Animator anim;

    private bool pausa = false;

    private void Start()
    {
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
        if (keyboard.escapeKey.wasPressedThisFrame)
        //Input.GetButtonDown("Cancel"))
        {
            if (pausa)
            {
                DesactivarPausa(); //Desactivar
            }
            else
            {   //Activar Pausa
                CanvasPausa.SetActive(true);
                EventSystem.current.SetSelectedGameObject(btnDefault);

                foreach (Animator anim in FindObjectsByType<Animator>(FindObjectsSortMode.None))
                {
                    //if (anim.CompareTag("Personaje"))
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
            //if (anim.CompareTag("Personaje"))
            {
                anim.enabled = true;
            }
        }

        pausa = false;
        Debug.Log($"Pausa = {pausa}");
    }
}



