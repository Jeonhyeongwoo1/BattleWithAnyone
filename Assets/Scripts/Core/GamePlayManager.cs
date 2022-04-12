using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GamePlayManager : MonoBehaviourPunCallbacks
{
	public enum Status { None, Prepare, Prepared, RoundPlaying, RoundDone, GameDone, GameEnd }

	public string roomName { get; set; }

	Status m_Status;
	int m_CurRound = 0;

	public Status GetState() => m_Status;
	public void SetState(Status status) => m_Status = status;

	public void Log(string message)
	{
		Debug.Log(message);
	}

	//GamePlay Routine
	public void OnGamePrepare()
	{
		Log("Getting ready to play");
		m_Status = Status.Prepare;
		Popups popups = Core.plugs.Get<Popups>();
		popups.OpenPopupAsync<Round>(() =>
		{
			Round round = popups.Get<Round>();
			round.ShowRoundInfo(OnGamePrepared);
		});
	}

	void OnGamePrepared()
	{
		Log("Game ready");
		m_Status = Status.Prepared;
		XState.MapPreferences map = Core.state.mapPreferences;
		XTheme theme = Core.plugs.Get<XTheme>();
		theme.roundTime = map.roundTime;
		theme.SetPlayersName(PhotonNetwork.MasterClient.NickName, PhotonNetwork.IsMasterClient ? PhotonNetwork.MasterClient.NickName : PhotonNetwork.NickName);

		Core.plugs.Get<XTheme>().Open(OnGameStart);
	}

	void OnGameStart()
	{
		Log("Game Start");
		m_Status = Status.RoundPlaying;
		m_CurRound++;
		XTheme theme = Core.plugs.Get<XTheme>();
		theme.gameTimer.StartTimer();
		StartCoroutine(GamePlaying(OnRoundDone));
	}

	IEnumerator GamePlaying(UnityAction done)
	{
		Log("Game Playing");

		while (m_Status == Status.RoundPlaying) { yield return null; }

		done?.Invoke();
	}

	void OnRoundDone()
	{
		Log("Round Done");
		m_Status = Status.RoundDone;
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
		m_Status = Status.GameDone;
		int winCount = PhotonNetwork.IsMasterClient ? Core.state.masterWinCount : Core.state.playerWinCount;
		Core.xEvent.Raise(winCount == Core.state.mapPreferences.numberOfRound ? GameEndUI.Victory : GameEndUI.Lose, true);
		StartCoroutine(WaitingGameEnd(GameEnd));
	}

	IEnumerator WaitingGameEnd(UnityAction done)
	{
		while (m_Status == Status.GameDone) { yield return null; }
		done?.Invoke();
	}

	void GameEnd()
	{
		Log("Game End");
		PhotonNetwork.LeaveRoom();
		Core.networkManager.WaitStateToConnectedToMasterServer(() => Core.scenario.OnLoadScenario(nameof(ScenarioHome)));
	}

}
