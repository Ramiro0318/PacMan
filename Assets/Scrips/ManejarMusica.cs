using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ManejarMusica: MonoBehaviour
{
    public AudioSource audioSource;
    public WaitForSeconds seconds = new WaitForSeconds(0.1f);
    private float loopStart = 0f;
    private float loopEnd = 4.94f;
    private int loops = 0;
    private bool mute = false;
    
    //Musica
    public Image iconImage;
    public Sprite muteIcon;
    public Sprite unmuteIcon;

    //Sonido
    public Image SonidoIconImage;
    public Sprite SonidoMuteIcon;
    public Sprite SonidoUnmuteIcon;
    private bool sonidoMute = false;


    void Start()
    {
        audioSource.time = loopStart;
        audioSource.Play();
    }

    void Update()
    {
        if (audioSource.time >= loopEnd & loops < 3)
        {
            audioSource.Pause();
            audioSource.UnPause();
            audioSource.time = loopStart;
            loops ++;
        }
    }




    public void SilenciarMusica()
    {
        mute = !mute;
        audioSource.mute = mute;
        iconImage.sprite = mute ? muteIcon : unmuteIcon;
    }

    public void SilenciarSonido()
    {
        sonidoMute = !sonidoMute;
        SonidoIconImage.sprite = sonidoMute ? SonidoMuteIcon : SonidoUnmuteIcon;
    }



}
