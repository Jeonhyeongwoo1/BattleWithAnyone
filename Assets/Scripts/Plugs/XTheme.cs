using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class XTheme : MonoBehaviour, IPlugable
{

	public string plugName => nameof(XTheme);

    [SerializeField] GameObject m_XTheme;

	public void Open(UnityAction done = null)
	{
        
	}

	public void Close(UnityAction done = null)
	{

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
