using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Photon.Pun;
using Cinemachine;

public class Scifi : MonoBehaviour, IModel
{
    public string Name => nameof(Scifi);
    public Transform[] playerCreatePoints => m_PlayerCreatePoints;
    public Transform poolObjectCreatePoints => m_PoolObjectCreatePoint;
    public Transform itemCreatePoint => m_InteractableItem.itemCreatePoint;

    [SerializeField] ModelSkybox m_ModelSkybox;
    [SerializeField] Transform m_PoolObjectCreatePoint;
    [SerializeField] InteractableItemControl m_InteractableItem;
    [SerializeField] Transform[] m_PlayerCreatePoints;
    [SerializeField] CinemachineVirtualCamera m_MasterCam;
    [SerializeField] CinemachineVirtualCamera m_PlayerCam;


    public void LoadedModel(UnityAction done = null)
    {
        if (!PhotonNetwork.IsConnected) { return; }
        m_InteractableItem.CreateHelpableItems();
        m_ModelSkybox.SetupSkybox(false);
        if (Camera.main.TryGetComponent<FadeBlockingObject>(out var fadingModel))
        {
            fadingModel.StartCheckBlockingObject();
        }

        done?.Invoke();
    }

    public void UnLoadModel(UnityAction done = null)
    {
        m_ModelSkybox.SetupSkybox(true);
        if (Camera.main.TryGetComponent<FadeBlockingObject>(out var fadingModel))
        {
            fadingModel.StopCheckBlockingObject();
        }
        
        done?.Invoke();
    }

    public void ReadyCamera(bool isMaster, UnityAction done = null)
    {
        CinemachineVirtualCamera cam = isMaster ? m_MasterCam : m_PlayerCam;
        cam.Priority = 11;
        StartCoroutine(SwitchingCamera(cam, done));
    }

    IEnumerator SwitchingCamera(CinemachineVirtualCamera cam, UnityAction done)
    {
        while (!(CinemachineCore.Instance.IsLive(cam) && !CinemachineCore.Instance.GetActiveBrain(0).IsBlending))
        {
            if (!CinemachineCore.Instance.IsLive(cam)) { cam.enabled = false; cam.enabled = true; }
            yield return null;
        }

        done?.Invoke();
    }

    public IEnumerator ShootingCamera(bool isMaster, UnityAction done)
    {
        Transform character = isMaster ? Core.state.masterCharacter : Core.state.playerCharacter;
        CinemachineVirtualCamera cam = isMaster ? m_MasterCam : m_PlayerCam;
        cam.Priority = 10;
        if (character.TryGetComponent<PlayerController>(out var contorller))
        {
            CinemachineVirtualCamera camera = contorller.GetCamera();
            yield return SwitchingCamera(camera, done);
        }
        else
        {
            done?.Invoke();
        }
    }
    void Awake()
    {
        Core.models?.OnLoaded(this);
    }

    private void OnDestroy()
    {
        Core.models?.Unloaded(this);
    }

}
