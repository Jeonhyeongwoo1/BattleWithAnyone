using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class ScenarioPlay : MonoBehaviourPunCallbacks, IScenario
{
    public string scenarioName => nameof(ScenarioPlay);

    [SerializeField] GamePlayLoading m_GameLoading;

    UnityAction ScenarioPrepared;
    private bool m_IsScenarioStarted = false;

    public void OnScenarioPrepare(UnityAction done)
    {   
        Core.plugs.DefaultEnsure();
        Core.audioManager.StopBackground();
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

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        StartCoroutine(WaitUntilGameStart(() => OnGameEnd(otherPlayer.NickName)));
    }

    void OnGameEnd(string name)
    {
        if (Core.gameManager.state != GamePlayManager.State.GameDone)
        {
            NoticePopup.content = string.Format(MessageCommon.Get("game.player.leftroom"), name);
            Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>(() => Core.gameManager.OnGameDoneByPlayerLeftRoom());
        }
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
    }

    IEnumerator WaitUntilGameStart(UnityAction done)
    {
        while (!m_IsScenarioStarted) { yield return null; }
        done?.Invoke();
    }

}
