using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

public class ScenarioRoom : MonoBehaviourPunCallbacks, IScenario
{
	public string scenarioName => typeof(ScenarioRoom).Name;

	public RoomUI roomUI;
	public RoomChat roomChat;

	private DevPhotonNetwork devPhotonNetwork;

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
		if (!PhotonNetwork.IsConnectedAndReady && Core.networkManager.member == null)
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

		string roomName = Core.gameManager.roomName;
        if (string.IsNullOrEmpty(roomName))
		{
			NoticePopup.content = MessageCommon.Get("room.joinroomfailed");
			Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
			StartCoroutine(WaitingScnearioUnloaded(() => Core.scenario.OnLoadScenario(nameof(ScenarioHome))));
			done?.Invoke();
			return;
		}

		PhotonNetwork.JoinRoom(roomName);
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
		StartCoroutine(WaitingScnearioUnloaded(() => Core.scenario.OnLoadScenario(nameof(ScenarioHome))));
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		StartCoroutine(WaitingScnearioUnloaded(() => Core.scenario.OnLoadScenario(nameof(ScenarioHome))));
	}

	public override void OnJoinedLobby()
	{
		devPhotonNetwork = new DevPhotonNetwork();
		devPhotonNetwork.CreateRoom();
	}

	public override void OnCreatedRoom()
	{
		Debug.Log("On Created Room");
		roomUI.Init();
		Core.networkManager.member = MemberFactory.Get();
		roomChat.Connect(Core.networkManager.member.mbr_id, devPhotonNetwork.GetRoomTitle());
		roomChat.connectCompleted = ConnectCompleted;
		PhotonNetwork.EnableCloseConnection = true;
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		if (roomChat.IsConnect())
		{
			roomChat.OnSendMessage(string.Format(MessageCommon.Get("room.playerLeft"), otherPlayer.NickName));
		}
		roomUI.OnPlayerLeft();
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		roomUI.OnPlayerEnter(newPlayer.NickName);
	}

	public override void OnLeftRoom()
	{
		if (PhotonNetwork.IsConnected && PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.DisconnectingFromGameServer)
		{
			roomChat.DisConnect();
			Core.scenario?.OnLoadScenario(nameof(ScenarioHome));
		}
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
			string master = (string)custom["RoomManager"];
			string title = PhotonNetwork.CurrentRoom.Name;
			int numberOfRound = (int)custom["NumberOfRound"];
			int roundTime = (int)custom["RoundTime"];

			roomUI.SetInfo(title);
			roomUI.SetMasterName(master);
			roomUI.RoomMapSetInfo(map, numberOfRound, roundTime);
			Core.state.mapPreferences = new XState.MapPreferences(map, numberOfRound, roundTime);

			if (!PhotonNetwork.IsMasterClient)
			{
				roomUI.SetPlayerName(PhotonNetwork.NickName);
			}
		}
	}

	void ConnectRoomChat()
	{
		roomChat.Connect(Core.networkManager.member.mbr_id, PhotonNetwork.CurrentRoom.Name);
		roomChat.connectCompleted = ConnectCompleted;
		PhotonNetwork.EnableCloseConnection = true;
	}

	IEnumerator WaitingScnearioUnloaded(UnityAction done)
	{
        while (Core.scenario.GetPreScenario() != null) { yield return null; }
		done?.Invoke();
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
