#if UNITY_IOS
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using UnityEngine;
using System.Text;
using System;

public class AppleAuthLogin : MonoBehaviour
{
    [SerializeField] AppleSignup m_Signup;

    public void Login()
    {
        if (!AppleAuthManager.IsCurrentPlatformSupported)
        {
            NoticePopup.content = Core.language.GetNotifyMessage("login.apple.platfromsupport");
            Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
            return;
        }

        var deserializer = new PayloadDeserializer();
        IAppleAuthManager appleAuthManager = new AppleAuthManager(deserializer);
        appleAuthManager.SetCredentialsRevokedCallback(result =>
        {
            NoticePopup.content = Core.language.GetNotifyMessage("login.apple.failed");
            Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
            Debug.LogError("Received revoke callback " + result);
        });

        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);
        Core.networkManager.appleAuthManager = appleAuthManager;
        appleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                var appleIdCredential = credential as IAppleIDCredential;
                NetworkManager.AppleAuth appleAuth = new NetworkManager.AppleAuth();
                appleAuth.appleUser = appleIdCredential.User;
                appleAuth.authCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);
                appleAuth.idToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                Core.networkManager.appleAuth = appleAuth;
                Core.networkManager.ReqLoginAppleAuth(appleAuth.appleUser, LoginSuccessed, LoginFailed);
            },
            error =>
            {
                NoticePopup.content = Core.language.GetNotifyMessage("login.apple.failed");
                Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
                Core.networkManager.appleAuthManager = null;
                Debug.LogError("Apple Login Error : " + authorizationErrorCode);
            });
    }

    public void LoginSuccessed(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            NoticePopup.content = Core.language.GetNotifyMessage("find.failedmember");
            Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
            return;
        }

        try
        {
            Member member = JsonUtility.FromJson<Member>(data);
            Core.networkManager.member = member;
        }
        catch (Exception e)
        {
            NoticePopup.content = Core.language.GetNotifyMessage("login.apple.failed");
            Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
            Debug.LogError("Apple Login Error: " + e);
            return;
        }

        Core.networkManager.ReqUpdateAppleToken(Core.networkManager.appleAuth.idToken, Core.networkManager.appleAuth.appleUser, UpdateTokenSuccessed, UpdateTokenFailed);
    }

    void UpdateTokenFailed(string error)
    {
        Core.networkManager.member = null;
        NoticePopup.content = Core.language.GetNotifyMessage("login.apple.failed");
        Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
        Debug.LogError("Apple Login Failed : " + error);
    }

    void UpdateTokenSuccessed(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            NoticePopup.content = Core.language.GetNotifyMessage("login.apple.failed");
            Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
            Debug.LogError("Apple Login Failed : " + data);
            return;
        }

        Core.audioManager.StopBackground();
        Core.scenario.OnLoadScenario(nameof(ScenarioHome));
    }

    public void LoginFailed(string error)
    {
        //Create new memeber
        NoticePopup.content = Core.language.GetNotifyMessage("login.inputemailandname");
        Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
        m_Signup.Open();
    }

}
#endif