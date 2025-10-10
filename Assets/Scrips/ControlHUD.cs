using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ControlHUD : MonoBehaviour
{
    public TextMeshProUGUI TextoPuntuacion;

    private int puntuacion = 0;

    public void SumarPuntos(int puntos) 
    {
        puntuacion += puntos;
        TextoPuntuacion.text = puntuacion.ToString();
        Debug.Log(puntuacion.ToString());
    
    }
}
