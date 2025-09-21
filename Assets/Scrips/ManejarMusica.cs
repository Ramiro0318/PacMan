using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ManejarMusica: MonoBehaviour
{
    public AudioSource audioSource;
    public WaitForSeconds seconds = new WaitForSeconds(0.1f);
    float loopStart = 0f;
    float loopEnd = 4.94f;
    int loops = 0;

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

}
