using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScenarioLogin : MonoBehaviour, IScenario
{
    public string scenarioName => typeof(ScenarioLogin).Name;

    [SerializeField] RectTransform m_LoginForm;
    [SerializeField] InputField m_Id;
    [SerializeField] InputField m_Password;
    [SerializeField] Button m_LoginBtn;
    [SerializeField] Signup m_Signup;
    [SerializeField] FindMember m_FindMember;
    [SerializeField] AppleAuthLogin m_AppleLogin;

    public void OnScenarioPrepare(UnityAction done)
    {
        BattleWtihAnyOneStarter.GetBlockSkybox()?.gameObject.SetActive(false);
        Core.plugs.DefaultEnsure();
        done?.Invoke();
    }

    public void OnScenarioStandbyCamera(UnityAction done)
    {
        done?.Invoke();
    }

    public void OnScenarioStart(UnityAction done)
    {
        m_LoginForm.gameObject.SetActive(true);
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
            Core.networkManager.SetPlayerName(Core.settings.user.Get().id);
            Core.networkManager.isLogined = true;
            Core.scenario.OnLoadScenario(nameof(ScenarioHome));
            return;
        }

        if (string.IsNullOrEmpty(id))
        {
            m_Id.ActivateInputField();
            Debug.Log("Input id");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            m_Password.ActivateInputField();
            Debug.Log("Input password");
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
        Core.networkManager.SetPlayerName(member.mbr_id);
        Core.networkManager.isLogined = true;
        Core.scenario.OnLoadScenario(nameof(ScenarioHome));
    }

    void LoginFailed(string error)
    {
        //Error
        m_Id.text = null;
        m_Password.text = null;
    }

    void OnAppleLoginSuccessed()
    {
        Core.networkManager.SetPlayerName(null);
        Core.networkManager.isLogined = true;
        Core.scenario.OnLoadScenario(nameof(ScenarioHome));
    }

    private void Start()
    {
        Core.Ensure(() => Core.scenario.OnLoadedScenario(this));
    }

    private void Awake()
    {
        Core.Ensure(() => Core.scenario.OnScenarioAwaked(this));
        m_LoginBtn.onClick.AddListener(OnLogin);
        m_AppleLogin.GetComponent<Button>().onClick.AddListener(() => m_AppleLogin.Login(OnAppleLoginSuccessed));
    }
}
