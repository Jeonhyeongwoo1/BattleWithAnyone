using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;
using Photon.Pun;
using System;

public class Mansion : MonoBehaviour, IModel
{
    public string Name => nameof(Mansion);
    public Transform[] playerCreatePoints => m_PlayerCreatePoints;
    public Transform poolObjectCreatePoints => m_PoolObjectCreatePoint;
    public Transform itemCreatePoint => m_InteractableItem.itemCreatePoint;

    [Serializable]
    public struct DollyCameraComponent
    {
        public CinemachineDollyCart dollyCart;
        public CinemachineSmoothPath smoothPath;
        public CinemachineVirtualCamera camera;
    }

    [SerializeField] ModelSkybox m_ModelSkybox;
    [SerializeField] Transform m_PoolObjectCreatePoint;
    [SerializeField] InteractableItemControl m_InteractableItem;
    [SerializeField] Transform[] m_PlayerCreatePoints;
    [SerializeField] DollyCameraComponent m_MasterDolly;
    [SerializeField] DollyCameraComponent m_PlayerDolly;


    public void LoadedModel(UnityAction done = null)
    {
        if (!PhotonNetwork.IsConnected) { return; }
        m_InteractableItem.CreateHelpableItems();
        m_InteractableItem.PlayInteractableItem();
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
        DollyCameraComponent dolly = isMaster ? m_MasterDolly : m_PlayerDolly;
        dolly.camera.Priority = 11;
        StartCoroutine(SwitchingCamera(dolly.camera, done));
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
        yield return new WaitForSeconds(1f);

        DollyCameraComponent dolly = isMaster ? m_MasterDolly : m_PlayerDolly;
        dolly.dollyCart.enabled = true;
        float pathlength = dolly.smoothPath.PathLength;
        float position = 0;

        while (pathlength > position)
        {
            position = dolly.dollyCart.m_Position;
            yield return null;
        }

        dolly.camera.Priority = 10;
        dolly.dollyCart.enabled = false;
        dolly.camera.enabled = false;

        Transform character = isMaster ? Core.state.masterCharacter : Core.state.playerCharacter;
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
