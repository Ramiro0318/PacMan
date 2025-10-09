using UnityEngine;

public class Pellet : MonoBehaviour
{
    public int points = 10;
    public bool isBigPellet = false;
    public PelletGenerator pelletGenerator;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Pellet comido - Tipo: {(isBigPellet ? "Big" : "Normal")} en posición {transform.position}");

            // Notificar al generator PRIMERO
            if (pelletGenerator != null)
            {
                pelletGenerator.OnPelletEaten(isBigPellet, points);
            }
            else
            {
                Debug.LogError("PelletGenerator no asignado!");
            }

            // Destruir el pellet DESPUÉS de notificar
            Destroy(gameObject);
        }
    }
}