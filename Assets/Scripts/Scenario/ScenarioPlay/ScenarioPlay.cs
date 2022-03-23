using UnityEngine.Events;
using Photon.Pun;
using UnityEngine;

public class ScenarioPlay : MonoBehaviourPunCallbacks, IScenario
{
    public string scenarioName => typeof(ScenarioPlay).Name;

    [SerializeField] GamePlayLoading m_GamePlayLoading;

    UnityAction ScenarioPrepared;

    public void OnScenarioPrepare(UnityAction done)
    {
        Core.plugs.DefaultEnsure();

        if (!PhotonNetwork.IsConnectedAndReady && !Core.networkManager.isLogined) //DEV
        {
            BattleWtihAnyOneStarter.GetLoading()?.StartLoading();
            Core.networkManager.ConnectPhotonNetwork(() => PhotonNetwork.JoinLobby());
            ScenarioPrepared = done;
            return;
        }

          done?.Invoke();
    }

    public void OnScenarioStandbyCamera(UnityAction done)
    {
        done?.Invoke();
    }

    public void OnScenarioStart(UnityAction done)
    {
        //Core.gameManager.OnPrepareGamePlay();
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
        ScenarioPrepared?.Invoke();

        string title = Core.gameManager.GetMapPreference().mapName;
        string playerName = Core.gameManager.playerName;
        string masterName = Core.networkManager.userNickName;
        
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
        
           
    }

    private void Start()
    {
        Core.Ensure(() => Core.scenario.OnLoadedScenario(this));
    }

    private void Awake()
    {
        Core.Ensure(() => Core.scenario.OnScenarioAwaked(this));
    }

}
