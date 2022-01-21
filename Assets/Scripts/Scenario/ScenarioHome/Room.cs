using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Room : MonoBehaviourPunCallbacks, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] Color m_OnHover;
	[SerializeField] Color m_Normal;

	[SerializeField] Text m_Title;
	[SerializeField] Text m_RoomManagerName;
	[SerializeField] Text m_Map;

	string m_RoomName;

	public void SetRoomInfo(string title, string roomManager, string mapInfo)
	{
		m_Title.text = "방 제목 : " + title;
		m_RoomManagerName.text = "Player : " + roomManager;
		m_Map.text = "맵 : " + mapInfo;
		m_RoomName = title;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		//OnClick
		if (!PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InLobby)
		{
			Debug.LogError("Network isn't connected");
			return;
		}

		Debug.Log("Join Room : " + m_RoomName);
		PhotonNetwork.JoinRoom(m_RoomName);
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("Joined Room");
		Core.scenario.OnLoadScenario(nameof(ScenarioRoom));
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		GetComponent<Image>().color = m_OnHover;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		GetComponent<Image>().color = m_Normal;
	}
}
