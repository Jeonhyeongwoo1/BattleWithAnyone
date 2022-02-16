using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Photon.Pun;
using System.Text.RegularExpressions;
using UnityEngine.Events;

public class MapSettings : MonoBehaviour, IPlugable
{

    [Serializable]
    public class MapPreferences
    {
        public string roomTitle;
        public string mapTitle;
        public int roundTime;
        public int roundNumber;
    }

    [Serializable]
    public class MapInfo
    {
        public string mapTitle;
        public string mapInfo;
        public Transform check;
        public Text title;
        public Text info;
        public Button button;
    }

    public string plugName => nameof(MapSettings);

    public List<MapInfo> mapInfos = new List<MapInfo>();
    public UnityAction<string, string, int, int> confirm;

    [SerializeField] MapPreferences m_Preferences;
    [SerializeField] GameObject m_Map;
    [SerializeField] Button m_Confirm;
    [SerializeField] Button m_Cancel;
    [SerializeField] Transform m_MapContentView;
    [SerializeField] InputField m_RoomTitle;

    [Header("[Round Settings]")]
    [SerializeField] Toggle m_RoundTime120;
    [SerializeField] Toggle m_RoundTime150;
    [SerializeField] Toggle m_RoundTime180;
    [SerializeField] Toggle m_RoundNumber1;
    [SerializeField] Toggle m_RoundNumber3;
    [SerializeField] Toggle m_RoundNumber5;

    MapInfo m_SelectedMap;
    string m_NotContainSpecial = @"[^0-9a-zA-Zㄱ-ㅎㅏ-ㅣ가-힣]";

    public void Open(UnityAction done = null)
    {
        m_Map.SetActive(true);
        done?.Invoke();
    }

    public void Close(UnityAction done = null)
    {
        m_Map.SetActive(false);

        if (m_SelectedMap != null)
        {
            m_SelectedMap.button.gameObject.SetActive(false);
            m_SelectedMap.title.color = Color.white;
            m_SelectedMap.info.color = Color.white;
        }

        m_RoundNumber3.isOn = true;
        m_RoundTime150.isOn = true;
        m_Preferences.roundTime = 3;
        m_Preferences.roundNumber = 150;
        m_RoomTitle.text = null;

        done?.Invoke();
    }

    void OnClickConfirm()
    {
        Popups popups = Core.plugs.Get<Popups>();

        if (!PhotonNetwork.IsConnected)
		{
			NoticePopup.content = MessageCommon.Get("network.disconnect");
            popups.OpenPopupAsync<NoticePopup>();
            return;
        }

        if (string.IsNullOrEmpty(m_Preferences.mapTitle))
        {
            NoticePopup.content = MessageCommon.Get("map.selectmap");
            popups.OpenPopupAsync<NoticePopup>();
            return;
        }

        m_Preferences.roomTitle = m_RoomTitle.text;
        if (string.IsNullOrEmpty(m_Preferences.roomTitle))
        {
            NoticePopup.content = MessageCommon.Get("map.inputtitle");
            popups.OpenPopupAsync<NoticePopup>();
            return;
        }

        Regex regex = new Regex(m_NotContainSpecial);
        if (regex.IsMatch(m_Preferences.roomTitle))
        {
            NoticePopup.content = MessageCommon.Get("map.notcontain");
            popups.OpenPopupAsync<NoticePopup>();
            m_RoomTitle.text = null;
            return;
        }

        confirm?.Invoke(m_Preferences.mapTitle, m_Preferences.roomTitle, m_Preferences.roundTime, m_Preferences.roundNumber);
    }

    void OnSelectMap(MapInfo map)
    {
        if (map == null) { return; }

        if (m_SelectedMap != null)
        {
            m_SelectedMap.check.gameObject.SetActive(false);
            m_SelectedMap.title.color = Color.white;
            m_SelectedMap.info.color = Color.white;
        }

        map.check.gameObject.SetActive(true);
        map.title.color = Color.yellow;
        map.info.color = Color.yellow;
        m_SelectedMap = map;
        m_Preferences.mapTitle = map.mapTitle;
    }

    void SetRoundTime(int value, bool on)
    {
        if (!on) { return; }

        m_Preferences.roundTime = value;
    }

    void SetRoundNumber(int value, bool on)
    {
        if (!on) { return; }

        m_Preferences.roundNumber = value;
    }

    private void Awake()
    {
        m_Map.SetActive(false);
        m_RoundNumber1.onValueChanged.AddListener((b) => SetRoundNumber(1, b));
        m_RoundNumber3.onValueChanged.AddListener((b) => SetRoundNumber(3, b));
        m_RoundNumber5.onValueChanged.AddListener((b) => SetRoundNumber(5, b));
        m_RoundTime120.onValueChanged.AddListener((b) => SetRoundTime(120, b));
        m_RoundTime150.onValueChanged.AddListener((b) => SetRoundTime(150, b));
        m_RoundTime180.onValueChanged.AddListener((b) => SetRoundTime(180, b));
        m_Confirm.onClick.AddListener(OnClickConfirm);
        m_Cancel.onClick.AddListener(() => Close());
        mapInfos.ForEach((v) => v.button.onClick.AddListener(() => OnSelectMap(v)));

        //Awake Default
        m_Preferences.roundTime = 3;
        m_Preferences.roundNumber = 150;
    }

    // Start is called before the first frame update
    void Start()
    {
        Core.Ensure(() => Core.plugs.Loaded(this));
    }

    private void OnDestroy()
    {
        Core.Ensure(() => Core.plugs.Unloaded(this));
    }

}
