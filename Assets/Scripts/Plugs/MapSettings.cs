using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Photon.Pun;
using System.Text.RegularExpressions;
using UnityEngine.Events;

public class MapSettings : MonoBehaviour, IPlugable
{
    enum RoundTimeType
    {
        ROUND120 = 120,
        ROUND150 = 150,
        ROUND180 = 180
    }

    enum NumberOfRoundType
    {
        ROUND1 = 1,
        ROUND3 = 3,
        ROUND5 = 5
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
    public UnityAction<string> confirm;

    [SerializeField] GameObject m_Map;
    [SerializeField] Button m_Confirm;
    [SerializeField] Button m_Cancel;
    [SerializeField] Transform m_MapContentView;
    [SerializeField] InputField m_RoomTitle;
    [SerializeField] GameObject m_RoomTitleForm;

    [Header("[Round Settings]")]
    [SerializeField] Toggle m_RoundTime120;
    [SerializeField] Toggle m_RoundTime150;
    [SerializeField] Toggle m_RoundTime180;
    [SerializeField] Toggle m_NumberOfRound1;
    [SerializeField] Toggle m_NumberOfRound3;
    [SerializeField] Toggle m_NumberOfRound5;

    MapInfo m_SelectedMap;
    string m_NotContainSpecial = @"[^0-9a-zA-Zㄱ-ㅎㅏ-ㅣ가-힣]";
    GamePlayManager.MapPreferences m_Preferences = new GamePlayManager.MapPreferences();

    public void Open(UnityAction done = null)
    {
        m_Map.SetActive(true);
        string scenario = Core.scenario.GetCurScenarioName();

        if (scenario == nameof(ScenarioHome))
        {
            Init();
        }
        else if (scenario == nameof(ScenarioRoom))
        {
            if (Core.gameManager.HasMapPreference())
            {
                GamePlayManager.MapPreferences preferences = Core.gameManager.GetMapPreference();
                string mapName = preferences.mapName;
                int numberOfRound = preferences.numberOfRound;
                int roundTime = preferences.roundTime;

                MapInfo mapInfo = mapInfos.Find((v) => v.mapTitle == mapName);
                if (mapInfo != null)
                {
                    OnSelectMap(mapInfo);
                }

                RoundTimeType rType = (RoundTimeType)Enum.Parse(typeof(RoundTimeType), roundTime.ToString());
                NumberOfRoundType nType = (NumberOfRoundType)Enum.Parse(typeof(NumberOfRoundType), numberOfRound.ToString());
                OnNumberOfRound(nType, true);
                OnRoundTime(rType, true);
            }
            else
            {
                Init();
            }

            m_RoomTitleForm.SetActive(false);
        }

        done?.Invoke();
    }

    public void Close(UnityAction done = null)
    {
        m_Map.SetActive(false);
        done?.Invoke();
    }

    void OnClickConfirm()
    {
        Popups popups = Core.plugs.Get<Popups>();

        if (!PhotonNetwork.IsConnected)
        {
            NoticePopup.content = MessageCommon.Get("network.disconnect");
            popups?.OpenPopupAsync<NoticePopup>();
            return;
        }

        m_Preferences.mapName = m_SelectedMap?.mapTitle;
        if (string.IsNullOrEmpty(m_Preferences.mapName))
        {
            NoticePopup.content = MessageCommon.Get("map.selectmap");
            popups?.OpenPopupAsync<NoticePopup>();
            return;
        }

        string roomName = null;
        if (Core.scenario.GetCurScenarioName() != nameof(ScenarioRoom))
        {
            roomName = m_RoomTitle.text;
            if (string.IsNullOrEmpty(roomName))
            {
                NoticePopup.content = MessageCommon.Get("map.inputtitle");
                popups?.OpenPopupAsync<NoticePopup>();
                return;
            }

            Regex regex = new Regex(m_NotContainSpecial);
            if (regex.IsMatch(roomName))
            {
                NoticePopup.content = MessageCommon.Get("map.notcontain");
                popups?.OpenPopupAsync<NoticePopup>();
                m_RoomTitle.text = null;
                return;
            }
        }

        Core.gameManager.SetMapPreference(m_Preferences.mapName, m_Preferences.numberOfRound, m_Preferences.roundTime);
        confirm?.Invoke(roomName);
        Close();
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
    }

    void OnRoundTime(RoundTimeType value, bool on)
    {
        if (!on) { return; }

        switch (value)
        {
            case RoundTimeType.ROUND120:
                m_RoundTime120.isOn = on;
                m_Preferences.roundTime = 120;
                break;
            case RoundTimeType.ROUND150:
                m_RoundTime150.isOn = on;
                m_Preferences.roundTime = 150;
                break;
            case RoundTimeType.ROUND180:
                m_RoundTime180.isOn = on;
                m_Preferences.roundTime = 180;
                break;
            default:
                m_RoundTime150.isOn = on;
                m_Preferences.roundTime = 150;
                break;
        }
    }

    void OnNumberOfRound(NumberOfRoundType value, bool on)
    {

        switch (value)
        {
            case NumberOfRoundType.ROUND1:
                m_NumberOfRound1.isOn = on;
                m_Preferences.numberOfRound = 1;
                break;
            case NumberOfRoundType.ROUND3:
                m_NumberOfRound3.isOn = on;
                m_Preferences.numberOfRound = 3;
                break;
            case NumberOfRoundType.ROUND5:
                m_NumberOfRound5.isOn = on;
                m_Preferences.numberOfRound = 5;
                break;
            default:
                m_NumberOfRound3.isOn = on;
                m_Preferences.numberOfRound = 3;
                break;
        }
    }

    void Init()
    {
        if (m_SelectedMap != null)
        {
            m_SelectedMap.check.gameObject.SetActive(false);
            m_SelectedMap.title.color = Color.white;
            m_SelectedMap.info.color = Color.white;
            m_SelectedMap = null;
        }

        if (!m_RoomTitleForm.activeSelf)
        {
            m_RoomTitleForm.SetActive(true);
        }

        if (!string.IsNullOrEmpty(m_RoomTitle.text))
        {
            m_RoomTitle.text = null;
        }

        OnRoundTime(RoundTimeType.ROUND150, true);
        OnNumberOfRound(NumberOfRoundType.ROUND3, true);
    }

    private void Awake()
    {
        m_NumberOfRound1.onValueChanged.AddListener((b) => OnNumberOfRound(NumberOfRoundType.ROUND1, b));
        m_NumberOfRound3.onValueChanged.AddListener((b) => OnNumberOfRound(NumberOfRoundType.ROUND3, b));
        m_NumberOfRound5.onValueChanged.AddListener((b) => OnNumberOfRound(NumberOfRoundType.ROUND5, b));
        m_RoundTime120.onValueChanged.AddListener((b) => OnRoundTime(RoundTimeType.ROUND120, b));
        m_RoundTime150.onValueChanged.AddListener((b) => OnRoundTime(RoundTimeType.ROUND150, b));
        m_RoundTime180.onValueChanged.AddListener((b) => OnRoundTime(RoundTimeType.ROUND180, b));
        m_Confirm.onClick.AddListener(OnClickConfirm);
        m_Cancel.onClick.AddListener(() => Close());
        mapInfos.ForEach((v) => v.button.onClick.AddListener(() => OnSelectMap(v)));
        m_Map.SetActive(false);
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
