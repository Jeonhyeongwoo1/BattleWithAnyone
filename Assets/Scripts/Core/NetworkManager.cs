using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Events;
using UnityEngine.Networking;
using AppleAuth;
using System;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    [Serializable]
    public class AppleAuth
    {
        public string appleUser;
        public string authCode;
        public string idToken;
    }

    AppleAuth m_AppleAuth;
    public AppleAuth appleAuth
    {
        get => m_AppleAuth;
        set => m_AppleAuth = value;
    }

    private Member m_Member;
    public Member member
    {
        get => m_Member;
        set => m_Member = value;
    }

    IAppleAuthManager m_AppleAuthManager;
    public IAppleAuthManager appleAuthManager
    {
        get => m_AppleAuthManager;
        set => m_AppleAuthManager = value;
    }

    [SerializeField] float m_CheckTokenValueTime = 30f;

    public void Log(string message)
    {
        if (XSettings.networkManagerLog)
        {
            Debug.Log(message);
        }

    }

    public void ReqUpdateAppleToken(string token, string id, UnityAction<string> success, UnityAction<string> fail)
    {
        string url = Core.settings.url + "/appleUpdateToken";

        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("id", id);

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        StartCoroutine(RequestData(request, success, fail));
    }

    public void ReqLoginAppleAuth(string appleUser, UnityAction<string> success, UnityAction<string> fail)
    {
        string url = Core.settings.url + "/appleLogin";

        WWWForm form = new WWWForm();
        form.AddField("id", appleUser);

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        StartCoroutine(RequestData(request, success, fail));
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

    public void ReqAppleSignup(string id, string password, string email, string phoneNumber, string name, UnityAction<string> success, UnityAction<string> fail)
    {
        //if (appleAuth == null) { return; }

        string url = Core.settings.url + "/appleSignup";

        WWWForm form = new WWWForm();
        form.AddField("id", id);
        form.AddField("pw", password);
        form.AddField("email", email);
        form.AddField("phoneNumber", phoneNumber);
        form.AddField("name", name);
        form.AddField("appleUser", appleAuth.appleUser);
        form.AddField("authCode", appleAuth.authCode);
        form.AddField("idToken", appleAuth.idToken);

        UnityWebRequest request = UnityWebRequest.Post(url, form);
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
        if (PhotonNetwork.IsConnected)
        {
            NoticePopup.content = MessageCommon.Get("network.alreadyconnected");
            Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
            return;
        }

        StartCoroutine(ConnectingNetwork(done));
    }

    public void WaitStateToConnectedToMasterServer(UnityAction done)
    {
        StartCoroutine(WaitingConnectedToMasterServer(done));
    }

    public void CheckTokenValue()
    {
        StartCoroutine(CheckingTokenValue());
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Log("DisConnected" + cause);
        ConfirmPopup.content = MessageCommon.Get("network.disconnect");
        Core.plugs?.Get<Popups>()?.OpenPopupAsync<ConfirmPopup>(() => Application.Quit());
/*
        NoticePopup.content = MessageCommon.Get("network.disconnect");
        Core.plugs?.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
        Core.scenario?.OnLoadScenario(nameof(ScenarioLogin));

        //in ScenarioPlay..
        XTheme xTheme = Core.plugs?.Get<XTheme>();
        if(xTheme != null)
        {
            Core.plugs?.Unload<XTheme>();
        }

        if (Core.models.Has())
        {
            //Bug Fix
            Core.models.UnloadCurModel();
        }
*/
    }

    public override void OnConnected()
    {
        Log("OnConnected");
    }

    public override void OnConnectedToMaster()
    {
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
        float m_NetworkMaxWaitTime = 10f;
        bool o = PhotonNetwork.ConnectUsingSettings();

        if (!o) { Log("Failed to Network Connect. Check User Settings"); }

        while (!PhotonNetwork.IsConnected)
        {
            if (elapsed > m_NetworkMaxWaitTime && !o)
            {
                Log("Failed to Network Connect");
                NoticePopup.content = MessageCommon.Get("network.failed");
                Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
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
        float m_NetworkMaxWaitTime = 10f;

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

    IEnumerator CheckingTokenValue()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(m_CheckTokenValueTime);
        
        string token = appleAuth.idToken;
        string url = Core.settings.url + "/CheckToken" + token;

        UnityWebRequest request = UnityWebRequest.Get(url);
        while(true)
        {
            yield return request.SendWebRequest();

            if (request.isDone)
            {
                string value = request.downloadHandler.text;
                if(token != value)
                {
                    NoticePopup.content = MessageCommon.Get("network.duplicatelogin");
                    Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
                    request.Dispose();
                    yield return new WaitForSeconds(1f);
                    Application.Quit();
                }
            }

            yield return waitForSeconds;
        }

    }

    private void Update()
    {
        if (m_AppleAuthManager != null)
        {
            m_AppleAuthManager.Update();
        }
    }

}
