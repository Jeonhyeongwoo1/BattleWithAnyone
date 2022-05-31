using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    private Member m_Member;
    public Member member
    {
        get => m_Member;
        set
        {
            m_Member = value;
            PhotonNetwork.NickName = member.mbr_id;
        }
    }

    float m_NetworkMaxWaitTime = 10f;
    bool m_IsConnectSuccessed = false;

    public void Log(string message)
    {
        if (XSettings.networkManagerLog)
        {
            Debug.Log(message);
        }

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

    public IEnumerator OnOtherPlayerDisconnectedDuringLoading()
    {
        //강제로 제어....
        Scene scene = SceneManager.GetSceneByName(nameof(ScenarioRoom));
        if (scene != null)
        {
            SceneManager.UnloadSceneAsync(nameof(ScenarioRoom));
        }

        scene = SceneManager.GetSceneByName(nameof(ScenarioPlay));
        if (scene != null)
        {
            SceneManager.UnloadSceneAsync(nameof(ScenarioPlay));
        }

        Core.scenario.previousScenario = null;
        Core.scenario.currentScenario = null;

        //GameLoading 중에 플레이어가 나가게되면 룸을 떠나고 Home 화면으로 이동한다.
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        NoticePopup.content = MessageCommon.Get("game.player.disconnected");
        Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
        Core.networkManager.WaitStateToConnectedToMasterServer(() => Core.scenario.OnLoadScenario(nameof(ScenarioHome)));
        BattleWtihAnyOneStarter.GetBlockSkybox()?.gameObject.SetActive(true);
        yield return null;
    }

    public void ConnectPhotonNetwork(UnityAction done)
    {
        StartCoroutine(ConnectingNetwork(done));
    }

    public void WaitStateToConnectedToMasterServer(UnityAction done)
    {
        StartCoroutine(WaitingConnectedToMasterServer(done));
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Log("DisConnected" + cause);

        NoticePopup.content = MessageCommon.Get("network.disconnect");
        Core.plugs?.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
        Core.scenario?.OnLoadScenario(nameof(ScenarioLogin));
    }

    public override void OnConnected()
    {
        Log("OnConnected");
    }

    public override void OnConnectedToMaster()
    {
        m_IsConnectSuccessed = true;
        Log("OnConnectedToMaster");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("error code : " + returnCode + "On Create Room Failed : " + message);

        switch (returnCode)
        {
            case (int)PhotonCode.EXIST_ROOM:
                Core.state.mapPreferences = null;
                NoticePopup.content = MessageCommon.Get("room.existroom");
                Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
                break;
            case (int)PhotonCode.OPERATION_NOTALLOWED_INCURRENT_STATE:
                ConfirmPopup.content = MessageCommon.Get("pun.states.waiting");
                Core.plugs.Get<Popups>()?.OpenPopupAsync<ConfirmPopup>();
                break;
            default:
                Core.state.mapPreferences = null;
                NoticePopup.content = MessageCommon.Get("room.createfailed");
                Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
                break;
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("On JoinRoom Failed. Return Code :" + returnCode + ", Message : " + message);

        switch (returnCode)
        {
            case (int)PhotonCode.GAME_FULL:
                NoticePopup.content = MessageCommon.Get("room.gamefull");
                Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
                break;
            default:
                NoticePopup.content = MessageCommon.Get("room.joinroomfailed");
                Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
                break;
        }
    }

    void OnApplicationQuit()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
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

    IEnumerator WaitingConnectedToMasterServer(UnityAction done)
    {
        float elapsed = 0;
        while (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer)
        {
            if (elapsed > m_NetworkMaxWaitTime)
            {
                NoticePopup.content = MessageCommon.Get("network.failed");
                Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
                Core.scenario.OnLoadScenario(nameof(ScenarioLogin));
                BattleWtihAnyOneStarter.GetLoading()?.StopLoading();
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        done?.Invoke();
    }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
    }

}
