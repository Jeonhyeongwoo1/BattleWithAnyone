using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;

public class ScenarioHome : MonoBehaviour, IScenario
{
    public string scenarioName => typeof(ScenarioHome).Name;
    public bool isCompleteLoading = false;
    public UserInfo userInfo;
    public RoomMenu roomMenu;

    [SerializeField] Button m_Exit;
    [SerializeField] GameObject m_LoadingContent;
    [SerializeField] Image m_Loading;
    [SerializeField] Button m_GamePlay;
    [SerializeField] RectTransform m_LoginForm;
    [SerializeField] InputField m_Id;
    [SerializeField] Button m_LoginBtn;

    public void OnScenarioPrepare(UnityAction done)
    {
        m_LoadingContent.SetActive(true);
        StartCoroutine(Loading());
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
        StopCoroutine(Loading());
        m_LoadingContent.SetActive(false);
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

    IEnumerator Loading(UnityAction done = null)
    {
        float elapsed = 0;
        float duration = 1f;

        while (!isCompleteLoading)
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                m_Loading.fillAmount = Mathf.Lerp(0, 1, elapsed / duration);
                yield return null;
            }

            elapsed = 0;
        }

        done?.Invoke();
    }

}