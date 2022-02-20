using UnityEngine;
using System;
using Photon.Pun;

public class GamePlayManager : MonoBehaviour
{
    [Serializable]
    public class MapPreferences
    {
        public string mapName;
        public int numberOfRound = 3; //게임 라운드 횟수
        public int roundTime = 150;
    }

    private string m_RoomName;
    public string roomName
    {
        get
        {
            if (PhotonNetwork.InLobby)
            {
                return m_RoomName;
            }
            return null;
        }
        set
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InLobby)
            {
                m_RoomName = value;
            }
        }
    }

    private MapPreferences mapPreferences = new MapPreferences();

    public void SetMapPreference(string mapName, int numberOfround, int roundTime)
    {
        mapPreferences.mapName = mapName;
        mapPreferences.numberOfRound = numberOfround;
        mapPreferences.roundTime = roundTime;
    }

    public MapPreferences GetMapPreference() => mapPreferences;

    public bool HasMapPreference()
    {
        if (mapPreferences == null) { return false; }
        if (mapPreferences.mapName == null) { return false; }
        return true;
    }
}
