using UnityEngine.Events;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioPlay : MonoBehaviourPunCallbacks, IScenario
{
    public string scenarioName => typeof(ScenarioPlay).Name;

    [SerializeField] GamePlayLoading m_GamePlayLoading;
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

		string title = Core.gameManager.GetMapPreference().mapName;
		string playerName = PhotonNetwork.IsMasterClient ? Core.gameManager.playerName : Core.networkManager.member.mbr_id;
		string masterName = PhotonNetwork.IsMasterClient ? Core.networkManager.member.mbr_id : PhotonNetwork.MasterClient.NickName;

		if (!m_GamePlayLoading.gameObject.activeSelf)
		{
			m_GamePlayLoading.gameObject.SetActive(true);
		}

		m_GamePlayLoading.SetInfo(title, masterName, playerName);
		m_GamePlayLoading.StartGameLoading(OnLoadedGame);
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
        Core.gameManager.GamePrepare();
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

        string title = Core.gameManager.GetMapPreference().mapName;
		string playerName = PhotonNetwork.IsMasterClient ? Core.gameManager.playerName : Core.networkManager.member.mbr_id;
		string masterName = PhotonNetwork.IsMasterClient ? Core.networkManager.member.mbr_id : Core.gameManager.playerName;

		if (!m_GamePlayLoading.gameObject.activeSelf)
        {
            m_GamePlayLoading.gameObject.SetActive(true);
        }

        m_GamePlayLoading.SetInfo(title, masterName, playerName);
        m_GamePlayLoading.StartGameLoading(OnLoadedGame);
    }

    void OnLoadedGame()
    {
        Debug.Log("OnLoaded Game");
        m_GamePlayLoading.StopGameLoading();
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
