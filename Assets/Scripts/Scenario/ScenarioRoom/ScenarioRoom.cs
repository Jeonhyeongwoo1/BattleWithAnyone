using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ScenarioRoom : MonoBehaviourPunCallbacks, IScenario
{
	public string scenarioName => typeof(ScenarioRoom).Name;

	public RoomUI roomUI;
	public RoomChat roomChat;
	public string testname = "전형우";

	public void OnScenarioPrepare(UnityAction done)
	{
		BattleWtihAnyOneStarter.GetBlockSkybox()?.gameObject.SetActive(false);
		BattleWtihAnyOneStarter.GetLoading()?.StartLoading();
		Core.plugs.DefaultEnsure();
		done?.Invoke();
	}

	public void OnScenarioStandbyCamera(UnityAction done)
	{
		done?.Invoke();
	}

	public void OnScenarioStart(UnityAction done)
	{
		if (!PhotonNetwork.IsConnectedAndReady && !Core.networkManager.isLogined)
		{
			//Room에 바로 진입시(DEV)
			Core.networkManager.ConnectPhotonNetwork(() => PhotonNetwork.JoinLobby());
			done?.Invoke();
			return;
		}

		if (PhotonNetwork.InRoom)
		{
			SetRoomCustomInfo();
			ConnectRoomChat();
			done?.Invoke();
			return;
		}

		PhotonNetwork.JoinRoom(Core.networkManager.roomName);
		done?.Invoke();
	}

	public void OnScenarioStop(UnityAction done)
	{
		PhotonNetwork.EnableCloseConnection = false;
		done?.Invoke();
	}

	public override void OnJoinedRoom()
	{
		SetRoomCustomInfo();
		ConnectRoomChat();
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		Debug.LogError("On JoinRoom Failed. Return Code :" + returnCode + ", Message : " + message);

		NoticePopup.content = MessageCommon.Get("room.joinroomfailed");
		Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
		Core.scenario.OnLoadScenario(nameof(ScenarioHome));
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		Debug.LogError("Failed Create Room. Return Code : " + returnCode + ", Message : " + message);
	}

	public override void OnJoinedLobby()
	{
        Debug.Log("On Joined Lobby");
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

	public override void OnCreatedRoom()
	{
        Debug.Log("On Created Room");
		roomUI.Init();
		roomChat.Connect(testname, "TestRoom");
		roomChat.connectCompleted = ConnectCompleted;
		PhotonNetwork.EnableCloseConnection = true;
		Core.networkManager.isLogined = true;
		Core.networkManager.SetPlayerName(testname);
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		if (roomChat.IsConnect())
		{
			roomChat.OnSendMessage(otherPlayer.NickName + " Left Room..");
		}
		roomUI.OnPlayerLeft();
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
        roomUI.OnPlayerEnter(newPlayer.NickName);
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.LogError("Disconnected : " + cause);

		NoticePopup.content = MessageCommon.Get("network.disconnect");
		Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
		Core.scenario.OnLoadScenario(nameof(ScenarioLogin));
	}

	public override void OnLeftRoom()
	{
		roomChat.DisConnect();
		Core.scenario?.OnLoadScenario(nameof(ScenarioHome));
	}

	//Photon Network, Chat Connect Completed
	void ConnectCompleted()
	{
		BattleWtihAnyOneStarter.GetLoading()?.StopLoading();
	}

	void SetRoomCustomInfo()
	{
		ExitGames.Client.Photon.Hashtable custom = PhotonNetwork.CurrentRoom?.CustomProperties;
		if (custom != null)
		{
			string map = (string)custom["Map"];
			string title = PhotonNetwork.CurrentRoom.Name;
			roomUI.SetInfo(title);
			roomUI.SetMasterName(PhotonNetwork.MasterClient.NickName);

			if (!PhotonNetwork.IsMasterClient)
			{
				roomUI.SetPlayerName(PhotonNetwork.NickName);
			}
		}
	}

	void ConnectRoomChat()
	{
		roomChat.Connect(Core.networkManager.userNickName, PhotonNetwork.CurrentRoom.Name);
		roomChat.connectCompleted = ConnectCompleted;
		PhotonNetwork.EnableCloseConnection = true;
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
