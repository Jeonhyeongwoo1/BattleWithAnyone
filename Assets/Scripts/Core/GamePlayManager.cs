using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GamePlayManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
	public enum State { None, Prepare, Prepared, RoundPlaying, RoundDone, GameDone, GameEnd }

    public string roomName { get; set; }

    State m_State;
	public State state
    {
        get => m_State;
        set => m_State = value;
    }

	public DayAndNight lightEnvironment => m_LightEnvironment;

	[SerializeField] float m_CharacterDieWaitTime = 3f;
	[SerializeField] DayAndNight m_LightEnvironment;
	int m_CurRound = 0;

	public void OnEvent(EventData photonEvent)
	{
		switch (photonEvent.Code)
		{
			case (byte)PhotonEventCode.ROUNDDONE_TIMEOUT:
				m_State = State.RoundDone;
                Core.state.masterWinCount++; //타임아웃일 경우에는 방장이 승리한다.
                break;
			case (byte)PhotonEventCode.ROUNDDONE_CHARACTER_DIE:
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
		m_CurRound = numberOfRound;
		StopAllCoroutines();
        Popups popups = Core.plugs.Get<Popups>();
		popups.StopAllCoroutines();
        Round round = popups.Get<Round>();
		round.StopAllCoroutines();
		round.Close();
		
		OnRoundDone();
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
		
		theme.SetPlayersName(PhotonNetwork.MasterClient.NickName, PhotonNetwork.IsMasterClient ? PhotonNetwork.MasterClient.NickName : PhotonNetwork.NickName);
		Core.plugs.Get<XTheme>().Open(OnGameStart);
	}

	void OnGameStart()
	{
		Log("Game Start");
		m_State = State.RoundPlaying;
		m_CurRound++;
		XTheme theme = Core.plugs.Get<XTheme>();
		theme.Crosshair.SetActive(true);
		theme.gameTimer.StartTimer();
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
		if (m_CurRound < numberOfRound)
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
        m_CurRound = 0;
        Core.state.masterWinCount = 0;
        Core.state.playerWinCount = 0;
		Core.state.totalDamangeReceived = 0;
		Core.state.totalTakeDamange = 0;
        Core.state.masterCharacter = null;
        Core.state.playerCharacter = null;
        Core.state.mapPreferences = null;
		string model = Core.models.Get().Name;
		Core.models.Unload(model);
		Core.plugs.Unload<XTheme>();

        PhotonNetwork.LeaveRoom();
		Core.networkManager.WaitStateToConnectedToMasterServer(() => Core.scenario.OnLoadScenario(nameof(ScenarioHome)));
        BattleWtihAnyOneStarter.GetBlockSkybox()?.gameObject.SetActive(true);
	}
}
