using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;

public class RoomUI : MonoBehaviourPunCallbacks
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

	[SerializeField] Text m_Title;
	[SerializeField] Button m_Close;
	[SerializeField] Button m_GameStart;
	[SerializeField] Transform m_MasterCharacterSlot;
	[SerializeField] Transform m_PlayerCharacterSlot;
	[SerializeField] Button m_CharacterFormClose;
	[SerializeField] RawImage m_MasterCharacter;
	[SerializeField] RawImage m_PlayerCharacter;

	public Text m_Player1Txt;
	public Text m_Player2Txt;

	public List<Characters> characters = new List<Characters>();

	Characters m_SelectedCharacter;
	Transform m_SelectedForm;

	public void SetInfo(string title)
	{
		m_Title.text = title;
	}

	public void SelectCharacter(string player)
	{
		if ((player == "Master" && PhotonNetwork.IsMasterClient) || (player == "Player" && !PhotonNetwork.IsMasterClient))
		{
			m_CharacterSelectForm.gameObject.SetActive(true);
		}
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
		//Set Info

	}

	void SetDefaultSelectedForm()
	{
		m_SelectedForm.GetChild(2).gameObject.SetActive(false);
		m_SelectedForm.GetChild(1).GetComponent<Text>().color = Color.white;
		m_SelectedForm.GetChild(1).localScale = Vector3.one;
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		if (!newPlayer.IsMasterClient)
		{
			m_Player2Txt.text = "Player";
		}
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

	// Start is called before the first frame update
	void Start()
	{
		m_Close.onClick.AddListener(GoHome);
		m_CharacterFormClose.onClick.AddListener(OnCharacterFormClose);
		m_CharacterSelectionCompleted.onClick.AddListener(OnCharacterSelectionCompleted);

		//Init 한번만 할지 계속 만들지 고민..
		SetCharacterList();

		if (PhotonNetwork.IsMasterClient)
		{
			m_Player1Txt.text = "Master";
		}
		else
		{
			m_Player2Txt.text = "Player";
		}
	}
}
