using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Room : MonoBehaviourPunCallbacks, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Color m_OnHover;
    [SerializeField] Color m_Normal;

    [SerializeField] Text m_Title;
    [SerializeField] Text m_RoomManagerName;
    [SerializeField] Text m_Map;

    public void SetRoomInfo(string title, string roomManagerName, string mapInfo)
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //OnClick
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<Image>().color = m_OnHover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GetComponent<Image>().color = m_Normal;
    }

    // Start is called before the first frame update
    void Start()
    {

    }
}
