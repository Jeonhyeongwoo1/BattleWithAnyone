using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Scifi : MonoBehaviour, IModel
{
	public string Name => nameof(Battleground);
    public Transform[] playerCreatePoints { get; set; }

	public void LoadedModel(UnityAction done = null)
	{

	}

	public void UnLoadModel(UnityAction done = null)
	{

	}

	public void ReadyCamera(bool isMaster, UnityAction done = null) { }
	
	public IEnumerator ShootingCamera(bool isMaster, UnityAction done)
    {
        yield return null;
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
