using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlPausa : MonoBehaviour
{
    public GameObject CanvasPausa;
    private bool pausa = false;
    Keyboard keyboard = Keyboard.current;

    private void Start()
    {
        if (CanvasPausa != null)
            CanvasPausa.SetActive(false);
    }

    private void Update()
    {
        CambuarPausa();
    }

    public void CambuarPausa()
    {
        if (keyboard == null) return;
        if (keyboard.escapeKey.wasPressedThisFrame)
        //Input.GetButtonDown("Cancel"))
        {
            if (pausa)
            {
                DesactivarPausa();
            }
            else
            {   //Activar Pausa
                CanvasPausa.SetActive(true);
                pausa = true;
                Debug.Log($"Pausa = {pausa}");
            }
        } 
    }

    public void DesactivarPausa()
    {
        CanvasPausa.SetActive(false);
        pausa = false;
        Debug.Log($"Pausa = {pausa}");
    }
}



