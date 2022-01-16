using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Scifi : BaseModel
{
    public override void LoadModel(UnityAction done = null)
    {

    }

    public override void UnLoadModel(UnityAction done = null)
    {

    }

    private void Start()
    {
        Core.Ensure(() => Core.models.OnLoaded(this));
    }

    private void OnDestroy()
    {
        Core.Ensure(() => Core.models.Unloaded(this));
    }

}
