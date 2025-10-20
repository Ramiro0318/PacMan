using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [Header("Configuración de Puntos")]
    public TextMeshProUGUI scoreText;
    public string scoreFormat = "{0}";

    private void Start()
    {
        // Configurar automáticamente si no hay referencia
        if (scoreText == null)
            scoreText = GetComponent<TextMeshProUGUI>();

        // Buscar y suscribirse a eventos
        PelletGenerator pelletGenerator = FindObjectOfType<PelletGenerator>();
        if (pelletGenerator != null)
        {
            pelletGenerator.OnScoreChanged += HandleScoreChanged;
            // Establecer puntuación inicial
            UpdateScoreUI(pelletGenerator.totalScore);
        }
        else
        {
            Debug.LogError("ScoreUI: No se encontró PelletGenerator");
        }

        // Asegurar que el texto esté actualizado
        UpdateScoreUI(0);
    }

    private void HandleScoreChanged(int newScore)
    {
        UpdateScoreUI(newScore);
    }

    private void UpdateScoreUI(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = string.Format(scoreFormat, score);
        }
    }

    private void OnDestroy()
    {
        // Limpieza de eventos
        PelletGenerator pelletGenerator = FindObjectOfType<PelletGenerator>();
        if (pelletGenerator != null)
        {
            pelletGenerator.OnScoreChanged -= HandleScoreChanged;
        }
    }
}