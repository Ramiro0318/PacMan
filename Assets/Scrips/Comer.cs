using UnityEngine;

public class Comer:MonoBehaviour
{
    //public int score = 0;
    public ControlHUD HUD;
    public Pellet pelletRef;
    
    private int puntosBigPellet;
    private int puntosFantasma;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger con: " + other.name + " - Tag: " + other.tag);

        if (other.tag == "Pallets" || other.CompareTag("Collector") ||
            other.CompareTag("Point") || other.CompareTag("Coin"))
        {
            
            CollectPellet(other.gameObject);
        }
    }

    void CollectPellet(GameObject pellet)
    {
        if (pellet != null)
        {
            pellet.SetActive(true);

        }
        HUD.SumarPuntos(pelletRef.points);

        //score++;

        Debug.Log("Bolita recolectada! Puntaje: " + pelletRef.points);
    }
}