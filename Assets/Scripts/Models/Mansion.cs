using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Mansion : MonoBehaviour, IModel
{
	public string Name => nameof(Battleground);

	public void LoadedModel(UnityAction done = null)
	{

	}

	public void UnLoadModel(UnityAction done = null)
	{

	}

	public void ReadyCamera(bool isMaster, UnityAction done = null) { }
	public void CreateCharacter(Transform master, Transform player, UnityAction done = null) { }

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
