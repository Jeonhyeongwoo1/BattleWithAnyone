using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ScenarioRoom : MonoBehaviourPunCallbacks, IScenario
{
	public string scenarioName => typeof(ScenarioRoom).Name;

	public RoomUI roomUI;

	[SerializeField] Text m_Title;
	[SerializeField] Button m_Close;

	public void OnScenarioPrepare(UnityAction done)
	{
		if (!PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InRoom)
		{
			//Debug.LogError("DisConnected");
			//Error 처리


			//개발용
			//Connect
			Core.networkManager.ConnectPhotonNetwork(JoinRoom);
		}

		ExitGames.Client.Photon.Hashtable custom = PhotonNetwork.CurrentRoom?.CustomProperties;

		if (custom != null)
		{
			string map = (string)custom["Map"];
			string title = PhotonNetwork.CurrentRoom.Name;

			roomUI.SetInfo(title);
		}

		done?.Invoke();
	}

	void JoinRoom()
	{
		PhotonNetwork.JoinLobby();
	}

	public override void OnJoinedLobby()
	{
		CreateRoom();
	}

	public override void OnCreatedRoom()
	{
		Debug.Log("On CreateRoom");
	}

	void CreateRoom()
	{
		string[] LobbyOptions = new string[2];
		LobbyOptions[0] = "RoomManager";
		LobbyOptions[1] = "Map";
		ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable() {
											{ "RoomManager", "Name" },
											{ "Map", "Battleground" }};

		RoomOptions roomOptions = new RoomOptions();
		roomOptions.IsVisible = true;
		roomOptions.IsOpen = true;
		roomOptions.MaxPlayers = (byte)2;
		roomOptions.CustomRoomPropertiesForLobby = LobbyOptions;
		roomOptions.CustomRoomProperties = customProperties;
		PhotonNetwork.CreateRoom("TestRoom", roomOptions, TypedLobby.Default);
	}

	public void OnScenarioStandbyCamera(UnityAction done)
	{
		done?.Invoke();
	}

	public void OnScenarioStart(UnityAction done)
	{
		done?.Invoke();
	}

	public void OnScenarioStop(UnityAction done)
	{
		done?.Invoke();
	}

	public void SetRoomInfo(string title)
	{
		m_Title.text = title;
	}

	// Start is called before the first frame update
	void Start()
	{
		Core.Ensure(() => Core.scenario.OnLoadedScenario(this));
	}

	private void Awake()
	{
		Core.Ensure(() => Core.scenario.OnScenarioAwaked(this));
	}

}
