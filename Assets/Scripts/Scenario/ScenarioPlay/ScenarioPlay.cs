using UnityEngine.Events;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioPlay : MonoBehaviourPunCallbacks, IScenario
{
    public string scenarioName => typeof(ScenarioPlay).Name;

    [SerializeField] GamePlayLoading m_GameLoading;
    [SerializeField] Button m_GoHome;

    UnityAction ScenarioPrepared;
    
    public void OnScenarioPrepare(UnityAction done)
    {
        Core.plugs.DefaultEnsure();
        if (!PhotonNetwork.IsConnectedAndReady && Core.networkManager.member == null) //DEV
        {
            BattleWtihAnyOneStarter.GetLoading()?.StartLoading();
            Core.networkManager.ConnectPhotonNetwork(() => PhotonNetwork.JoinLobby());
            ScenarioPrepared = done;
            return;
        }

        m_GameLoading.Prepare();
		m_GameLoading.StartLoading(OnLoadedGame);
		ScenarioPrepared = done;
    }

    public void OnScenarioStandbyCamera(UnityAction done)
    {
        IModel model = Core.models.Get();
        if (model == null)
        {
            Debug.LogError("Un loaded Map!!");
            done?.Invoke();
			return;
		}

		StartCoroutine(model.ShootingCamera(PhotonNetwork.IsMasterClient, done));
	}

	public void OnScenarioStart(UnityAction done)
    {
        Core.gameManager.OnGamePrepare();
		done?.Invoke();
    }

    public void OnScenarioStop(UnityAction done)
    {
        done?.Invoke();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Core.scenario.OnLoadScenario(nameof(ScenarioHome));
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Core.scenario.OnLoadScenario(nameof(ScenarioHome));
    }

    public override void OnJoinedLobby()
    {
        DevPhotonNetwork dev = new DevPhotonNetwork();
        dev.CreateRoom();
    }

    public override void OnCreatedRoom()
    {
        BattleWtihAnyOneStarter.GetLoading()?.StopLoading();
        m_GameLoading.Prepare();
        m_GameLoading.StartLoading(OnLoadedGame);
    }

    void OnLoadedGame()
    {
        Debug.Log("OnLoaded Game");
        m_GameLoading.EndLoading();
        ScenarioPrepared?.Invoke();
        
    }

    void GoHome()
    {
        PhotonNetwork.Disconnect();
        Core.scenario.OnLoadScenario(nameof(ScenarioHome));
    }

    private void Start()
    {
        Core.Ensure(() => Core.scenario.OnLoadedScenario(this));
    }

    private void Awake()
    {
        Core.Ensure(() => Core.scenario.OnScenarioAwaked(this));
        m_GoHome.onClick.AddListener(GoHome);
    }

}
