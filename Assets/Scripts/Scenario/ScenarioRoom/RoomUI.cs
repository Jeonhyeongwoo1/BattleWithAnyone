using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;

public class RoomUI : MonoBehaviourPunCallbacks
{
	[Serializable]
	public class Character
	{
		public string name;
		public string info;
		public Sprite sprite;
		public Transform model;
	}

	public List<Character> characters = new List<Character>();

	[SerializeField] CharacterInfoForm m_CharacterInfo;
	[SerializeField] RoomMapInfo m_RoomMapInfo;
	[SerializeField] Button m_CharacterSelectionCompleted;
	[SerializeField] Transform m_CharacterContentModelImage;
	[SerializeField] Transform m_CharacterContentList;
	[SerializeField] Transform m_CharacterPrefab;
	[SerializeField] Transform m_CharacterSelectForm;
	[SerializeField] Button m_CharacterSelect;
	[SerializeField] CharacterDrag m_CharacterDrag;

	[SerializeField] Text m_Title;
	[SerializeField] Button m_Close;
	[SerializeField] Button m_GameStart;
	[SerializeField] Button m_GameReady;
	[SerializeField] Button m_CharacterFormClose;
	[SerializeField] Button m_KickPlayer;
	[SerializeField] Button m_Language;
	[SerializeField] GameObject m_Ready;
	[SerializeField] Button m_MapSetting;
	[SerializeField] Button m_GameSetting;
	[SerializeField] Transform m_SelectedPlayerCharacter;
	[SerializeField] Transform m_SelectedMasterCharacter;
	[SerializeField] Text m_PlayerName;
	[SerializeField] Text m_MasterName;

	Character m_SelectedCharacter;
	Transform m_OtherPlayerCharcter;
	Transform m_SelectedForm;
	bool m_IsGameReady = false;

	public void SetInfo(string title) => m_Title.text = title;
	public void SetPlayerName(string name) => m_PlayerName.text = name;
	public void SetMasterName(string name) => m_MasterName.text = name;

