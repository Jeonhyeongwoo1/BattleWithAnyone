using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Events;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public bool isLogined = false;
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

    public void ReqFindId(string email, string userName, UnityAction<string> success, UnityAction<string> fail)
    {
        string url = Core.settings.url + "/findId/" + email + "/" + userName;

        UnityWebRequest request = UnityWebRequest.Get(url);
        StartCoroutine(RequestData(request, success, fail));
    }

    public void ReqFindPassword(string id, string email, UnityAction<string> success, UnityAction<string> fail)
    {
        string url = Core.settings.url + "/findPw/" + id + "/" + email;

        UnityWebRequest request = UnityWebRequest.Get(url);
        StartCoroutine(RequestData(request, success, fail));
    }

    public void ReqCheckUserId(string id, UnityAction<string> success, UnityAction<string> fail)
    {
        string url = Core.settings.url + "/checkId/" + id;

        UnityWebRequest request = UnityWebRequest.Get(url);
        StartCoroutine(RequestData(request, success, fail));
    }

    public void ReqSignup(string id, string password, string email, string phoneNumber, string name, UnityAction<string> success, UnityAction<string> fail)
    {
        string url = Core.settings.url + "/signup";

        WWWForm form = new WWWForm();
        form.AddField("id", id);
        form.AddField("pw", password);
        form.AddField("email", email);
        form.AddField("phoneNumber", phoneNumber);
        form.AddField("name", name);

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        StartCoroutine(RequestData(request, success, fail));
    }

    public void ReqLogin(string id, string password, UnityAction<string> success, UnityAction<string> fail)
    {
        string url = Core.settings.url + "/login";

        WWWForm form = new WWWForm();
        form.AddField("id", id);
        form.AddField("password", password);

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        StartCoroutine(RequestData(request, success, fail));
    }

    IEnumerator RequestData(UnityWebRequest request, UnityAction<string> success, UnityAction<string> fail)
    {

        if (request == null)
        {
            Debug.LogError("Check Request Method!!");
            fail?.Invoke(null);
            yield break;
        }

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            string error = "Error Code : " + request.responseCode + "/" + request.error;
            Debug.LogError("Failed to GetData" + error);
            fail?.Invoke(error);
            request.Dispose();
            yield break;
        }

        if (request.isDone)
        {
            string data = request.downloadHandler.text;
            if (string.IsNullOrEmpty(request.downloadHandler.text))
            {
                string error = "There is no data";
                fail?.Invoke(error);
                request.Dispose();
                yield break;
            }

            success?.Invoke(data);
            request.Dispose();
        }
    }

    public void ConnectPhotonNetwork(UnityAction done)
    {
        StartCoroutine(ConnectingNetwork(done));
    }

    public void StartCheckingNetwork()
    {
        StartCoroutine(CheckingNetwork());
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

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Log("Failed Create Room. Return Code : " + returnCode + ", Message : " + message);
    }

    void OnApplicationQuit()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    //네트워크 상태를 체크한다.///
    IEnumerator CheckingNetwork()
    {
        while (!PhotonNetwork.IsConnected) { yield return new WaitForSeconds(1f); }

        Core.scenario.OnLoadScenario(nameof(ScenarioLogin));
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
