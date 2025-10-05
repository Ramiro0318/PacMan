using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ControlEnEscena : MonoBehaviour
{
    public GameObject FromTransicion;
    GameObject ToTransicion;
    Vector2 destino = new Vector3(0, 0);
    RectTransform RectPanelTo;
    RectTransform RectPanelFrom;
    bool enTransicion, enReversa;

    public void Transicion(GameObject toTransicion)
    {
        ToTransicion = toTransicion;
        FromTransicion.SetActive(true);
        toTransicion.SetActive(true);
        Debug.Log( toTransicion.name);

        RectPanelFrom = FromTransicion.GetComponent<RectTransform>();
        RectPanelTo = toTransicion.GetComponent<RectTransform>();
        //EnTransicion |= RectPanel.GetComponent<bool>();
        enTransicion = true;


    }
    public void TransicionReversa(GameObject toTransicion)
    {
        FromTransicion.SetActive(true);
        //toTransicion.SetActive(false);

        RectPanelFrom = FromTransicion.GetComponent<RectTransform>();
        RectPanelTo = toTransicion.GetComponent<RectTransform>();

        enTransicion = true;
        enReversa = true;

    }

    private void Update()
    {
        if (enTransicion && !enReversa && RectPanelTo != null)
        {
            RectPanelFrom.anchoredPosition = Vector2.MoveTowards(RectPanelFrom.anchoredPosition, new Vector2(-RectPanelFrom.rect.width, 0), 500f * Time.deltaTime);
            RectPanelTo.anchoredPosition = Vector2.MoveTowards(RectPanelTo.anchoredPosition, destino, 500f * Time.deltaTime);

            if (RectPanelTo.anchoredPosition == destino)
            {
                enTransicion = false;
                FromTransicion.SetActive(false);
            }
        }
        else if (enTransicion && enReversa && RectPanelTo != null)
        {
            RectPanelFrom.anchoredPosition = Vector2.MoveTowards(RectPanelFrom.anchoredPosition, new Vector2(0, 0), 500f * Time.deltaTime);
            RectPanelTo.anchoredPosition = Vector2.MoveTowards(RectPanelTo.anchoredPosition, new Vector2(RectPanelTo.rect.width, 0), 500f * Time.deltaTime);

            if (RectPanelFrom.anchoredPosition == destino)
            {
                enTransicion = false;
                enReversa = false;
                ToTransicion.SetActive(false);
            }
        }
    }




}