	public void Init()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			m_GameStart.gameObject.SetActive(true);
			m_MapSetting.gameObject.SetActive(true);
			m_GameReady.gameObject.SetActive(false);
		}
		else
		{
			m_GameReady.gameObject.SetActive(true);
			m_MapSetting.gameObject.SetActive(false);
			m_GameStart.gameObject.SetActive(false);
		}
	}

	public void SelectCharacter()
	{
		m_CharacterSelectForm.gameObject.SetActive(true);
	}

	public void OnPlayerEnter(string playerName)
	{
		m_KickPlayer.gameObject.SetActive(true);
		m_PlayerName.text = playerName;

		if (m_SelectedCharacter != null)
		{
			photonView.RPC(nameof(CreateCharacter), RpcTarget.Others, m_SelectedCharacter.name);
		}
	}

	public void OnPlayerLeft()
	{
		//상대방이 나가면 내가 원래 방장이든 아니든 나의 정보를 새롭게 구성
		//Master가 아니면 Master자리로 이동(왼쪽이 Master)

		if (m_SelectedPlayerCharacter.childCount > 0)
		{
			Destroy(m_SelectedPlayerCharacter.GetChild(0).gameObject);
		}

		if (m_SelectedMasterCharacter.childCount > 0)
		{
			Destroy(m_SelectedMasterCharacter.GetChild(0).gameObject);
		}

		m_GameReady.gameObject.SetActive(false);
		m_GameStart.gameObject.SetActive(true);
		m_KickPlayer.gameObject.SetActive(false);
		m_MasterName.text = Core.networkManager.member.mbr_nm;
		m_PlayerName.text = null;
		if (m_IsGameReady)
		{
			m_Ready.SetActive(false);
			m_IsGameReady = false;
		}

		m_MapSetting.gameObject.SetActive(true);

		if (m_SelectedCharacter != null)
		{
			Transform character = Instantiate<Transform>(m_SelectedCharacter.model,
															m_SelectedMasterCharacter.position,
															Quaternion.Euler(new Vector3(0, -180, 0)),
															m_SelectedMasterCharacter);
		}

		string roomManager = (string)PhotonNetwork.CurrentRoom.CustomProperties["RoomManager"];
		if (roomManager != PhotonNetwork.MasterClient.NickName)
		{
			ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable() {
											{ "RoomManager", PhotonNetwork.MasterClient.NickName }};
			//Room 정보가 변경되므로 ScearnioHome RoomListUpdate가 호출된다.
			PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);

		}
	}

	void OnSelectCharacter(Character character, Transform form)
	{
		if (m_SelectedCharacter != null)
		{
			m_SelectedCharacter.model.gameObject.SetActive(false);
		}

		if (m_SelectedForm)
		{
			SetDefaultSelectedForm();
		}

		character.model.gameObject.SetActive(true);
		m_SelectedCharacter = character;

		form.GetChild(2).gameObject.SetActive(true);
		form.GetChild(1).GetComponent<Text>().color = Color.yellow;
		form.GetChild(1).localScale = new Vector3(1.1f, 1.1f, 0);
		m_SelectedForm = form;

		m_CharacterSelectionCompleted.gameObject.SetActive(true);
		m_CharacterDrag.SetCharacter(character.model);
        
		if(!m_CharacterInfo.gameObject.activeSelf)
        {
            m_CharacterInfo.gameObject.SetActive(true);
        }

		if(character.model.TryGetComponent<PlayerController>(out var player))
		{
			string name = character.name;
			BulletAttribute.BulletType type = player.gunType;
			float speed = player.speed;
			int power = player.damage;
			int bulletCount = player.bulletCount;
			m_CharacterInfo.SetInfo(name, type, speed, power, bulletCount);
		}		

	}

	void SetDefaultSelectedForm()
	{
		m_SelectedForm.GetChild(2).gameObject.SetActive(false);
		m_SelectedForm.GetChild(1).GetComponent<Text>().color = Color.white;
		m_SelectedForm.GetChild(1).localScale = Vector3.one;
	}

	void GoHome()
	{
		//Open Popup
		if (PhotonNetwork.InRoom)
		{
			PhotonNetwork.LeaveRoom();
		}
	}

	void OnCharacterFormClose()
	{
		if (m_SelectedCharacter != null)
		{
			m_SelectedCharacter.model.gameObject.SetActive(false);
			m_SelectedCharacter = null;

			//Change
			photonView.RPC(nameof(ChangeCharacter), RpcTarget.All, "", PhotonNetwork.IsMasterClient);
		}

		if (m_SelectedForm != null)
		{
			SetDefaultSelectedForm();
			m_SelectedForm = null;
		}

		m_CharacterSelectionCompleted.gameObject.SetActive(false); //캐릭터 선택완료
		m_CharacterSelectForm.gameObject.SetActive(false); //캐릭터 선택창
		m_CharacterInfo.gameObject.SetActive(false);
	}

	void OnDestroyCharacterContentList()
	{
		if (m_CharacterContentList.childCount > 0)
		{
			for (int i = 0; i < m_CharacterContentList.childCount; i++)
			{
				Transform tr = m_CharacterContentList.GetChild(i);
				Destroy(tr.gameObject);
			}
		}
	}

	void KickPlayer()
	{
		//Master, 방장은 Kick 불가능
		foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
		{
			if (!player.Value.IsMasterClient)
			{
				PhotonNetwork.CloseConnection(player.Value);
			}
		}
	}

	void OnCharacterSelectionCompleted()
	{
		if (m_SelectedCharacter == null)
		{
			NoticePopup.content = Core.language.GetNotifyMessage("room.selectcharacter");
			Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
			return;
		}

		m_CharacterSelectForm.gameObject.SetActive(false);
		photonView.RPC(nameof(ChangeCharacter), RpcTarget.All, m_SelectedCharacter.name, PhotonNetwork.IsMasterClient);
	}

	void SetCharacterList()
	{
		foreach (Character character in characters)
		{
			Transform tr = Instantiate<Transform>(m_CharacterPrefab, Vector3.zero, Quaternion.identity, m_CharacterContentList);
			tr.GetChild(0).GetChild(0).GetComponent<Image>().sprite = character.sprite;
			tr.GetChild(1).GetComponent<Text>().text = character.name;
			tr.GetComponent<Button>().onClick.AddListener(() => OnSelectCharacter(character, tr));
			tr.name = character.name;
		}
	}

	void GameReady()
	{
		if (PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom || !PhotonNetwork.IsConnectedAndReady) { return; }

		if (m_SelectedCharacter == null)
		{
			NoticePopup.content = Core.language.GetNotifyMessage("room.selectcharacter");
			Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
			return;
		}

		photonView.RPC(nameof(PlayerGameReadied), RpcTarget.All);
	}

	void OpenMapSetting()
	{
		if (Core.plugs.Has<MapSettings>())
		{
			MapSettings settings = Core.plugs.Get<MapSettings>();
			settings.Open();
			settings.confirm = (roomName) => UpdateMapSetting();
			return;
		}

		Core.plugs.Load<MapSettings>(() =>
		{
			MapSettings settings = Core.plugs.Get<MapSettings>();
			settings.Open();
			settings.confirm = (roomName) => UpdateMapSetting();
		});
	}

	void GameStart()
	{
		if (!PhotonNetwork.IsMasterClient) { return; }

		if (!m_IsGameReady)
		{
			NoticePopup.content = Core.language.GetNotifyMessage("room.playerisnotready");
			Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
			return;
		}

		if (m_SelectedCharacter == null)
		{
			NoticePopup.content = Core.language.GetNotifyMessage("room.selectcharacter");
			Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
			return;
		}

		if (Core.state.mapPreferences == null)
		{
			NoticePopup.content = Core.language.GetNotifyMessage("map.selectmap");
			Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
			return;
		}

        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable() {
                                            { "GameStart", true }};
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);

		Core.state.masterCharacter = m_SelectedCharacter.model;
		photonView.RPC(nameof(UpdatePlayerNames), RpcTarget.All);
        photonView.RPC(nameof(SetPlayersCharacter), RpcTarget.Others);
		photonView.RPC(nameof(OnLoadScenarioPlay), RpcTarget.All);
	}

    void OpenGameSetting()
    {
        Popups popup = Core.plugs.Get<Popups>();
        if (!popup.IsOpened<GameSettings>())
        {
            popup.OpenPopupAsync<GameSettings>();
        }
    }

    void UpdateMapSetting()
    {
        XState.MapPreferences preferences = Core.state.mapPreferences;
        string mapName = preferences.mapName;
        int numberOfRound = preferences.numberOfRound;
        int roundTime = preferences.roundTime;

        photonView.RPC(nameof(RoomMapSetInfo), RpcTarget.All, mapName, numberOfRound, roundTime);

        string[] LobbyOptions = new string[4];
        LobbyOptions[0] = "RoomManager";
        LobbyOptions[1] = "Map";
        LobbyOptions[2] = "NumberOfRound";
        LobbyOptions[3] = "RoundTime";
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable() {
                                            { "RoomManager", PhotonNetwork.MasterClient.NickName },
                                            { "Map", mapName },
                                            { "NumberOfRound", numberOfRound},
                                            { "RoundTime", roundTime }};
        //Room 정보가 변경되므로 ScearnioHome RoomListUpdate가 호출된다.
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
    }

    void OpenLanguagePopup()
    {
        Popups popups = Core.plugs.Get<Popups>();
        if (popups == null) { return; }
        if (!popups.IsOpened<LanguagePopup>())
        {
            popups.OpenPopupAsync<LanguagePopup>();
        }
    }

    [PunRPC]
	void SetPlayersCharacter()
    {
		Core.state.playerCharacter = m_SelectedCharacter.model;
	}

	[PunRPC]
	void OnLoadScenarioPlay()
	{
		BattleWtihAnyOneStarter.GetBlockSkybox()?.gameObject.SetActive(true);
		Core.scenario.OnLoadScenario(nameof(ScenarioPlay), ()=> gameObject.SetActive(false));
	}

    [PunRPC]
    void UpdatePlayerNames()
    {
        Core.gameManager.playerName = PhotonNetwork.PlayerList[1]?.NickName;
    }

    [PunRPC]
	public void RoomMapSetInfo(string map, int numberOfRound, int roundTime) => m_RoomMapInfo.SetInfo(map, numberOfRound, roundTime);

	[PunRPC]
	void CreateCharacter(string character)
	{
		foreach (var v in characters)
		{
			if (v.name == character)
			{
				Transform c = Instantiate<Transform>(v.model, m_SelectedMasterCharacter.position, Quaternion.Euler(new Vector3(0, -180, 0)), m_SelectedMasterCharacter);
				if (!c.gameObject.activeSelf)
				{
					c.gameObject.SetActive(true);
				}
                m_OtherPlayerCharcter = v.model;
			}
		}
	}

	[PunRPC]
	void ChangeCharacter(string character, bool isMaster)
	{
		Transform target = isMaster ? m_SelectedMasterCharacter : m_SelectedPlayerCharacter;
		if (target.childCount > 0)
		{
			for (int i = 0; i < target.childCount; i++)
			{
				Destroy(target.GetChild(i).gameObject);
			}
		}

		foreach (var v in characters)
		{
			if (v.name == character)
			{
				Transform c = Instantiate<Transform>(v.model, target.position, Quaternion.Euler(new Vector3(0, -180, 0)), target);
				if (!c.gameObject.activeSelf)
				{
					c.gameObject.SetActive(true);
				}
				m_OtherPlayerCharcter = v.model;
			}
		}
		
	}

	[PunRPC]
	void PlayerGameReadied()
	{
		m_IsGameReady = !m_IsGameReady;
		m_Ready.SetActive(m_IsGameReady);
	}

	// Start is called before the first frame update
	void Awake()
	{
		m_Close.onClick.AddListener(GoHome);
		m_CharacterFormClose.onClick.AddListener(OnCharacterFormClose);
		m_CharacterSelectionCompleted.onClick.AddListener(OnCharacterSelectionCompleted);
		m_KickPlayer.onClick.AddListener(KickPlayer);
		m_GameReady.onClick.AddListener(GameReady);
		m_GameStart.onClick.AddListener(GameStart);
		m_CharacterSelect.onClick.AddListener(SelectCharacter);
		m_MapSetting.onClick.AddListener(OpenMapSetting);
        m_GameSetting.onClick.AddListener(OpenGameSetting);
        m_Language.onClick.AddListener(OpenLanguagePopup);
		Init();
		//Init 한번만 할지 계속 만들지 고민..
		SetCharacterList();

	}


}
