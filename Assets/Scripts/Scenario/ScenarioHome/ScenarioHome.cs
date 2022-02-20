using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;

public class ScenarioHome : MonoBehaviourPunCallbacks, IScenario
{
    public string scenarioName => typeof(ScenarioHome).Name;
    public UserInfo userInfo;
    public RoomMenu roomMenu;

    [SerializeField, Range(0, 10)] float m_NetworkWaitTime = 10;
    [SerializeField] Button m_Exit;

    public string testname = "전형우";

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
        if (Core.networkManager.isLogined && PhotonNetwork.IsConnected)
        {
            StartCoroutine(WaitingForNetworkConnection(() => PhotonNetwork.JoinLobby()));
            done?.Invoke();
            return;
        }

        Core.networkManager.ConnectPhotonNetwork(() => StartCoroutine(WaitingForNetworkConnection(JoinLobby)));
        done?.Invoke();
    }

    void JoinLobby()
    {
        if (!Core.networkManager.isLogined)
        {
            Core.networkManager.SetPlayerName(testname);
        }

        PhotonNetwork.JoinLobby();
    }

    public void OnScenarioStop(UnityAction done)
    {
        StopAllCoroutines();
        done?.Invoke();
    }

    public override void OnJoinedLobby()
    {
        Core.networkManager.isLogined = true;
        BattleWtihAnyOneStarter.GetLoading()?.StopLoading();
        roomMenu.gameObject.SetActive(true);
        userInfo.SetUserInfo(Core.networkManager.userNickName);
        userInfo.gameObject.SetActive(true);
        m_Exit.gameObject.SetActive(true);
    }

    IEnumerator WaitingForNetworkConnection(UnityAction done)
    {
        float elapsed = 0;
        while (PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.ConnectedToMasterServer)
        {
            if (elapsed > m_NetworkWaitTime)
            {
                NoticePopup.content = MessageCommon.Get("network.failed");
                Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
                Core.scenario.OnLoadScenario(nameof(ScenarioLogin));
                BattleWtihAnyOneStarter.GetLoading()?.StopLoading();
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        done?.Invoke();
    }

    private void Start()
    {
        Core.Ensure(() => Core.scenario.OnLoadedScenario(this));
    }

    private void Awake()
    {
        Core.Ensure(() => Core.scenario.OnScenarioAwaked(this));
        m_Exit.onClick.AddListener(() => Application.Quit());
    }

}