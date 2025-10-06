using NUnit.Framework.Constraints;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class ManejarSonidos : MonoBehaviour
{

    public List<AudioSource> audioSources = new();
    public AudioSource sonidoAceptar;
    public AudioSource sonidoCancelar;

    //Sonido
    public Image iconImage;
    public Sprite sonidoMuteIcon;
    public Sprite sonidoUnmuteIcon;
    private bool mute = false;
    public void ReproducirSonido(string tag)
    {

        if (sonidoAceptar != null && tag == "Siguiente-Aceptar")
        {
            sonidoAceptar.Play();


        }
        else if (sonidoCancelar != null && tag == "Anterior-Cancelar")
        {
            sonidoCancelar.Play();
        }

    }
    public void SilenciarSonido()
    {
        mute = !mute;
        foreach (var audioS in audioSources)
        {
            if (audioS != null)
            {
                audioS.mute = mute;
            }
        }
        iconImage.sprite = mute ? sonidoMuteIcon : sonidoUnmuteIcon;
    }

    public void CambiarVolumen(GameObject sldMusica)
    {
        //audioSource.volume = sldMusica.CloneViaFakeSerialization().GetComponent<Slider>().value;
        var volumen = sldMusica.GetComponent<Slider>().value * .10f;

        mute = volumen == 0;
        foreach (var audioS in audioSources)
        {


            if (audioS != null)
            {
                audioS.volume = volumen;
                audioS.mute = mute;
            }
        }
            iconImage.sprite = mute ? sonidoMuteIcon : sonidoUnmuteIcon;
    }
   
}
