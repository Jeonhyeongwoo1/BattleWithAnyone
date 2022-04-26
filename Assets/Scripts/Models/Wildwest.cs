using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Wildwest : MonoBehaviour, IModel
{
	public string Name => nameof(Battleground);
    public Transform[] playerCreatePoints { get; set; }
	public Transform poolObjectCreatePoints => m_PoolObjectCreatePoint;

    [SerializeField] Transform m_PoolObjectCreatePoint;

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

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}
