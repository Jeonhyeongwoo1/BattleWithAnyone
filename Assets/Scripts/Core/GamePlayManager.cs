using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using Photon.Pun;

public class GamePlayManager : MonoBehaviour
{
	[Serializable]
	public class MapPreferences
	{
		public string mapName;
		public int numberOfRound = 3; //게임 라운드 횟수
		public int roundTime = 150;
	}

	public string playerName { get; set; }
	public string roomName { get; set; }

	[SerializeField] private MapPreferences mapPreferences = new MapPreferences();

	Transform m_MasterCharacter;
	Transform m_PlayerCharacter;

	public void SetPlayersCharacter(Transform master, Transform player)
	{
		m_MasterCharacter = master;
		m_PlayerCharacter = player;
	}

	public (Transform, Transform) GetPlayersCharacter() => (m_MasterCharacter, m_PlayerCharacter);

	public void SetMapPreference(string mapName, int numberOfround, int roundTime)
	{
		mapPreferences.mapName = mapName;
		mapPreferences.numberOfRound = numberOfround;
		mapPreferences.roundTime = roundTime;
	}

	public MapPreferences GetMapPreference() => mapPreferences;

	public bool HasMapPreference()
	{
		if (mapPreferences == null) { return false; }
		if (mapPreferences.mapName == null) { return false; }
		return true;
	}

	public void Log(string message)
	{
		Debug.Log(message);
	}

	//GamePlay Routine
	public void GamePrepare()
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
		XTheme theme = Core.plugs.Get<XTheme>();
		theme.SetGameInfo(mapPreferences.roundTime.ToString(), mapPreferences.numberOfRound.ToString(), Core.networkManager.member.mbr_id, playerName);
		Core.plugs.Get<XTheme>().Open();

		StartCoroutine(GamePlaying());
	}

	IEnumerator GamePlaying()
	{
		Log("Game Play");



		yield return null;
	}

}
