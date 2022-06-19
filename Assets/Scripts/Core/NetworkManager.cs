using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Events;
using UnityEngine.Networking;
using AppleAuth;
using System;
using System.Text;
using System.Security.Cryptography;
using AppleAuth.Enums;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    AppleLoginAuth m_AppleAuth;
    public AppleLoginAuth appleAuth
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

    bool m_StopAppleCredentialInspection;
    public bool stopAppleCredentialInspection
    {
        get => m_StopAppleCredentialInspection;
        set => m_StopAppleCredentialInspection = value;
    }

    bool m_StopTokenInspection;
    public bool stopTokenInspection
    {
        get => m_StopTokenInspection;
        set => m_StopTokenInspection = value;
    }

    [SerializeField] float m_CheckTokenValueTime = 30f;
    [SerializeField] float m_CheckAppleAuthCredentialTime = 180f;
 
    public void Log(string message)
    {
        if (XSettings.networkManagerLog)
        {
            Debug.Log(message);
        }
    }

    public void ReqUpdateUserName(string id, string name, UnityAction<string> success, UnityAction<string> fail)
    {
        string url = Core.settings.url + "/updateUserName";

        WWWForm form = new WWWForm();
        form.AddField("appleId", id);
        form.AddField("userName", name);

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        Core.dispatcher.Request(() => StartCoroutine(RequestData(request, success, fail)));
        //StartCoroutine(RequestData(request, success, fail));
    }

    public void ReqUpdateAppleToken(UnityAction<string> success, UnityAction<string> fail)
    {
        string url = Core.settings.url + "/appleUpdateToken";

        WWWForm form = new WWWForm();
        form.AddField("token", appleAuth.idToken);
        form.AddField("id", appleAuth.appleUser);
        
        UnityWebRequest request = UnityWebRequest.Post(url, form);
        Core.dispatcher.Request(() => StartCoroutine(RequestData(request, success, fail)));
        //StartCoroutine(RequestData(request, success, fail));
    }

    public void ReqLoginAppleAuth(string appleId, string authCode, string email, string nickName, UnityAction<string> success, UnityAction<string> fail)
    {
        string url = Core.settings.url + "/appleLogin";

        WWWForm form = new WWWForm();
        form.AddField("appleId", appleId);
        form.AddField("authCode", authCode);
        if (!string.IsNullOrEmpty(email))
        {
            form.AddField("email", email);
        }

        if(!string.IsNullOrEmpty(nickName))
        {
            form.AddField("name", nickName);
        }

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        Core.dispatcher.Request(() => StartCoroutine(RequestData(request, success, fail)));
        //StartCoroutine(RequestData(request, success, fail));
    }

    public void ReqFindId(string email, UnityAction<string> success, UnityAction<string> fail)
    {
        string url = Core.settings.url + "/findId/" + email;

        UnityWebRequest request = UnityWebRequest.Get(url);
        Core.dispatcher.Request(() => StartCoroutine(RequestData(request, success, fail)));
        //StartCoroutine(RequestData(request, success, fail));
    }

    public void ReqFindPassword(string id, string email, UnityAction<string> success, UnityAction<string> fail)
    {
        string url = Core.settings.url + "/findPw/" + id + "/" + email;

        UnityWebRequest request = UnityWebRequest.Get(url);
        Core.dispatcher.Request(() => StartCoroutine(RequestData(request, success, fail)));
        //StartCoroutine(RequestData(request, success, fail));
    }

    public void ReqUpdatePassword(string email, string password, UnityAction<string> success, UnityAction<string> fail)
    {
        string url = Core.settings.url + "/updatePw";

        string pw = GetSHA256Hash(password);
        if (string.IsNullOrEmpty(pw))
        {
            pw = password;
        }

        WWWForm form = new WWWForm();
        form.AddField("pw", pw);
        form.AddField("email", email);

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        Core.dispatcher.Request(() => StartCoroutine(RequestData(request, success, fail)));
        //StartCoroutine(RequestData(request, success, fail));
    }

    public void ReqCheckUserId(string id, UnityAction<string> success, UnityAction<string> fail)
    {
        string url = Core.settings.url + "/checkId/" + id;

        UnityWebRequest request = UnityWebRequest.Get(url);
        Core.dispatcher.Request(() => StartCoroutine(RequestData(request, success, fail)));
        //StartCoroutine(RequestData(request, success, fail));
    }

    public void ReqSignup(string id, string password, string email, string name, UnityAction<string> success, UnityAction<string> fail)
    {
        string url = Core.settings.url + "/signup";
        string pw = GetSHA256Hash(password);
        if(string.IsNullOrEmpty(pw))
        {
            pw = password;
        }

        WWWForm form = new WWWForm();
        form.AddField("id", id);
        form.AddField("pw", pw);
        form.AddField("email", email);
        form.AddField("name", name);

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        Core.dispatcher.Request(() => StartCoroutine(RequestData(request, success, fail)));
        //StartCoroutine(RequestData(request, success, fail));
    }

    public void ReqLogin(string id, string password, UnityAction<string> success, UnityAction<string> fail)
    {
        string url = Core.settings.url + "/login";
        string pw = GetSHA256Hash(password);
        if(string.IsNullOrEmpty(pw))
        {
            pw = password;
        }

        WWWForm form = new WWWForm();
        form.AddField("id", id);
        form.AddField("password", pw);

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        Core.dispatcher.Request(() => StartCoroutine(RequestData(request, success, fail)));
        //StartCoroutine(RequestData(request, success, fail));
    }

    public void ConnectPhotonNetwork(UnityAction done)
    {
        if (PhotonNetwork.IsConnected)
        {
            NoticePopup.content = Core.language.GetNotifyMessage("network.alreadyconnected");
            Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
            return;
        }

        StartCoroutine(ConnectingNetwork(done));
    }

    public void WaitStateToConnectedToMasterServer(UnityAction done)
    {
        StartCoroutine(WaitingConnectedToMasterServer(done));
    }

    public void OnDisconnectedAppleCredential()
    {
        StartCoroutine(WaitScenarioReady(OnDisconnectedAppleAuth));
    }

    public void VerifyValidateAuth()
    {
        if (m_AppleAuthManager != null)
        {
            StartCoroutine(VerifyValidateAppleCredential());
            return;
        }

        StartCoroutine(VerfiyValidateAccessToken());
    }

    public async void RequestDataAsync(UnityWebRequest request, UnityAction<string> success, UnityAction<string> fail)
    {
        await Core.dispatcher.RequestAsync(() => StartCoroutine(RequestData(request, success, fail)));
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Log("DisConnected" + cause);
        ConfirmPopup.content = Core.language?.GetNotifyMessage("network.disconnect");
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
                NoticePopup.content = Core.language.GetNotifyMessage("room.existroom");
                Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
                break;
            case (int)PhotonCode.OPERATION_NOTALLOWED_INCURRENT_STATE:
                ConfirmPopup.content = Core.language.GetNotifyMessage("pun.states.waiting");
                Core.plugs.Get<Popups>()?.OpenPopupAsync<ConfirmPopup>();
                break;
            default:
                Core.state.mapPreferences = null;
                NoticePopup.content = Core.language.GetNotifyMessage("room.createfailed");
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
                NoticePopup.content = Core.language.GetNotifyMessage("room.gamefull");
                Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
                break;
            default:
                NoticePopup.content = Core.language.GetNotifyMessage("room.joinroomfailed");
                Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
                break;
        }
    }

    string GetSHA256Hash(string value)
    {
        SHA256 sha = new SHA256Managed();
        byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(value));
        StringBuilder stringBuilder = new StringBuilder();
        foreach (byte b in hash)
        {
            stringBuilder.AppendFormat("{0:x2}", b);
        }

        return stringBuilder.ToString();
    }

    void OnDisconnectedAppleAuth()
    {
        NoticePopup.content = Core.language.GetNotifyMessage("network.disconnect");
        Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
        PhotonNetwork.Disconnect();

        IScenario current = Core.scenario.currentScenario;
        if (current?.scenarioName == nameof(ScenarioPlay))
        {
            Core.gameManager.InitForGamePlay();
        }

        m_AppleAuthManager = null;
        Core.scenario.OnLoadScenario(nameof(ScenarioLogin));
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
                NoticePopup.content = Core.language.GetNotifyMessage("network.failed");
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
                NoticePopup.content = Core.language.GetNotifyMessage("network.failed");
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

    IEnumerator VerifyValidateAppleCredential()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(m_CheckAppleAuthCredentialTime);
        string userId = m_AppleAuth.appleUser;
        stopAppleCredentialInspection = false;
        while (!stopAppleCredentialInspection)
        {
            if (m_AppleAuthManager == null) { yield break; }

            m_AppleAuthManager?.GetCredentialState(userId,
                state =>
                        {
                            switch (state)
                            {
                                case CredentialState.Authorized:
                                    // User ID is still valid. Login the user.
                                    break;
                                case CredentialState.Revoked:
                                case CredentialState.NotFound:
                                    // User ID was not found. Go to login screen.
                                    if(!ScenarioDirector.scenarioReady)
                                    {
                                        StartCoroutine(WaitScenarioReady(OnDisconnectedAppleAuth));
                                        break;
                                    }
                                    
                                    OnDisconnectedAppleAuth();
                                    break;
                            }
                        },
                error =>
                        {
                            // Something went wrong
                            if (!ScenarioDirector.scenarioReady)
                            {
                                StartCoroutine(WaitScenarioReady(OnDisconnectedAppleAuth));
                                return;
                            }

                            OnDisconnectedAppleAuth();
                        });

            yield return waitForSeconds;
        }
    }

    IEnumerator WaitScenarioReady(UnityAction done)
    {
        while (!ScenarioDirector.scenarioReady) { yield return null; }
        done?.Invoke();
    }

    IEnumerator VerfiyValidateAccessToken()
    {
        string token = member.mbr_token;
        if (string.IsNullOrEmpty(token)) { yield break; }

        WaitForSeconds waitForSeconds = new WaitForSeconds(m_CheckTokenValueTime);
        string url = Core.settings.url + "/checkToken/" + token;
        int errorCount = 0;
        stopTokenInspection = false;

        while(!stopTokenInspection)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (errorCount > 3)
                {
                    NoticePopup.content = Core.language.GetNotifyMessage("network.duplicatelogin");
                    Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
                    Application.Quit();
                    yield break;
                }

                errorCount++;
            }

            if (request.isDone)
            {
                string value = request.downloadHandler.text;
                print(value);
                if(string.IsNullOrEmpty(value) || value != token)
                {
                    if (errorCount > 3)
                    {
                        ConfirmPopup.content = Core.language.GetNotifyMessage("network.duplicatelogin");
                        Core.plugs.Get<Popups>()?.OpenPopupAsync<ConfirmPopup>();
                        yield return new WaitForSeconds(5f);
                        Application.Quit();
                    }
                    
                    errorCount++;
                }

                //success
            }

            request.Dispose();
            yield return waitForSeconds;
        }
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

    void OnApplicationQuit()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
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
