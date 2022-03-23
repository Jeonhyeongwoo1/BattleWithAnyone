using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class RoomMenu : MonoBehaviourPunCallbacks
{
    public Room roomPrefab;

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

    public override void OnCreatedRoom()
    {
        Debug.Log("On Created Room");
        Core.scenario.OnLoadScenario(nameof(ScenarioRoom));
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
            bool o = roomInfo.Key.Contains(roomName);
            if (o)
            {
                OtherCreateRoom(roomInfo.Value, m_SearchedRoomContent);
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

    void RemoveRoom(string roomName)
    {
        m_CachedRoomList.Remove(roomName);
        Transform room = m_RoomContent.Find(roomName);
        if (room) { Destroy(room.gameObject); }
    }

    void RoomListUpdate(List<RoomInfo> roomList)
    {
        int count = roomList.Count;
        for (int i = 0; i < count; i++)
        {
            RoomInfo roomInfo = roomList[i];
            if (m_CachedRoomList.ContainsKey(roomInfo.Name) && roomInfo.RemovedFromList)
            {
                RemoveRoom(roomInfo.Name);
            }
            else if (m_CachedRoomList.ContainsKey(roomInfo.Name))
            {
                string roomName = roomInfo.Name;
                if (roomInfo.PlayerCount == roomInfo.MaxPlayers)
                {
                    RemoveRoom(roomName);
                    continue;
                }

                //Update Custom Prerties
                RoomInfo existRoom = m_CachedRoomList[roomName];
                string oldRoomManager = (string)existRoom.CustomProperties["RoomManager"];
                string oldMap = (string)existRoom.CustomProperties["Map"];

                string newRoomManager = (string)roomInfo.CustomProperties["RoomManager"];
                string newMap = (string)roomInfo.CustomProperties["Map"];

                if (oldRoomManager != newRoomManager || oldMap != newMap)
                {
                    Room room = m_RoomContent.Find(roomName).GetComponent<Room>();
                    room.SetRoomInfo(roomName, newRoomManager, newMap);
                }

                continue;
            }
            else
            {
                if (roomInfo.PlayerCount == 0) { continue; }
                if (roomInfo.MaxPlayers == roomInfo.PlayerCount) { continue; } //풀방일 경우

                OtherCreateRoom(roomInfo, m_RoomContent);
                m_CachedRoomList.Add(roomInfo.Name, roomInfo);
            }
        }

        int c = m_CachedRoomList.Count;
        m_Refresh.gameObject.SetActive(c != 0);
        m_Search.gameObject.SetActive(c != 0);
        m_NoRoom.gameObject.SetActive(c == 0);

    }

    void OtherCreateRoom(RoomInfo roomInfo, Transform parent)
    {
        string roomManager = (string)roomInfo.CustomProperties["RoomManager"];
        string mapTitle = (string)roomInfo.CustomProperties["Map"];
        Room room = Instantiate<Room>(roomPrefab, Vector3.zero, Quaternion.identity, parent);
        room.SetRoomInfo(roomInfo.Name, roomManager, mapTitle);
        room.name = roomInfo.Name;
    }

    void OnClickCreateRoom()
    {
        if (Core.plugs.Has<MapSettings>())
        {
            MapSettings settings = Core.plugs.Get<MapSettings>();
            settings.Open();
            settings.confirm = (roomName) => UserCreateRoom(roomName);
        }
    }

    void UserCreateRoom(string roomName)
    {
        string[] LobbyOptions = new string[4];
        LobbyOptions[0] = "RoomManager";
        LobbyOptions[1] = "Map";
        LobbyOptions[2] = "NumberOfRound";
        LobbyOptions[3] = "RoundTime";

        GamePlayManager.MapPreferences preferences = Core.gameManager.GetMapPreference();
        string map = preferences.mapName;
        int numberOfRound = preferences.numberOfRound;
        int roundTime = preferences.roundTime;

        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable() {
                                            { "RoomManager", PhotonNetwork.LocalPlayer.NickName },
                                            { "Map", map },
                                            { "NumberOfRound", numberOfRound},
                                            { "RoundTime", roundTime }};

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        roomOptions.MaxPlayers = (byte)2;
        roomOptions.CustomRoomPropertiesForLobby = LobbyOptions;
        roomOptions.CustomRoomProperties = customProperties;
        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
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
            OtherCreateRoom(roomInfo.Value, m_RoomContent);
            yield return new WaitForSeconds(0.2f);
        }

        if (m_CopyRoomList.Count > 0)
        {
            RoomListUpdate(m_CopyRoomList);
            m_CopyRoomList.Clear();
        }

        m_IsRefreshing = false;

    }

    void Awake()
    {
        m_SearchedClose.onClick.AddListener(SearchedRoomClose);
        m_Refresh.onClick.AddListener(RefreshRoom);
        m_Search.onClick.AddListener(() => m_SearchRoomPopup.Open());
        m_CreateRoom.onClick.AddListener(OnClickCreateRoom);
    }
}
