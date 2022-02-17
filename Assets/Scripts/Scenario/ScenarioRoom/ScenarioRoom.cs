using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ScenarioRoom : MonoBehaviourPunCallbacks, IScenario
{
    public string scenarioName => typeof(ScenarioRoom).Name;

    public string userName = "Joe";
    public RoomUI roomUI;
    public RoomChat roomChat;

    [SerializeField] Text m_Title;
    [SerializeField] Button m_Close;

    public void OnScenarioPrepare(UnityAction done)
    {
        BattleWtihAnyOneStarter.GetBlockSkybox()?.gameObject.SetActive(false);
        BattleWtihAnyOneStarter.GetLoading()?.StartLoading();

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
            roomUI.SetMasterName(PhotonNetwork.MasterClient.NickName);

            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.Log(PhotonNetwork.NickName);
                roomUI.SetPlayerName(PhotonNetwork.NickName);
            }
        }

        Core.plugs.DefaultEnsure();
        done?.Invoke();
    }

    public void OnScenarioStandbyCamera(UnityAction done)
    {
        done?.Invoke();
    }

    public void OnScenarioStart(UnityAction done)
    {
        if (PhotonNetwork.InRoom)
        {
            roomChat.Connect(Core.networkManager.userNickName, PhotonNetwork.CurrentRoom.Name);
            roomChat.connectCompleted = ConnectCompleted;
            PhotonNetwork.EnableCloseConnection = true;
        }

        done?.Invoke();
    }

    public void OnScenarioStop(UnityAction done)
    {
        PhotonNetwork.EnableCloseConnection = false;
        done?.Invoke();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (roomChat.IsConnect())
        {
            roomChat.OnSendMessage(otherPlayer.NickName + " Left Room..");
        }
        Debug.LogError(otherPlayer.IsMasterClient);
        roomUI.ActiveKickPlayerBtn(false);
        roomUI.OnLeftPlayer();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        roomUI.SetPlayerName(newPlayer.NickName);
        roomUI.ActiveKickPlayerBtn(true);
    }

    public override void OnJoinedLobby()
    {
        CreateRoom();
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("On CreateRoom");

        roomChat.Connect("Joe", "TestRoom");
        roomChat.connectCompleted = ConnectCompleted;
        PhotonNetwork.EnableCloseConnection = true;
        roomUI.Init();
        Core.networkManager.isLogined = true;
    }

    //Photon Network, Chat Connect Completed
    void ConnectCompleted()
    {
        BattleWtihAnyOneStarter.GetLoading()?.StopLoading();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError(cause);
    }

    public override void OnLeftRoom()
    {
        roomChat.DisConnect();
        Core.scenario?.OnLoadScenario(nameof(ScenarioHome));
    }

    void JoinRoom()
    {
        PhotonNetwork.JoinLobby();
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
