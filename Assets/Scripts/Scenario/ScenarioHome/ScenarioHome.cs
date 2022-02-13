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

    [SerializeField] Button m_Exit;

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

        Core.networkManager.ConnectPhotonNetwork(() => PhotonNetwork.JoinLobby());
        done?.Invoke();
    }

    public void OnScenarioStop(UnityAction done)
    {
        StopAllCoroutines();
        done?.Invoke();
    }

    public override void OnJoinedLobby()
    {
        BattleWtihAnyOneStarter.GetLoading()?.StopLoading();
        roomMenu.gameObject.SetActive(true);
        userInfo.gameObject.SetActive(true);
    }

    IEnumerator WaitingForNetworkConnection(UnityAction done)
    {
        while (PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.ConnectedToMasterServer)
        {
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
    }

}