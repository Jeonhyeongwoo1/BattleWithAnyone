using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;

public class ScenarioHome : MonoBehaviourPunCallbacks, IScenario
{
    public string scenarioName => typeof(ScenarioHome).Name;
    public UserInfo userInfo;
    public RoomMenu roomMenu;

    [SerializeField] Button m_Exit;
    [SerializeField] RectTransform m_LoginForm;
    [SerializeField] InputField m_Id;
    [SerializeField] InputField m_Password;
    [SerializeField] Button m_LoginBtn;

    public void OnScenarioPrepare(UnityAction done)
    {
        BattleWtihAnyOneStarter.GetBlockSkybox()?.gameObject.SetActive(false);
        BattleWtihAnyOneStarter.GetLoading()?.StartLoading();
        Core.plugs.DefaultEnsure();
        done?.Invoke();
    }

    public void OnScenarioStandbyCamera(UnityAction done)
    {
        done?.Invoke();
    }

    public void OnScenarioStart(UnityAction done)
    {
        if (Core.networkManager.isLogined)
        {
            StartCoroutine(WaitingForNetworkConnection(() => PhotonNetwork.JoinLobby()));
            done?.Invoke();
            return;
        }

        Core.networkManager.ConnectPhotonNetwork(() => ConnectedPhotonNetwork(null));
        done?.Invoke();
    }

    void ConnectedPhotonNetwork(UnityAction done)
    {
        BattleWtihAnyOneStarter.GetLoading()?.StopLoading();
        m_Exit.gameObject.SetActive(true);
        roomMenu.gameObject.SetActive(true);
        m_LoginForm.gameObject.SetActive(true);

        done?.Invoke();
    }

    public void OnScenarioStop(UnityAction done)
    {
        StopAllCoroutines();
        done?.Invoke();
    }

    void OnLogin()
    {

        string id = m_Id.text;
        string password = m_Password.text;

		if (Core.settings.profile == XSettings.Profile.local)
		{
            XSettings.User user = Core.settings.user;
            id = user.Get().id;
            password = user.Get().password;
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
        userInfo.SetUserInfo(member.mbr_id);
        Core.networkManager.isLogined = true;
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        BattleWtihAnyOneStarter.GetLoading()?.StopLoading();
        roomMenu.OnEnableRoomMenu();
        userInfo.gameObject.SetActive(true);
        m_LoginForm.gameObject.SetActive(false);
    }

    void LoginFailed(string error)
    {
        //Error
        m_Id.text = null;
        m_Password.text = null;

    }

    IEnumerator WaitingForNetworkConnection(UnityAction done)
    {
        while (PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.ConnectedToMasterServer)
        {
            yield return null;
        }

        done?.Invoke();
    }

    private void Start()
    {
        Core.Ensure(() => Core.scenario.OnLoadedScenario(this));
    }

    private void Awake()
    {
        Core.Ensure(() => Core.scenario.OnScenarioAwaked(this));
        m_LoginBtn.onClick.AddListener(OnLogin);
    }

}