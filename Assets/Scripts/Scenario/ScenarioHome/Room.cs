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
        m_Title.text = Core.language.GetUIMessage("home.room.roomtitle") + title;
        m_RoomManagerName.text = Core.language.GetUIMessage("home.room.player") + roomManager;
        m_Map.text = Core.language.GetUIMessage("home.room.map") + map;
        m_RoomName = title;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InLobby)
        {
            NoticePopup.content = Core.language.GetNotifyMessage("network.disconnect");
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
