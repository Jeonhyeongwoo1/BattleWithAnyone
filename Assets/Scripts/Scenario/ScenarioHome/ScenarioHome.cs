using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Photon.Pun;

public class ScenarioHome : MonoBehaviour, IScenario
{
    public string scenarioName => typeof(ScenarioHome).Name;
    public UserInfo userInfo;
    public RoomMenu roomMenu;

    [SerializeField] Button m_Exit;
    [SerializeField] Button m_GamePlay;
    [SerializeField] RectTransform m_LoginForm;
    [SerializeField] InputField m_Id;
    [SerializeField] Button m_LoginBtn;

    string m_NotSpecialPattern = @"[^0-9a-zA-Zㄱ-ㅎㅏ-ㅣ가-힣]";

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
        Core.networkManager.ConnectPhotonNetwork(() => ConnectedPhotonNetwork(null));
        done?.Invoke();
    }

    void ConnectedPhotonNetwork(UnityAction done)
    {
        BattleWtihAnyOneStarter.GetLoading()?.StopLoading();
        m_GamePlay.gameObject.SetActive(true);
        m_Exit.gameObject.SetActive(true);
        done?.Invoke();
    }

    public void OnScenarioStop(UnityAction done)
    {
        StopAllCoroutines();
        done?.Invoke();
    }

    void OnClickLoginBtn()
    {
        if (string.IsNullOrEmpty(m_Id.text))
        {
            Debug.Log("There is no id value");
            return;
        }

        Regex regex = new Regex(m_NotSpecialPattern);
        if (regex.IsMatch(m_Id.text))
        {
            Debug.Log("There is special key");
            m_Id.text = null;
            return;
        }

        Core.networkManager.SetPlayerName(m_Id.text);
        userInfo.SetUserInfo(m_Id.text);
        userInfo.gameObject.SetActive(true);
        m_LoginForm.gameObject.SetActive(false);
        PhotonNetwork.JoinLobby();
        roomMenu.OnEnableRoomMenu();
    }

    void OnClickGamePlay()
    {
        roomMenu.gameObject.SetActive(true);
        m_LoginForm.gameObject.SetActive(true);
        m_GamePlay.gameObject.SetActive(false);
    }

    private void Start()
    {
        m_GamePlay.onClick.AddListener(OnClickGamePlay);
        m_LoginBtn.onClick.AddListener(OnClickLoginBtn);
        Core.Ensure(() => Core.scenario.OnLoadedScenario(this));
    }

    private void Awake()
    {
        Core.Ensure(() => Core.scenario.OnScenarioAwaked(this));
    }

}