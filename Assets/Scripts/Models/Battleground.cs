using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Battleground : BaseModel
{
    
    public override void LoadedModel(UnityAction done = null)
    {

    }

    public override void UnLoadModel(UnityAction done = null)
    {

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
