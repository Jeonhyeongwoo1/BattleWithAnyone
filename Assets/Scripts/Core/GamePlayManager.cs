using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;

public class GamePlayManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
	public enum State { None, Prepare, Prepared, RoundPlaying, RoundDone, GameDone, GameEnd }

    public string roomName { get; set; }

    string m_PlayerName;
    public string playerName
    {
        get => m_PlayerName;
        set => m_PlayerName = value;
    }

    State m_State;
	public State state
    {
        get => m_State;
        set => m_State = value;
    }

    UnityAction m_OnGameStarted;
    public UnityAction onGameStarted
    {
        get => m_OnGameStarted;
        set => m_OnGameStarted = value;
    }

	public DayAndNight lightEnvironment => m_LightEnvironment;

	[SerializeField] float m_CharacterDieWaitTime = 3f;
	[SerializeField] DayAndNight m_LightEnvironment;

	public void OnEvent(EventData photonEvent)
	{
		switch (photonEvent.Code)
		{
			case (byte)PhotonEventCode.ROUNDDONE_TIMEOUT:
				m_State = State.RoundDone;
                Core.state.masterWinCount++; //타임아웃일 경우에는 방장이 승리한다.
                break;
			case (byte)PhotonEventCode.ROUNDDONE:
                object data = photonEvent.CustomData;
                bool isMaster = (bool)data;
                if (isMaster)
                {
                    Core.state.playerWinCount++;
                }
                else
                {
                    Core.state.masterWinCount++;
                }

                StartCoroutine(WaitTime(m_CharacterDieWaitTime, () => m_State = State.RoundDone));
                break;
		}
	}
	
	void Log(string message)
	{
		if(XSettings.gamePlayManagerLog)
		{
            Debug.Log(message);
		}
	}

	//GamePlay Routine
	public void OnGamePrepare()
	{
		Log("Getting ready to play");
		m_State = State.Prepare;
		Popups popups = Core.plugs.Get<Popups>();
		popups.OpenPopupAsync<Round>(() =>
		{
			Round round = popups.Get<Round>();
			round.ShowRoundInfo(OnGamePrepared);
		});
	}

    public void OnGameDoneByPlayerLeftRoom()
    {
        int numberOfRound = Core.state.mapPreferences.numberOfRound;
        Core.state.masterWinCount = numberOfRound;
		StopAllCoroutines();
        Popups popups = Core.plugs.Get<Popups>();
		popups.StopAllCoroutines();
        Round round = popups.Get<Round>();
		round.StopAllCoroutines();
		round.Close();
		
		OnRoundDone();
	}

    public void OnOtherPlayerDisconnectedDuringLoading()
    {
        //강제로 제어....
        Scene scene = SceneManager.GetSceneByName(nameof(ScenarioRoom));
        if (scene != null)
        {
            SceneManager.UnloadSceneAsync(nameof(ScenarioRoom));
        }

        scene = SceneManager.GetSceneByName(nameof(ScenarioPlay));
        if (scene != null)
        {
            SceneManager.UnloadSceneAsync(nameof(ScenarioPlay));
        }

        Core.scenario.previousScenario = null;
        Core.scenario.currentScenario = null;

        //GameLoading 중에 플레이어가 나가게되면 룸을 떠나고 Home 화면으로 이동한다.
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        if (Core.models.Has())
        {
            IModel model = Core.models.Get();
            Core.models.Unload(model.Name);
        }

        NoticePopup.content = Core.language.GetNotifyMessage("game.player.disconnected");
        Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
        Core.networkManager.WaitStateToConnectedToMasterServer(() => Core.scenario.OnLoadScenario(nameof(ScenarioHome)));
        BattleWtihAnyOneStarter.GetBlockSkybox()?.gameObject.SetActive(true);
    }

    void OnGamePrepared()
	{
		Log("Game ready");
		m_State = State.Prepared;

		XState.MapPreferences map = Core.state.mapPreferences;
		XTheme theme = Core.plugs.Get<XTheme>();
		theme.roundTime = map.roundTime;

        Transform charcter = PhotonNetwork.IsMasterClient ? Core.state.masterCharacter : Core.state.playerCharacter;
        if (charcter.TryGetComponent<PlayerController>(out var player))
        {
            theme.player = player;
			theme.bullet = player.bulletCount.ToString();
        }
		
		theme.SetPlayersName(PhotonNetwork.MasterClient?.NickName, m_PlayerName);
		Core.plugs.Get<XTheme>().Open(OnGameStart);
	}

	void OnGameStart()
	{
		Log("Game Start");
		m_State = State.RoundPlaying;
		XTheme theme = Core.plugs.Get<XTheme>();
		theme.Crosshair.SetActive(true);
		theme.gameTimer.StartTimer();
		m_OnGameStarted?.Invoke();
        Core.xEvent?.Raise("Move.Stop", null);
		StartCoroutine(GamePlaying(OnRoundDone));
	}

	IEnumerator GamePlaying(UnityAction done)
	{
		Log("Game Playing");
		while (m_State == State.RoundPlaying) { yield return null; }
		done?.Invoke();
	}

	void OnRoundDone()
	{
		Log("Round Done");
		m_State = State.RoundDone;
		int numberOfRound = Core.state.mapPreferences.numberOfRound;

		XTheme theme = Core.plugs.Get<XTheme>();
		theme.Close();
		int playerWinCount = Core.state.playerWinCount;
		int masterWinCount = Core.state.masterWinCount;
		int round = playerWinCount > masterWinCount ? playerWinCount : masterWinCount;
		if (round < numberOfRound)
		{
			IModel model = Core.models.Get();
			Transform[] createPoints = model.playerCreatePoints;
			Transform tr = PhotonNetwork.IsMasterClient ? Core.state.masterCharacter : Core.state.playerCharacter;
			int index = PhotonNetwork.IsMasterClient ? 0 : 1;
            tr.SetPositionAndRotation(createPoints[index].position, createPoints[index].rotation);
            if (tr.TryGetComponent<PlayerController>(out var player))
            {
                player.photonView.RPC(nameof(player.NotifyResetState), RpcTarget.All);
            }

            OnGamePrepare();
		}
		else
		{
			//Game Done
			OnGameDone();
		}

	}

    void OnGameDone()
    {
        Log("Game Done");
        m_State = State.GameDone;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent((byte)PhotonEventCode.GAME_END, null, raiseEventOptions, SendOptions.SendReliable);

        Core.poolManager.Remove(nameof(Bullet));
        Core.poolManager.Remove(nameof(BulletCollisionEffect));
        StartCoroutine(WaitingGameEnd(GameEnd));
    }

    IEnumerator WaitingGameEnd(UnityAction done)
	{
		while (m_State == State.GameDone) { yield return null; }
		done?.Invoke();
	}

	IEnumerator WaitTime(float time, UnityAction done)
	{
		yield return new WaitForSeconds(time);
		done?.Invoke();
	}

	void GameEnd()
	{
		Log("Game End");

        m_State = State.None;
        Core.state.masterWinCount = 0;
        Core.state.playerWinCount = 0;
		Core.state.totalDamangeReceived = 0;
		Core.state.totalTakeDamange = 0;
        Core.state.masterCharacter = null;
        Core.state.playerCharacter = null;
        Core.state.mapPreferences = null;
		string model = Core.models.Get()?.Name;
		Core.models.Unload(model);

		XTheme xTheme = Core.plugs.Get<XTheme>();
		xTheme.gameTimer?.StopTimer();
		Core.plugs.Unload<XTheme>();

        PhotonNetwork.LeaveRoom();
		Core.networkManager.WaitStateToConnectedToMasterServer(() => Core.scenario.OnLoadScenario(nameof(ScenarioHome)));
        BattleWtihAnyOneStarter.GetBlockSkybox()?.gameObject.SetActive(true);
	}
}
