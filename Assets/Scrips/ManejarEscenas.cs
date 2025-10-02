using UnityEngine;
using UnityEngine.SceneManagement;

public class ManejarEscenas : MonoBehaviour
{
    GameObject CanvasPausa; //= GameObject.Find("CanvasPausa");
    GameObject CanvasGameOver;// = GameObject.Find("CanvasGameOver");

    //public string EscenaSiguiente; //Asignar el nonmbre de escena

    public void IrAEscena(string escena)
    {
        //SceneManager.LoadScene(EscenaSiguiente); //esto selecciona la escena desde SceneManager
        SceneManager.LoadScene(escena);
        //if (CanvasPausa.activeInHierarchy)
        {
        }
        CanvasPausa = GameObject.Find("CanvasPausa");
        CanvasGameOver = GameObject.Find("CanvasGameOver");
        CanvasGameOver.SetActive(false);
        CanvasPausa.SetActive(false);
    }
        
    public void SalirJuego()
    {
        Debug.Log("Salir del juego");
        Application.Quit();
    }

}
