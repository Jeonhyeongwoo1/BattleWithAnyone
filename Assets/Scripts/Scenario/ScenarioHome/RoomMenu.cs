using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Linq;

public class RoomMenu : MonoBehaviourPunCallbacks
{
    public Room roomPrefab;
    public CreateRoomForm roomForm;

    [SerializeField] Button m_SearchedClose;
    [SerializeField] Text m_SearchedRoomCount;
    [SerializeField] Transform m_SearchedRoom;
    [SerializeField] Transform m_SearchedRoomContent;
    [SerializeField] SearchRoom m_SearchRoomPopup;
    [SerializeField] Button m_Search;
    [SerializeField] Button m_Refresh;
    [SerializeField] Text m_NoRoom;
    [SerializeField] Transform m_Room;
    [SerializeField] Button m_CreateRoom;
    [SerializeField] Transform m_RoomContent;

    bool m_IsRefreshing = false;
    Dictionary<string, RoomInfo> m_CachedRoomList = new Dictionary<string, RoomInfo>();
    List<RoomInfo> m_CopyRoomList = new List<RoomInfo>();

    public void OnEnableRoomMenu()
    {
        m_Room.gameObject.SetActive(true);
        m_CreateRoom.gameObject.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (m_IsRefreshing)
        {
            foreach (RoomInfo v in roomList)
            {
                m_CopyRoomList.Add(v);
            }
            return;
        }

        RoomListUpdate(roomList);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined");
    }

    public void SearchRoom(string roomName)
    {
        if (m_SearchedRoomContent.childCount != 0)
        {
            DestroySearchedRoomList();
        }

        if (!m_SearchedRoom.gameObject.activeSelf)
        {
            m_SearchedRoom.gameObject.SetActive(true);
        }

        int count = 0;
        foreach (KeyValuePair<string, RoomInfo> roomInfo in m_CachedRoomList)
        {
            bool o = roomInfo.Key.StartsWith(roomName) || roomInfo.Key.Contains(roomName);
            if (o)
            {
                CreateRoom(roomInfo.Value, m_SearchedRoomContent);
                count++;
            }
        }

        m_SearchedRoomCount.text = count.ToString();

    }

    void SearchedRoomClose()
    {
        DestroySearchedRoomList();
        m_SearchedRoom.gameObject.SetActive(false);
    }

    void DestroySearchedRoomList()
    {
        for (int i = 0; i < m_SearchedRoomContent.childCount; i++)
        {
            Transform tr = m_SearchedRoomContent.GetChild(i);
            if (tr.name == "Top") { continue; }
            Destroy(tr.gameObject);
        }
    }

    void RoomListUpdate(List<RoomInfo> roomList)
    {
        int count = roomList.Count;
        for (int i = 0; i < count; i++)
        {
            RoomInfo roomInfo = roomList[i];

            if (m_CachedRoomList.ContainsKey(roomInfo.Name) && roomInfo.RemovedFromList)
            {
                m_CachedRoomList.Remove(roomInfo.Name);

                Transform room = m_RoomContent.Find(roomInfo.Name);
                if (room)
                {
                    Destroy(room.gameObject);
                }

            }
            else
            {
                if (roomInfo.PlayerCount == 0) { continue; }

                CreateRoom(roomInfo, m_RoomContent);
                m_CachedRoomList.Add(roomInfo.Name, roomInfo);
            }
        }

        m_Refresh.gameObject.SetActive(m_CachedRoomList.Count != 0);
        m_Search.gameObject.SetActive(m_CachedRoomList.Count != 0);
        m_NoRoom.gameObject.SetActive(m_CachedRoomList.Count == 0);

    }

    void CreateRoom(RoomInfo roomInfo, Transform parent)
    {
        string roomManager = (string)roomInfo.CustomProperties["RoomManager"];
        string mapTitle = (string)roomInfo.CustomProperties["Map"];
        Room room = Instantiate<Room>(roomPrefab, Vector3.zero, Quaternion.identity, parent);
        room.SetRoomInfo(roomInfo.Name, roomManager, mapTitle);
        room.name = roomInfo.Name;
    }

    void OnCreateRoom()
    {
        roomForm.CreateMapList();
        roomForm.gameObject.SetActive(true);
    }

    void RefreshRoom()
    {
        if (m_CachedRoomList.Count == 0) { return; }

        StartCoroutine(RefreshingRoom());
    }

    IEnumerator RefreshingRoom()
    {
        m_IsRefreshing = true;

        for (int i = 0; i < m_RoomContent.childCount; i++)
        {
            Transform tr = m_RoomContent.GetChild(i);
            Destroy(tr.gameObject);
        }

        foreach (KeyValuePair<string, RoomInfo> roomInfo in m_CachedRoomList)
        {
            CreateRoom(roomInfo.Value, m_RoomContent);
            yield return null;
        }

        if (m_CopyRoomList.Count > 0)
        {
            RoomListUpdate(m_CopyRoomList);
            m_CopyRoomList.Clear();
        }

        m_IsRefreshing = false;

    }

    private void Start()
    {
        m_SearchedClose.onClick.AddListener(SearchedRoomClose);
        m_CreateRoom.onClick.AddListener(OnCreateRoom);
        m_Refresh.onClick.AddListener(RefreshRoom);
        m_Search.onClick.AddListener(() => m_SearchRoomPopup.Open());
    }
}
