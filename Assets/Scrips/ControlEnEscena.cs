using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ControlEnEscena: MonoBehaviour
{
    public GameObject FromTrancision;
    public GameObject ToTrancision;


    public void Transicion(GameObject btn) 
    {
        FromTrancision.SetActive(false);
        ToTrancision.SetActive(true);
        EventSystem.current.SetSelectedGameObject(btn);

    }

    public void TransicionReversa(GameObject btn) 
    {
        FromTrancision.SetActive(true);
        ToTrancision.SetActive(false);
        EventSystem.current.SetSelectedGameObject(btn);

    }


   
}
