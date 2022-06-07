using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScenarioLogin : MonoBehaviour, IScenario
{
    public string scenarioName => nameof(ScenarioLogin);

    [SerializeField] RectTransform m_LoginForm;
    [SerializeField] InputField m_Id;
    [SerializeField] InputField m_Password;
    [SerializeField] Button m_LoginBtn;
    [SerializeField] Signup m_Signup;
    [SerializeField] FindMember m_FindMember;
    [SerializeField] Button m_Language;
#if UNITY_IOS
    [SerializeField] AppleAuthLogin m_AppleLogin;
#endif

    public void OnScenarioPrepare(UnityAction done)
    {
        BattleWtihAnyOneStarter.GetBlockSkybox()?.gameObject.SetActive(false);
        BattleWtihAnyOneStarter.GetLoading()?.StopLoading();
        Core.plugs.DefaultEnsure();
        TouchInput.use = true;
        done?.Invoke();
    }

    public void OnScenarioStandbyCamera(UnityAction done)
    {
        done?.Invoke();
    }

    public void OnScenarioStart(UnityAction done)
    {
        m_LoginForm.gameObject.SetActive(true);
        Core.audioManager.PlayBackground(AudioManager.BackgroundType.LOGIN);
        done?.Invoke();
    }

    public void OnScenarioStop(UnityAction done)
    {
        done?.Invoke();
    }

    public void OpenSignup()
    {
        m_Signup.Open();
    }

    public void OpenFindMember()
    {
        m_FindMember.Open();
    }

    void OnLogin()
    {
        string id = m_Id.text;
        string password = m_Password.text;

        if (Core.settings.profile == XSettings.Profile.local)
        {
            Core.networkManager.member = MemberFactory.Get();
            Core.scenario.OnLoadScenario(nameof(ScenarioHome));
            return;
        }

        Popups popups = Core.plugs.Get<Popups>();
        NoticePopup notice = popups.Get<NoticePopup>();

        if (string.IsNullOrEmpty(id))
        {
            m_Id.ActivateInputField();
            NoticePopup.content = Core.language.GetNotifyMessage("login.checkid");
            Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            m_Password.ActivateInputField();
            NoticePopup.content = Core.language.GetNotifyMessage("login.checkpw");
            Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
            return;
        }

        Core.networkManager.ReqLogin(id, password, LoginSuccessed, LoginFailed);
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

    void LoginFailed(string error)
    {
        //Error
        m_Id.text = null;
        m_Password.text = null;

        NoticePopup.content = Core.language.GetNotifyMessage("login.failed");
        Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
    }

    private void Start()
    {
        Core.Ensure(() => Core.scenario.OnLoadedScenario(this));
    }

    void OpenLanguagePopup()
    {
        Popups popups = Core.plugs.Get<Popups>();
        if (popups == null) { return; }
        if (!popups.IsOpened<LanguagePopup>())
        {
            popups.OpenPopupAsync<LanguagePopup>();
        }
    }

    private void Awake()
    {
        Core.Ensure(() => Core.scenario.OnScenarioAwaked(this));
        m_LoginBtn.onClick.AddListener(OnLogin);
        m_Language.onClick.AddListener(OpenLanguagePopup);
#if UNITY_IOS
        m_AppleLogin.GetComponent<Button>().onClick.AddListener(() => m_AppleLogin.Login());
#endif
    }
}
