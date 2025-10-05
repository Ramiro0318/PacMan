using UnityEngine;

public class ManejarSonidos : MonoBehaviour
{
    public AudioSource SonidoAceptar;
    public AudioSource SonidoCancelar;
    public void ReproducirSonido(string tag) 
    {

        if (SonidoAceptar != null && tag == "Siguiente-Aceptar")
        {
            SonidoAceptar.Play();
            

        }
        else if (SonidoCancelar != null && tag == "Anterior-Cancelar")
        {
            SonidoCancelar.Play();
        }

    }


    void Update()
    {
        
    }
}
