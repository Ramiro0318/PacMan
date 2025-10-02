using UnityEngine;
using UnityEngine.Tilemaps;


public class Pellet : MonoBehaviour
{
    //Esto es solo para probar proxximanete lo implementamos en la interfaz
    public int points = 10;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Bolita comida! Distancia: " +
                     Vector3.Distance(transform.position, other.transform.position));
            Destroy(gameObject);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.05f);
    }
}