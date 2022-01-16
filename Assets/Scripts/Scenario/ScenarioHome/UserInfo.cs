using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInfo : MonoBehaviour
{

    [SerializeField] Text m_UserName;
    [SerializeField] Image m_UserFace;

    public void SetUserInfo(string userName)
    {
        m_UserName.text = userName;
    }


}
