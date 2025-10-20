using UnityEngine;
using UnityEngine.UI;

public class LivesUI : MonoBehaviour
{
    [Header("Configuración de Vidas")]
    public Image[] lifeIcons; // Arreglo de 3 imágenes para las vidas
    public Color fullLifeColor = Color.white;
    public Color emptyLifeColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    private void Start()
    {
        // Buscar Pacman y suscribirse a eventos
        SimplePacmanMove pacman = FindObjectOfType<SimplePacmanMove>();
        if (pacman != null)
        {
            pacman.OnLivesChanged += HandleLivesChanged;
            // Actualizar UI con el estado inicial
            UpdateLivesUI(pacman.GetCurrentLives());
        }
        else
        {
            Debug.LogError("No se encontró el script SimplePacmanMove en la escena");
        }
    }

    private void HandleLivesChanged(int currentLives)
    {
        UpdateLivesUI(currentLives);
    }

    public void UpdateLivesUI(int currentLives)
    {
        if (lifeIcons == null || lifeIcons.Length != 3)
        {
            Debug.LogWarning("LifeIcons no está configurado correctamente. Debe tener 3 elementos.");
            return;
        }

        // Actualizar cada icono de vida
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
            {
                if (i < currentLives)
                {
                    // Vida activa - color normal
                    lifeIcons[i].color = fullLifeColor;
                }
                else
                {
                    // Vida perdida - color apagado
                    lifeIcons[i].color = emptyLifeColor;
                }
            }
        }

        Debug.Log($"UI de Vidas actualizada: {currentLives}/3 vidas");
    }

    private void OnDestroy()
    {
        // Desuscribirse de eventos para evitar memory leaks
        SimplePacmanMove pacman = FindObjectOfType<SimplePacmanMove>();
        if (pacman != null)
        {
            pacman.OnLivesChanged -= HandleLivesChanged;
        }
    }
}