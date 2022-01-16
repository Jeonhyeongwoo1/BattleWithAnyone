using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class RoomMenu : MonoBehaviourPunCallbacks
{
    public List<RoomInfo> roomList = new List<RoomInfo>();
    public Transform roomPrefab;
    public CreateRoomForm roomForm;

    [SerializeField] Text m_NoRoom;
    [SerializeField] Transform m_Room;
    [SerializeField] Button m_CreateRoom;
    [SerializeField] Transform m_RoomContent;

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room List Update");

        if (roomList.Count != 0)
        {
            this.roomList.AddRange(roomList);
        }
    }

    public void OnEnableRoomMenu()
    {
        m_Room.gameObject.SetActive(true);
        m_CreateRoom.gameObject.SetActive(true);

        if (roomList.Count == 0)
        {
            m_NoRoom.gameObject.SetActive(true);
            return;
        }

        int count = roomList.Count;
        for (int i = 0; i < count; i++)
        {
            Transform viewContent = Instantiate<Transform>(roomPrefab, Vector3.zero, Quaternion.identity, m_RoomContent);
            viewContent.name = "Room : " + i;
        }

        m_NoRoom.gameObject.SetActive(false);
    }

    void OnCreateRoom()
    {
        if (!PhotonNetwork.IsConnected) { return; }

        roomForm.gameObject.SetActive(true);

    //    PhotonNetwork.CreateRoom("JeonHyeongwooRoom", new RoomOptions { MaxPlayers = 2 });
    //    Core.scenario.OnLoadScenario(nameof(ScenarioRoom));
    }

    private void Start()
    {
        m_CreateRoom.onClick.AddListener(OnCreateRoom);
    }
}
