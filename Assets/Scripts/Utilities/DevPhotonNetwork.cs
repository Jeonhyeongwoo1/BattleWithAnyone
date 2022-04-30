using Photon.Pun;
using Photon.Realtime;

public class DevPhotonNetwork
{
	string m_RoomTitle = "TestRoom";
	string m_MapTitle = "Mansion";
	int m_NumberOfRound = 3;
	int m_RoundTime = 150;

    public string GetRoomTitle() => m_RoomTitle;

	public void CreateRoom()
	{
		string[] LobbyOptions = new string[4];
		LobbyOptions[0] = "RoomManager";
		LobbyOptions[1] = "Map";
		LobbyOptions[2] = "NumberOfRound";
		LobbyOptions[3] = "RoundTime";

        Member member = MemberFactory.Get();
        Core.networkManager.member = member;
		ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable() {
											{ "RoomManager", member.mbr_id },
											{ "Map", m_MapTitle },
											{ "NumberOfRound", m_NumberOfRound},
											{ "RoundTime", m_RoundTime }};

		RoomOptions roomOptions = new RoomOptions();
		roomOptions.IsVisible = true;
		roomOptions.IsOpen = true;
		roomOptions.MaxPlayers = (byte)2;
		roomOptions.CustomRoomPropertiesForLobby = LobbyOptions;
		roomOptions.CustomRoomProperties = customProperties;
		PhotonNetwork.CreateRoom(m_RoomTitle, roomOptions, TypedLobby.Default);
		Core.state.mapPreferences = new XState.MapPreferences(m_MapTitle, m_NumberOfRound, m_RoundTime);
	}

}
