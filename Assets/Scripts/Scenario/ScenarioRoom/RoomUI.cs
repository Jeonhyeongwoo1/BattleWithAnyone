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
    [SerializeField] GameObject m_Ready;
    [SerializeField] Button m_MapSetting;
    [SerializeField] Transform m_SelectedPlayerCharacter;
    [SerializeField] Transform m_SelectedMasterCharacter;
    [SerializeField] Text m_PlayerName;
    [SerializeField] Text m_MasterName;

    public List<Character> characters = new List<Character>();

    Character m_SelectedCharacter;
    Transform m_SelectedForm;
    bool m_IsGameReady = false;

    public void SetInfo(string title) => m_Title.text = title;
    public void SetPlayerName(string name) => m_PlayerName.text = name;
    public void SetMasterName(string name) => m_MasterName.text = name;

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
        foreach (Character character in characters)
        {
            Transform tr = Instantiate<Transform>(m_CharacterPrefab, Vector3.zero, Quaternion.identity, m_CharacterContentList);
            tr.GetChild(0).GetChild(0).GetComponent<Image>().sprite = character.sprite;
            tr.GetChild(1).GetComponent<Text>().text = character.name;
            tr.GetComponent<Button>().onClick.AddListener(() => OnSelectCharacter(character, tr));
            tr.name = character.name;
        }
    }

    public void OnLeftPlayer()
    {
        //상대방이 나갈경우 상대방의 데이터를 초기화한다.
        //내가 Master일 경우에는 상대방 데이터만 초기화한다.
        //만약 내가 Plyaer이고 Master가 방을 나갈경우에는 방장 데이터를 초기화하고 내가 방장자리로 이동한다.

        if (PhotonNetwork.IsMasterClient)
        {
            if (m_SelectedPlayerCharacter.childCount > 0)
            {
                Destroy(m_SelectedPlayerCharacter.GetChild(0).gameObject);
            }

            m_PlayerName.text = null;
            if (m_IsGameReady)
            {
                m_Ready.SetActive(false);
                m_IsGameReady = false;
            }
        }
        else
        {

            if (m_SelectedPlayerCharacter.childCount > 0)
            {
                Destroy(m_SelectedPlayerCharacter.GetChild(0).gameObject);
            }

            if (m_SelectedMasterCharacter.childCount > 0)
            {
                Destroy(m_SelectedMasterCharacter.GetChild(0).gameObject);
            }

            m_MasterName.text = m_PlayerName.text;
            m_PlayerName.text = null;
            if (m_IsGameReady)
            {
                m_Ready.SetActive(false);
                m_IsGameReady = false;
            }

            Transform character = Instantiate<Transform>(m_SelectedCharacter.model,
                                                            m_SelectedPlayerCharacter.position,
                                                            Quaternion.Euler(new Vector3(0, -180, 0)),
                                                            m_SelectedMasterCharacter);



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
    }

    void OnCharacterFormClose()
    {
        if (m_SelectedCharacter != null)
        {
            m_SelectedCharacter.model.gameObject.SetActive(false);
            m_SelectedCharacter = null;

            //Change
            photonView.RPC("ChangeCharacter", RpcTarget.All, "", PhotonNetwork.IsMasterClient ? "Master" : "Player");
        }

        if (m_SelectedForm != null)
        {
            SetDefaultSelectedForm();
            m_SelectedForm = null;
        }

        m_CharacterSelectionCompleted.gameObject.SetActive(false); //캐릭터 선택완료
        m_CharacterSelectForm.gameObject.SetActive(false); //캐릭터 선택창
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
            NoticePopup.content = MessageCommon.Get("room.selectcharacter");
            Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
            return;
        }

        m_CharacterSelectForm.gameObject.SetActive(false);
        photonView.RPC("ChangeCharacter", RpcTarget.All, m_SelectedCharacter.name, PhotonNetwork.IsMasterClient ? "Master" : "Player");
    }

    [PunRPC]
    public void ChangeCharacter(string character, string playerType)
    {
        Transform target = playerType == "Master" ? m_SelectedMasterCharacter : m_SelectedPlayerCharacter;
        if (target.childCount > 0)
        {
            for (int i = 0; i < target.childCount; i++)
            {
                Destroy(target.GetChild(i).gameObject);
            }
        }

        Transform c = null;
        foreach (var v in characters)
        {
            if (v.name == character)
            {
                c = Instantiate<Transform>(v.model, target.position, Quaternion.Euler(new Vector3(0, -180, 0)), target);
                if (!c.gameObject.activeSelf)
                {
                    c.gameObject.SetActive(true);
                }
            }
        }
    }

    public void Init()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            m_GameStart.gameObject.SetActive(true);
            m_MapSetting.gameObject.SetActive(true);
            if (m_GameReady.gameObject.activeSelf)
            {
                m_GameReady.gameObject.SetActive(false);
            }

        }
        else
        {
            m_GameReady.gameObject.SetActive(true);
            m_MapSetting.gameObject.SetActive(false);

            if (m_GameStart.gameObject.activeSelf)
            {
                m_GameStart.gameObject.SetActive(false);
            }

        }
    }

    void GameReady()
    {
        if (PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom || !PhotonNetwork.IsConnectedAndReady) { return; }

        if (m_SelectedCharacter == null)
        {
            NoticePopup.content = MessageCommon.Get("room.selectcharacter");
            Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
            return;
        }

        photonView.RPC("PlayerGameReadied", RpcTarget.All);
    }

    [PunRPC]
    void PlayerGameReadied()
    {
        m_IsGameReady = !m_IsGameReady;
        m_Ready.SetActive(m_IsGameReady);
    }

    void OpenMapSetting()
    {
        if (Core.plugs.Has<MapSettings>())
        {
            MapSettings settings = Core.plugs.Get<MapSettings>();
            settings.Open();
            settings.confirm = (a, b, c, d) => ChangedMapSetting(a, b, c, d);
            return;
        }

        Core.plugs.Load<MapSettings>(() =>
        {
            MapSettings settings = Core.plugs.Get<MapSettings>();
            settings.Open();
            settings.confirm = (a, b, c, d) => ChangedMapSetting(a, b, c, d);
        });
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
