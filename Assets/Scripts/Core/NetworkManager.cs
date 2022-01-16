using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.Events;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public string userNickName;

    float m_NetworkMaxWaitTime = 10f;
    bool m_IsConnectSuccessed = false;

    public void Log(string message)
    {
        Debug.Log(message);
    }

    public void SetPlayerName(string name)
    {
        PhotonNetwork.LocalPlayer.NickName = name;
        userNickName = PhotonNetwork.LocalPlayer.NickName;
    }

    public void ConnectPhotonNetwork(UnityAction done)
    {
        StartCoroutine(ConnectingNetwork(done));
    }

    public void LeaveRoom()
    {
        if (!PhotonNetwork.IsConnected) { return; }
        PhotonNetwork.LeaveRoom();
    }

    public void JoinRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            Log("Join Room");
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Log("DisConnected");
    }

    public override void OnConnected()
    {
        Log("OnConnected");
    }
    
    public override void OnConnectedToMaster()
    {
        m_IsConnectSuccessed = true;
        Log("OnConnectedToMaster");
        //        m_UserName.text = PhotonNetwork.LocalPlayer.NickName;
    }

    public override void OnJoinedRoom()
    {
        Log("Joined Room " + PhotonNetwork.CurrentRoom.Players);
        //        GameObject go = PhotonNetwork.Instantiate("player", Vector3.zero, Quaternion.identity);
        //        go.name = PhotonNetwork.LocalPlayer.NickName;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Log("On JoinRoom Failed");
    }

    public override void OnJoinedLobby()
    {
        Log("On Joinned Lobby");
    }

    public override void OnCreatedRoom()
    {
        Log("On Created Room");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Log("Failed Create Room. Return Code : " + returnCode + ", Message : " + message);
    }

    IEnumerator ConnectingNetwork(UnityAction done)
    {
        float elapsed = 0;
        bool o = PhotonNetwork.ConnectUsingSettings();

        if (!o) { Log("Failed to Network Connect. Check User Settings"); }

        while (!m_IsConnectSuccessed)
        {
            if (elapsed > m_NetworkMaxWaitTime && !o)
            {
                Log("Failed to Network Connect");
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        done?.Invoke();
    }

}
