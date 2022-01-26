using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;

public class RoomUI : MonoBehaviour
{
	[Serializable]
	public class Characters
	{
		public string name;
		public string info;
		public Sprite sprite;
		public Transform model;
	}

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
	[SerializeField] RawImage m_MasterCharacter;
	[SerializeField] RawImage m_PlayerCharacter;
	[SerializeField] Button m_KickPlayer;
	[SerializeField] GameObject m_Ready;
	[SerializeField] Button m_MapSetting;

	public Text m_Player1Txt;
	public Text m_Player2Txt;

	public List<Characters> characters = new List<Characters>();

	Characters m_SelectedCharacter;
	Transform m_SelectedForm;
	bool m_IsGameReady;

	public void SetInfo(string title)
	{
		m_Title.text = title;
	}

	public void ActiveKickPlayerBtn(bool active)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			m_KickPlayer.gameObject.SetActive(active);
		}
	}

	public void SelectCharacter()
	{
		m_CharacterSelectForm.gameObject.SetActive(true);
	}

	public void SetCharacterList()
	{
		foreach (Characters character in characters)
		{
			Transform tr = Instantiate<Transform>(m_CharacterPrefab, Vector3.zero, Quaternion.identity, m_CharacterContentList);
			tr.GetChild(0).GetChild(0).GetComponent<Image>().sprite = character.sprite;
			tr.GetChild(1).GetComponent<Text>().text = character.name;
			tr.GetComponent<Button>().onClick.AddListener(() => OnSelectCharacter(character, tr));
			tr.name = character.name;
		}
	}

	void OnSelectCharacter(Characters character, Transform form)
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
		//Set Info

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

		Core.scenario.OnLoadScenario(nameof(ScenarioHome));
	}

	void OnCharacterFormClose()
	{
		if (m_SelectedCharacter != null)
		{
			m_SelectedCharacter.model.gameObject.SetActive(false);
			m_SelectedCharacter = null;
		}

		if (m_SelectedForm != null)
		{
			SetDefaultSelectedForm();
			m_SelectedForm = null;
		}

		m_CharacterSelectionCompleted.gameObject.SetActive(false);
		m_CharacterSelectForm.gameObject.SetActive(false);
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
			Debug.Log("Select Character");
			return;
		}

		m_CharacterSelectForm.gameObject.SetActive(false);

		if (PhotonNetwork.IsMasterClient)
		{
			m_MasterCharacter.gameObject.SetActive(true);
		}
		else
		{
			m_PlayerCharacter.gameObject.SetActive(true);
		}

	}

	public void Init()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			m_Player1Txt.text = "Master";
			m_GameStart.gameObject.SetActive(true);

			if (m_GameReady.gameObject.activeSelf)
			{
				m_GameReady.gameObject.SetActive(false);
			}

		}
		else
		{
			m_Player2Txt.text = "Player";
			m_GameReady.gameObject.SetActive(true);

			if (m_GameStart.gameObject.activeSelf)
			{
				m_GameStart.gameObject.SetActive(false);
			}

		}
	}

	void GameReady()
	{
		if (PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom || !PhotonNetwork.IsConnectedAndReady) { return; }

		if (m_IsGameReady)
		{
			m_Ready.SetActive(false);
			m_IsGameReady = false;
		}
		else
		{
			if (m_SelectedCharacter == null)
			{
				Debug.Log("Selct Character");
				return;
			}

			m_Ready.SetActive(true);
			m_IsGameReady = true;
		}
	}

	void OpenMapSetting()
	{
		if (Core.plugs.Has<MapSettings>())
		{
			MapSettings settings = Core.plugs.Get<MapSettings>();
			settings.Open();
			settings.confirm = (a, b, c, d) => ChangedMapSetting(a, b, c, d);
		}

		Core.plugs.Load<MapSettings>();
	}

	void ChangedMapSetting(string map, string title, int time, int number)
	{

	}

	void GameStart()
	{
		if (!PhotonNetwork.IsMasterClient) { return; }
		if (!m_IsGameReady) { return; }

	}

	// Start is called before the first frame update
	void Start()
	{
		m_Close.onClick.AddListener(GoHome);
		m_CharacterFormClose.onClick.AddListener(OnCharacterFormClose);
		m_CharacterSelectionCompleted.onClick.AddListener(OnCharacterSelectionCompleted);
		m_KickPlayer.onClick.AddListener(KickPlayer);
		m_GameReady.onClick.AddListener(GameReady);
		m_GameStart.onClick.AddListener(GameStart);
		m_CharacterSelect.onClick.AddListener(SelectCharacter);
		m_MapSetting.onClick.AddListener(OpenMapSetting);

		Init();
		//Init 한번만 할지 계속 만들지 고민..
		SetCharacterList();

	}


}
