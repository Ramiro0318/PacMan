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
        OnPause();
    }

    public void OnPause()
    {
        if (keyboard == null) return;
        if (keyboard.escapeKey.wasPressedThisFrame)
        //Input.GetButtonDown("Cancel"))
        {
            if (pausa)
            {
                CanvasPausa.SetActive(false);
                pausa = false;
                Debug.Log($"Pausa = {pausa}");
            }
            else
            {
                CanvasPausa.SetActive(true);
                pausa = true;
                Debug.Log($"Pausa = {pausa}");
            }
        }
    }

}

