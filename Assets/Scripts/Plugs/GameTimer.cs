using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Timers;

public class GameTimer : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] Text m_Timer;

    private string s;

    IEnumerator Timer()
    {
        float time = 150;
        while(time != 0)
        {
            s = System.DateTime.Now.Second.ToString();
            yield return null;
        }

       
    }

    public void STartTimer()
    {
        photonView.RPC("OnLoaded", RpcTarget.All);
        StartCoroutine(Timer());
    }

    void OnLoaded()
    {
        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(s);
        }
        else
        {
            print(stream.ReceiveNext());
        }
    }

    [ContextMenu("TEST")]
    public void TEST()
    {
        s = "TEST";
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnDisable()
    {
        base.OnDisable();
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
