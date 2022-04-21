using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class XTheme : MonoBehaviour, IPlugable
{
    public string plugName => nameof(XTheme);

    public int roundTime
    {
        set
        {
            m_RoundTime.text = value.ToString();
        }
    }

	private PlayerController playerController;

    public GameTimer gameTimer;

    [SerializeField] RectTransform m_StageInfo;
    [SerializeField] RectTransform m_CharacterInfo;

    [Header("[Stage Info]")]
    [SerializeField] Text m_MasterName;
    [SerializeField] Text m_PlayerName;
    [SerializeField] Text m_RoundTime;
    [SerializeField] Text m_MasterWinCount;
    [SerializeField] Text m_PlayerWinCount;

    [Header("[Components]")]
    [SerializeField] JoyStick m_Joystick;
    [SerializeField] Button m_Roll;
    [SerializeField] Button m_Attack;
    [SerializeField] Button m_Reload;
    [SerializeField] Button m_Jump;
    [SerializeField] Text m_Bullet;
    [SerializeField] Slider m_HealthBar;
    [SerializeField] TextMeshProUGUI m_HealthValue;

    [SerializeField] AnimationCurve m_Curve;
    [SerializeField] AnimationCurve m_ScaleCurve;
    [SerializeField, Range(0, 1)] float m_OpenCloseDuration = 0.3f;
    [SerializeField] Vector3 m_StageInfo_Open_Pos = new Vector3(0, -35f, 0);
    [SerializeField] Vector3 m_StageInfo_Close_Pos = new Vector3(0, 43f, 0);
    [SerializeField] Vector3 m_CharacterInfo_Open_Pos = new Vector3(0, -165, 0);
    [SerializeField] Vector3 m_CharacterInfo_Close_Pos = new Vector3(0, -300, 0);

    public const string InteractableJump = "Jump";
    public const string Health = "Health";

    public void SetPlayersName(string master, string player)
    {
        m_MasterName.text = master;
        m_PlayerName.text = player;
    }

    IEnumerator Opening(UnityAction done)
    {
        int count = 0;
        StartCoroutine(CoUtilize.VLerp((v) => m_StageInfo.anchoredPosition = v, m_StageInfo_Close_Pos, m_StageInfo_Open_Pos, m_OpenCloseDuration, () => count++, m_Curve));
        StartCoroutine(CoUtilize.VLerp((v) => m_CharacterInfo.anchoredPosition = v, m_CharacterInfo_Close_Pos, m_CharacterInfo_Open_Pos, m_OpenCloseDuration, () => count++, m_Curve));
        StartCoroutine(CoUtilize.VLerpUnclamped((v) => m_Joystick.transform.localScale = v, Vector3.zero, Vector3.one, m_OpenCloseDuration, () => count++, m_ScaleCurve));
        StartCoroutine(CoUtilize.VLerpUnclamped((v) => m_Roll.transform.localScale = v, Vector3.zero, Vector3.one, m_OpenCloseDuration, () => count++, m_ScaleCurve));
        StartCoroutine(CoUtilize.VLerpUnclamped((v) => m_Attack.transform.localScale = v, Vector3.zero, Vector3.one, m_OpenCloseDuration, () => count++, m_ScaleCurve));
        StartCoroutine(CoUtilize.VLerpUnclamped((v) => m_Reload.transform.localScale = v, Vector3.zero, Vector3.one, m_OpenCloseDuration, () => count++, m_ScaleCurve));

        while (count != 6) { yield return null; }
        done?.Invoke();
    }

    IEnumerator Closing(UnityAction done)
    {
        int count = 0;
        StartCoroutine(CoUtilize.VLerp((v) => m_StageInfo.anchoredPosition = v, m_StageInfo_Open_Pos, m_StageInfo_Close_Pos, m_OpenCloseDuration, () => count++, m_Curve));
        StartCoroutine(CoUtilize.VLerp((v) => m_CharacterInfo.anchoredPosition = v, m_CharacterInfo_Open_Pos, m_CharacterInfo_Close_Pos, m_OpenCloseDuration, () => count++, m_Curve));
        StartCoroutine(CoUtilize.VLerpUnclamped((v) => m_Joystick.transform.localScale = v, Vector3.one, Vector3.zero, m_OpenCloseDuration, () => count++, m_Curve));
        StartCoroutine(CoUtilize.VLerpUnclamped((v) => m_Roll.transform.localScale = v, Vector3.one, Vector3.zero, m_OpenCloseDuration, () => count++, m_Curve));
        StartCoroutine(CoUtilize.VLerpUnclamped((v) => m_Attack.transform.localScale = v, Vector3.one, Vector3.zero, m_OpenCloseDuration, () => count++, m_Curve));
        StartCoroutine(CoUtilize.VLerpUnclamped((v) => m_Reload.transform.localScale = v, Vector3.one, Vector3.zero, m_OpenCloseDuration, () => count++, m_Curve));

        while (count != 6) { yield return null; }
        done?.Invoke();
    }

    void Init()
    {
        m_Joystick.transform.localScale = Vector3.zero;
        m_Roll.transform.localScale = Vector3.zero;
        m_Attack.transform.localScale = Vector3.zero;
        m_Reload.transform.localScale = Vector3.zero;

        m_StageInfo.anchoredPosition = m_StageInfo_Close_Pos;
        m_CharacterInfo.anchoredPosition = m_CharacterInfo_Close_Pos;
        Transform player = PhotonNetwork.IsMasterClient ? Core.state.masterCharacter : Core.state.playerCharacter;
        if(player.TryGetComponent<PlayerController>(out var component))
        {
            playerController = component;
        }
    }

    public void Open(UnityAction done = null)
    {
        Init();
        transform.GetChild(0).gameObject.SetActive(true);
        StartCoroutine(Opening(done));
    }

    public void Close(UnityAction done = null)
    {
        StartCoroutine(Closing(() =>
        {
            transform.GetChild(0).gameObject.SetActive(false);
            done?.Invoke();
        }));
    }

    void OnValueChanged(string key, object o)
    {
        int value = (int)o;
        switch (key)
        {
            case nameof(Core.state.playerWinCount):
                m_PlayerWinCount.text = value.ToString();
                break;
            case nameof(Core.state.masterWinCount):
                m_MasterWinCount.text = value.ToString();
                break;
        }
    }

    void DoAttack()
    {
		m_Attack.interactable = false;
		playerController.Attack(()=> m_Attack.interactable = true);
    }

    void DoRoll()
    {
        m_Roll.interactable = false;
        playerController.Roll(() => m_Roll.interactable = true);
    }

    void DoReload()
    {
        m_Reload.interactable = false;
        playerController.Reload(() => m_Reload.interactable = true);
    }

    void DoJump()
    {
        m_Jump.interactable = false;
        playerController.Jump(() => m_Jump.interactable = true);
    }

    void Awake()
    {
        //	transform.GetChild(0).gameObject.SetActive(false);
        m_Roll.onClick.AddListener(DoRoll);
        m_Reload.onClick.AddListener(DoReload);
        m_Attack.onClick.AddListener(DoAttack);
        m_Jump.onClick.AddListener(DoJump);
    }

    // Start is called before the first frame update
    void Start()
    {
        Core.Ensure(() => Core.plugs.Loaded(this));

        if(XSettings.isCharacterTest)
        {
            playerController = FindObjectOfType<PlayerController>();
        }
    }

    private void OnDestroy()
    {
        Core.Ensure(() => Core.plugs.Unloaded(this));
    }

    void OnRaisedEvent(string key, object o)
    {
        switch (key)
        {
            case InteractableJump:
                bool interactable = (bool)o;
                m_Jump.interactable = interactable;
                break;
            case Health:
                float amount = (float)o;
                if (amount < 0) { amount = 0; }
                m_HealthBar.value = amount;
                m_HealthValue.text = string.Format("{0}", amount);
                break;
        }
    }

    void OnEnable()
    {
        Core.state?.Listen(nameof(Core.state.playerWinCount), OnValueChanged);
        Core.state?.Listen(nameof(Core.state.masterWinCount), OnValueChanged);
        Core.xEvent?.Watch(InteractableJump, OnRaisedEvent);
        Core.xEvent?.Watch(Health, OnRaisedEvent);
    }

    void OnDisable()
    {
        Core.state?.Stop(nameof(Core.state.playerWinCount), OnValueChanged);
        Core.state?.Stop(nameof(Core.state.masterWinCount), OnValueChanged);
        Core.xEvent?.Stop(InteractableJump, OnRaisedEvent);
        Core.xEvent?.Stop(Health, OnRaisedEvent);
    }

    [ContextMenu("Open")]
    public void OpenTest()
    {
        StartCoroutine(Opening(null));
    }

    [ContextMenu("Close")]
    public void CloseTest()
    {
        StartCoroutine(Closing(null));
    }

}
