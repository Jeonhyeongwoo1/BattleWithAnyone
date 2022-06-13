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
            Core.networkManager.OnDisconnectedAppleCredential();
        });

        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);
        Core.networkManager.appleAuthManager = appleAuthManager;
        appleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                var appleIdCredential = credential as IAppleIDCredential;
                AppleLoginAuth appleAuth = new AppleLoginAuth();
                appleAuth.appleUser = appleIdCredential.User;
                appleAuth.email = appleIdCredential.Email;
                appleAuth.nickName = appleIdCredential.FullName?.Nickname;
                appleAuth.authCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);
                appleAuth.idToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                Core.networkManager.appleAuth = appleAuth;
                Core.networkManager.ReqLoginAppleAuth(appleAuth.appleUser, appleAuth.authCode, appleAuth.email, appleAuth.nickName, LoginSuccessed, LoginFailed);
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

    void LoginSuccessed(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            NoticePopup.content = Core.language.GetNotifyMessage("login.apple.failed");
            Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
            return;
        }

        if (data != "success")
        {
            Member member = new Member();
            member.mbr_nm = data;
            Core.networkManager.member = member;
        }

        Core.audioManager.StopBackground();
        Core.scenario.OnLoadScenario(nameof(ScenarioHome));
       // Core.networkManager.ReqUpdateAppleToken(UpdateTokenSuccessed, UpdateTokenFailed);
    }

    void LoginFailed(string error)
    {
        Debug.LogError(error);
        NoticePopup.content = Core.language.GetNotifyMessage("login.apple.failed");
        Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
    }

/*
    void SignupSuccessed(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            SignupFailed(null);
            return;
        }

        ConfirmPopup.content = Core.language.GetNotifyMessage("signup.signupsucceesd");
        Core.plugs.Get<Popups>().OpenPopupAsync<ConfirmPopup>();
        Core.networkManager.ReqLoginAppleAuth(Core.networkManager.appleAuth.appleUser, LoginSuccessed, LoginFailed);
    }

    void SignupFailed(string data)
    {
        NoticePopup.content = Core.language.GetNotifyMessage("signup.signupfailed");
        Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
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
*/
}
#endif