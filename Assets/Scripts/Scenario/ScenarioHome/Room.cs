using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Room : MonoBehaviourPunCallbacks, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Color m_OnHover;
    [SerializeField] Color m_Normal;
    [SerializeField] Text m_Title;
    [SerializeField] Text m_RoomManagerName;
    [SerializeField] Text m_Map;

    private string m_RoomName;

    public string GetRoomName() => m_RoomName;

    public void SetRoomInfo(string title, string roomManager, string map)
    {
        m_Title.text = "방 제목 : " + title;
        m_RoomManagerName.text = "Player : " + roomManager;
        m_Map.text = "맵 : " + map;
        m_RoomName = title;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InLobby)
        {
            NoticePopup.content = MessageCommon.Get("network.disconnect");
            Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>(() => Core.scenario.OnLoadScenario(nameof(ScenarioLogin)));
            return;
        }

        Core.gameManager.roomName = m_RoomName;
        Core.scenario.OnLoadScenario(nameof(ScenarioRoom));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<Image>().color = m_OnHover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GetComponent<Image>().color = m_Normal;
    }

}
