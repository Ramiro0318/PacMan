using Unity.VisualScripting;
using UnityEditor.Rendering;
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

    public void CambiarVolumen(GameObject sldMusica) 
    {
        //audioSource.volume = sldMusica.CloneViaFakeSerialization().GetComponent<Slider>().value;
        audioSource.volume = sldMusica.GetComponent<Slider>().value * .10f;
        if (audioSource.volume == 0)
        {
            mute = true;
        }
        else mute = false;

        audioSource.mute = mute;
        iconImage.sprite = mute ? muteIcon : unmuteIcon;
    }

}
