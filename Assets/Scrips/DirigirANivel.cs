using UnityEngine;
using UnityEngine.SceneManagement;

public class DirigirANivel : MonoBehaviour
{
        public string EscenaSiguiente; // Asigna el nombre de la escena en el Inspector

        public void IrAEscena()
        {
            SceneManager.LoadScene(EscenaSiguiente);
        }


}
