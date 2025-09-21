using UnityEngine;
using UnityEngine.SceneManagement;

public class ManejarEscenas : MonoBehaviour
{
    //public string EscenaSiguiente; //Asignar el nonmbre de escena

    public void IrAEscena(string escena)
    {
        //SceneManager.LoadScene(EscenaSiguiente); //esto selecciona la escena desde SceneManager
        SceneManager.LoadScene(escena);
    }
        
    public void SalirJuego()
    {
        Debug.Log("Salir del juego");
        Application.Quit();
    }

}
