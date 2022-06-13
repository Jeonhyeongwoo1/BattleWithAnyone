using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AppleSignup : MonoBehaviour
{
    [SerializeField] InputField m_AppleName;
    [SerializeField] InputField m_Email;
    [SerializeField] Button m_Close;
    [SerializeField] Button m_Signup;

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        Core.networkManager.appleAuth = null;
        Core.networkManager.appleAuthManager = null;
    }

    public void OnSignup()
    {
        string name = m_AppleName.text;
        string email = m_Email.text;

        if (string.IsNullOrEmpty(name))
        {
            NoticePopup.content = Core.language.GetNotifyMessage("login.inputname");
            Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
            m_AppleName.ActivateInputField();
            return;
        }

        if (string.IsNullOrEmpty(email))
        {
            NoticePopup.content = Core.language.GetNotifyMessage("login.inputemail");
            Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
            m_Email.ActivateInputField();
            return;
        }

      //   Core.networkManager.ReqAppleSignup(email, name, SignupSuccessed, SignupFailed);
    }

    void SignupSuccessed(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            SignupFailed(null);
            return;
        }

        ConfirmPopup.content = Core.language.GetNotifyMessage("signup.signupsucceesd");
        Core.plugs.Get<Popups>().OpenPopupAsync<ConfirmPopup>();
        //Core.networkManager.ReqLoginAppleAuth(Core.networkManager.appleAuth.appleUser, LoginSuccessed, LoginFailed);
    }

    void LoginSuccessed(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            LoginFailed(null);
            return;
        }

        Member member = JsonUtility.FromJson<Member>(data);
        Core.networkManager.member = member;
        Core.audioManager.StopBackground();
        Core.scenario.OnLoadScenario(nameof(ScenarioHome));
    }

    void LoginFailed(string data)
    {
        Init();
        NoticePopup.content = Core.language.GetNotifyMessage("signup.signupfailed");
        Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
    }

    void SignupFailed(string data)
    {
        Debug.LogError(data);
        Init();
        NoticePopup.content = Core.language.GetNotifyMessage("signup.signupfailed");
        Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
    }

    void Init()
    {
        m_AppleName.text = null;
        m_Email.text = null;
    }

    private void OnEnable()
    {
        Init();
    }

    private void Awake()
    {
        m_Close.onClick.AddListener(Close);
        m_Signup.onClick.AddListener(OnSignup);
    }
}
