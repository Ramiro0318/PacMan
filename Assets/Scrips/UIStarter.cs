using UnityEngine;
using UnityEngine.EventSystems;

public class UIStarter : MonoBehaviour
{
    public GameObject btnInicio; 

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(btnInicio);
    }


}
