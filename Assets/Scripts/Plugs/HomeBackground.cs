using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;

public class HomeBackground : MonoBehaviour, IPlugable
{
    public string plugName => nameof(HomeBackground);

    [SerializeField] CinemachineVirtualCamera m_ReadyVCam;
    [SerializeField] CinemachineVirtualCamera m_StandyVCam;
    
    public void Open(UnityAction done = null)
    {
        done?.Invoke();
    }

    public void Close(UnityAction done = null)
    {
        done?.Invoke();
    }

    public void ShootStandbyCamera(UnityAction done)
    {
        StartCoroutine(SwitchingCamera(m_StandyVCam, done));
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
