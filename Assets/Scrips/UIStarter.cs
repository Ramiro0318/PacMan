using UnityEngine;
using UnityEngine.EventSystems;

public class UIStarter : MonoBehaviour
{
    public GameObject btnInicio;

    void Start()
    {
        if (btnInicio != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(btnInicio);
        }
        else { 
        
        
        }
    }


}
