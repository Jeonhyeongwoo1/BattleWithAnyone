using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class ScenarioTraining : MonoBehaviourPunCallbacks, IScenario
{
    public string scenarioName => nameof(ScenarioTraining);
    
    [SerializeField] string m_TestCharacter = "Cowboy";

    public void OnScenarioPrepare(UnityAction done)
    {
        Core.plugs.DefaultEnsure();
        if (!PhotonNetwork.IsConnectedAndReady && Core.networkManager.member == null) //DEV
        {
            BattleWtihAnyOneStarter.GetLoading()?.StartLoading();
            Core.networkManager.ConnectPhotonNetwork(() => Core.networkManager.WaitStateToConnectedToMasterServer(()=> PhotonNetwork.JoinLobby()) );
        }

        done?.Invoke();
    }

    public void OnScenarioStandbyCamera(UnityAction done)
    {
        done?.Invoke();
    }

    public void OnScenarioStart(UnityAction done)
    {        
        done?.Invoke();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Core.scenario.OnLoadScenario(nameof(ScenarioHome));
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Core.scenario.OnLoadScenario(nameof(ScenarioHome));
    }

    public override void OnJoinedLobby()
    {
        DevPhotonNetwork dev = new DevPhotonNetwork();
        dev.CreateRoom();
    }

    public override void OnCreatedRoom()
    {
        BattleWtihAnyOneStarter.GetLoading()?.StopLoading();
        Core.models.Load(nameof(TrainingModel), () =>
        {

            IModel model = Core.models.Get();
            Transform[] point = model.playerCreatePoints;
            GameObject go = PhotonNetwork.Instantiate(XSettings.chracterPath + m_TestCharacter, point[0].position, point[0].rotation, 0);
            if (go.TryGetComponent<PlayerController>(out var player))
            {
                player.CreateBullet();
                player.CreateCollisionEffect();
                Core.plugs.Load<XTheme>(() =>
                {
                    XTheme theme = Core.plugs.Get<XTheme>();
                    theme.Open();
                    theme.player = player;
                    theme.Crosshair.SetActive(true);
                    theme.bullet = player.bulletCount.ToString();
                });
            }
        });

    }

    public void OnScenarioStop(UnityAction done)
    {
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
