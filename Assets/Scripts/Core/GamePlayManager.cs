using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using Photon.Pun;

public class GamePlayManager : MonoBehaviourPunCallbacks
{
	
	public string roomName { get; set; }

	public void Log(string message)
	{
		Debug.Log(message);
	}

	//GamePlay Routine
	public void OnGamePrepare()
	{
		Log("Getting ready to play");
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
		XState.MapPreferences map = Core.state.mapPreferences;
		XTheme theme = Core.plugs.Get<XTheme>();
		theme.SetGameInfo(map.roundTime.ToString(), map.numberOfRound.ToString(),
													PhotonNetwork.MasterClient.NickName,
													PhotonNetwork.IsMasterClient ? PhotonNetwork.MasterClient.NickName : PhotonNetwork.NickName);
		Core.plugs.Get<XTheme>().Open();
		theme.gameTimer.StartTimer();
		StartCoroutine(GamePlaying());
	}

	IEnumerator GamePlaying()
	{
		Log("Game Play");


		yield return null;
	}

}
