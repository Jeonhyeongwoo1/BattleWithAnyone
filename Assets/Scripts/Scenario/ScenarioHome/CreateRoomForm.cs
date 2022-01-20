using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

[Serializable]
public class MapInfo
{
	public string mapTitle;
	public string mapInfo;
	public string imageName;
	public Sprite sprite;
}

[Serializable]
public class MapList
{
	public MapInfo[] data;
}

public class CreateRoomForm : MonoBehaviourPunCallbacks
{
	public Transform mapPrefab;
	public MapList mapList;

	[SerializeField] Button m_Confirm;
	[SerializeField] Button m_Cancel;
	[SerializeField] Transform m_MapContentView;
	[SerializeField] InputField m_RoomTitle;

	Transform m_SelectedMap;
	string m_MapTitle;

	public void CreateMapList()
	{
		mapList = Core.settings.LoadMapList();
		if (mapList == null) { return; }

		int c = mapList.data.Length;
		for (int i = 0; i < c; i++)
		{
			MapInfo mapInfo = mapList.data[i];
			mapInfo.sprite = LoadMapImage(mapInfo.imageName);
			Transform map = Instantiate<Transform>(mapPrefab, Vector3.zero, Quaternion.identity, m_MapContentView);
			map.GetChild(1).GetChild(0).GetComponent<Text>().text = mapInfo.mapTitle;
			map.GetChild(1).GetChild(1).GetComponent<Text>().text = mapInfo.mapInfo;
			map.GetComponent<Image>().sprite = mapInfo.sprite;
			map.GetComponent<Button>().onClick.AddListener(() => OnClickMap(map, mapInfo.mapTitle));
		}
	}

	public Sprite LoadMapImage(string imageName)
	{
		string path = Core.settings.mapImagePath + "/" + imageName;
		Sprite image = Resources.Load<Sprite>(path);

		if (image == null)
		{
			Debug.LogError("Failed to Load Image");
			return null;
		}

		return image;
	}

	void OnClickConfirm()
	{
		if (!PhotonNetwork.IsConnected)
		{
			Debug.LogError("Network isn't Connected");
			return;
		}

		//Create Room
		if (!m_SelectedMap)
		{
			Debug.Log("Map is not Selected");
			return;
		}

		if (string.IsNullOrEmpty(m_RoomTitle.text))
		{
			Debug.Log("There isn't room Title");
			return;
		}

		string[] LobbyOptions = new string[2];
		LobbyOptions[0] = "RoomManager";
		LobbyOptions[1] = "Map";
		ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable() {
											{ "RoomManager", "Name" },
											{ "Map", m_MapTitle }};
											
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.IsVisible = true;
		roomOptions.IsOpen = true;
		roomOptions.MaxPlayers = (byte)2;
		roomOptions.CustomRoomPropertiesForLobby = LobbyOptions;
		roomOptions.CustomRoomProperties = customProperties;
		PhotonNetwork.CreateRoom(m_RoomTitle.text, roomOptions, TypedLobby.Default);

	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		Debug.LogError("On Create Room Failed : " + message);
	}

	public override void OnCreatedRoom()
	{
		Debug.Log("On Created Room");
		Core.scenario.OnLoadScenario(nameof(ScenarioRoom));
	}

	void OnClickMap(Transform map, string mapTitle)
	{
		if (m_SelectedMap)
		{
			m_SelectedMap.GetChild(0).gameObject.SetActive(false);
			m_SelectedMap.GetChild(1).GetChild(0).GetComponent<Text>().color = Color.white;
			m_SelectedMap.GetChild(1).GetChild(1).GetComponent<Text>().color = Color.white;
		}

		m_MapTitle = mapTitle;
		m_SelectedMap = map;
		map.GetChild(0).gameObject.SetActive(true);
		m_SelectedMap.GetChild(1).GetChild(0).GetComponent<Text>().color = Color.yellow;
		m_SelectedMap.GetChild(1).GetChild(1).GetComponent<Text>().color = Color.yellow;
	}

	void OnClickCancel()
	{
		gameObject.SetActive(false);
	}

	private void Start()
	{
		m_Confirm.onClick.AddListener(OnClickConfirm);
		m_Cancel.onClick.AddListener(OnClickCancel);
	}

	public override void OnDisable()
	{
		if (m_MapContentView.childCount > 0)
		{
			for (int i = 0; i < m_MapContentView.childCount; i++)
			{
				Transform tr = m_MapContentView.GetChild(i);
				Destroy(tr.gameObject);
			}
		}

		if (m_SelectedMap)
		{
			m_SelectedMap.GetChild(0).gameObject.SetActive(false);
			m_SelectedMap.GetChild(1).GetChild(0).GetComponent<Text>().color = Color.white;
			m_SelectedMap.GetChild(1).GetChild(1).GetComponent<Text>().color = Color.white;
		}

		m_MapTitle = null;
	}

}
