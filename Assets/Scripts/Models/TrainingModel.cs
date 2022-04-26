using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TrainingModel : MonoBehaviour, IModel
{

    public string Name { get => nameof(TrainingModel); }
    public Transform[] playerCreatePoints => m_PlayerCreatePoints; 
    public Transform poolObjectCreatePoints => m_PoolObjectCreatePoint; 

    [SerializeField] Transform[] m_PlayerCreatePoints;
    [SerializeField] Transform m_PoolObjectCreatePoint;

    public void LoadedModel(UnityAction done = null) { }
    
    public void UnLoadModel(UnityAction done = null) { }
    
    public void ReadyCamera(bool isMaster, UnityAction done = null) { }

    public IEnumerator ShootingCamera(bool isMaster, UnityAction done)
    {
        yield return null;

    }


    void Awake()
    {
        Core.Ensure(() => Core.models.OnLoaded(this));
    }

    private void OnDestroy()
    {
        Core.Ensure(() => Core.models.Unloaded(this));
    }
}
