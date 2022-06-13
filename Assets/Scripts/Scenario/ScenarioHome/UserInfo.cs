using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class UserInfo : MonoBehaviour
{

    [SerializeField] Text m_UserName;
    [SerializeField] Image m_UserFace;

    public void SetUserInfo(string userName)
    {
        PhotonNetwork.NickName = userName;
        m_UserName.text = userName;
    }


}
