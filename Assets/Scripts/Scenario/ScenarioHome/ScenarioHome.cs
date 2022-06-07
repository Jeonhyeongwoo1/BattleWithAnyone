using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;

public class ScenarioHome : MonoBehaviourPunCallbacks, IScenario
{
    public string scenarioName => nameof(ScenarioHome);
    public UserInfo userInfo;
    public RoomMenu roomMenu;

    [SerializeField] Button m_Language;
    [SerializeField] Button m_Exit;
    [SerializeField] Button m_Settings;

    public void OnScenarioPrepare(UnityAction done)
    {
        BattleWtihAnyOneStarter.GetBlockSkybox()?.gameObject.SetActive(false);
        BattleWtihAnyOneStarter.GetLoading()?.StartLoading();
        Core.plugs.DefaultEnsure();
        TouchInput.use = true;
        Core.plugs.Load<HomeBackground>(() =>
        {
            Core.plugs.Get<HomeBackground>()?.Open();
            done?.Invoke();
        });
    }

    public void OnScenarioStandbyCamera(UnityAction done)
    {
        done?.Invoke();
	}

	public void OnScenarioStart(UnityAction done)
	{
        if (Core.networkManager.member != null && PhotonNetwork.IsConnected)
		{
			Core.networkManager.WaitStateToConnectedToMasterServer(() => PhotonNetwork.JoinLobby());
			done?.Invoke();
			return;
		}

		Core.networkManager.ConnectPhotonNetwork(() => Core.networkManager.WaitStateToConnectedToMasterServer(JoinLobby));
		done?.Invoke();
	}

    public void OnScenarioStop(UnityAction done)
    {
        StopAllCoroutines();
        Core.plugs.Unload<HomeBackground>();
        done?.Invoke();
    }

    public override void OnJoinedLobby()
    {
        BattleWtihAnyOneStarter.GetLoading()?.StopLoading();
        Core.plugs.Get<HomeBackground>()?.ShootStandbyCamera(() =>
        {
            roomMenu.gameObject.SetActive(true);
            roomMenu.OnEnableRoomMenu();
            userInfo.SetUserInfo(Core.networkManager.member.mbr_nm);
            userInfo.gameObject.SetActive(true);
            m_Exit.gameObject.SetActive(true);
            m_Settings.gameObject.SetActive(true);
            m_Language.gameObject.SetActive(true);
            Core.audioManager.PlayBackground(AudioManager.BackgroundType.HOME);
        });
    }

    //Local 
    void JoinLobby()
    {
        if(XSettings.Profile.local == Core.settings.profile)
        {
            Core.networkManager.member = MemberFactory.Get();
        }
       
        PhotonNetwork.NickName = Core.networkManager.member.mbr_nm;
        PhotonNetwork.JoinLobby();
    }

    void OpenSettingsPopup()
    {
        Popups popups = Core.plugs.Get<Popups>();
        if (!popups.IsOpened<GameSettings>())
        {
            popups.OpenPopupAsync<GameSettings>();
        }
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

    private void Start()
    {
        Core.Ensure(() => Core.scenario.OnLoadedScenario(this));
    }

    private void Awake()
    {
        Core.Ensure(() => Core.scenario.OnScenarioAwaked(this));
        m_Exit.onClick.AddListener(() => Application.Quit());
        m_Settings.onClick.AddListener(OpenSettingsPopup);
        m_Language.onClick.AddListener(OpenLanguagePopup);
    }

}