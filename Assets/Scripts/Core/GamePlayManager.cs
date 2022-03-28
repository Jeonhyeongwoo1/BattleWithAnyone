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
			round.ShowRoundInfo(GamePrepared);
		});
	}

	void GamePrepared()
	{
		Log("Game ready");
		
		XState.MapPreferences map = Core.state.mapPreferences;
		XTheme theme = Core.plugs.Get<XTheme>();
		theme.SetGameInfo(map.roundTime.ToString(), map.numberOfRound.ToString(), Core.networkManager.member.mbr_id, Core.state.playerName);
		Core.plugs.Get<XTheme>().Open();
theme.gameTimer.STartTimer();
		StartCoroutine(GamePlaying());
	}

	IEnumerator GamePlaying()
	{
		Log("Game Play");


		yield return null;
	}

}
