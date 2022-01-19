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
			map.GetComponent<Button>().onClick.AddListener(() => OnClickMap(map));
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

		string roomTitle = string.IsNullOrEmpty(m_RoomTitle.text) ? m_RoomTitle.placeholder.GetComponent<Text>().text : m_RoomTitle.text;
		PhotonNetwork.CreateRoom(roomTitle, new RoomOptions { MaxPlayers = 2 });

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


	void OnClickMap(Transform map)
	{
		if (m_SelectedMap)
		{
			m_SelectedMap.GetChild(0).gameObject.SetActive(false);
			m_SelectedMap.GetChild(1).GetChild(0).GetComponent<Text>().color = Color.white;
			m_SelectedMap.GetChild(1).GetChild(1).GetComponent<Text>().color = Color.white;
		}

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
			foreach (Transform v in m_MapContentView)
			{
				DestroyImmediate(v.gameObject);
			}
		}
	}

}