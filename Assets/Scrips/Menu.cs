using UnityEngine;
using  System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Menu: MonoBehaviour
{
    public void Siguiente()
    {
        
    }
    public void Salir()
    {
        Debug.Log("Saliendo...");
        Application.Quit();
    }
}
