using UnityEngine;
using UnityEngine.EventSystems;

public class UIStarter : MonoBehaviour
{
    public GameObject BtnDefault;

    public void Seleccionado(GameObject btn)
    {

        EventSystem.current.SetSelectedGameObject(btn);


    }

    void Start()
    {
        //var btnDefault = GameObject.Find("btnInicio");
        EventSystem.current.SetSelectedGameObject(null);
        Seleccionado(BtnDefault);

    }




}
