#if UNITY_IOS
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using System.Collections;
using UnityEngine;
using System.Text;
using System.Security.Cryptography;
using UnityEngine.Events;

public class AppleAuthLogin : MonoBehaviour
{
    public string appleUser;
    public string idToken;
    public string authCode;
    public string rawNonce;
    public string nonce;

    IAppleAuthManager m_AppleAuthManager;

    public void Login(UnityAction done)
    {
        if (!AppleAuthManager.IsCurrentPlatformSupported)
        {
            NoticePopup.content = MessageCommon.Get("login.apple.platfromsupport");
            Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
            return;
        }

        var deserializer = new PayloadDeserializer();
        m_AppleAuthManager = new AppleAuthManager(deserializer);

        if (m_AppleAuthManager == null) { return; }

        m_AppleAuthManager.SetCredentialsRevokedCallback(result =>
        {
            NoticePopup.content = MessageCommon.Get("login.apple.failed");
            Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
            Debug.LogError("Received revoke callback " + result);
        });

        StartCoroutine(LoginProcess(done));
    }

    // Nonce는 SHA256으로 만들어서 전달해야함
    private static string GenerateNonce(string _rawNonce)
    {
        SHA256 sha = new SHA256Managed();
        StringBuilder sb = new StringBuilder();
        byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(_rawNonce));
        foreach (var b in hash) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    IEnumerator LoginProcess(UnityAction done)
    {
        var quickLoginArgs = new AppleAuthQuickLoginArgs();
        bool isSuccessed = false;
        bool isDone = false;

        rawNonce = System.Guid.NewGuid().ToString();
        nonce = GenerateNonce(rawNonce);

        m_AppleAuthManager.QuickLogin(
            quickLoginArgs,
            credential =>
            {
                var appleIdCredential = credential as IAppleIDCredential;
                if (appleIdCredential != null)
                {
                    appleUser = appleIdCredential.User;
                    authCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);
                    idToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                    isSuccessed = true;
                }
                else
                {
                    isSuccessed = false;
                }

                isDone = true;
            },
            error =>
            {
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
                NoticePopup.content = MessageCommon.Get("login.apple.failed");
                Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
                Debug.LogError("Quick Login Failed " + authorizationErrorCode.ToString() + " " + error.ToString());
                isSuccessed = false;
                isDone = true;
            });

        while (!isDone) { yield return null; }

        if (isSuccessed)
        {
            Core.networkManager.member = MemberFactory.Get();
            done?.Invoke();
            yield break;
        }

        isDone = false;

        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

        m_AppleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                var appleIdCredential = credential as IAppleIDCredential;
                if (appleIdCredential != null)
                {
                    appleUser = appleIdCredential.User;
                    authCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);
                    idToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                    isSuccessed = true;
                }
                else
                {
                    isSuccessed = false;
                }

            },
            error =>
            {
                NoticePopup.content = MessageCommon.Get("login.apple.failed");
                Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
                Debug.LogWarning("Quick Login Failed " + authorizationErrorCode.ToString() + " " + error.ToString());
                isSuccessed = false;
                isDone = true;
            });

        while (!isDone) { yield return null; }

        if (isSuccessed)
        {
            Core.networkManager.member = MemberFactory.Get();
            done?.Invoke();
            yield break;
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
#endif