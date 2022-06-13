using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Apple Login을 했는데 Name을 못 받아오는 경우에 활성화한다.
public class UserNameInput : MonoBehaviour
{
    [SerializeField] UserInfo m_UserInfo;
    [SerializeField] InputField m_UserName;
    [SerializeField] Button m_Confirm;

    void OnConfirm()
    {
        string userName = m_UserName.text;
        if(string.IsNullOrEmpty(userName))
        {
            NoticePopup.content = Core.language.GetNotifyMessage("login.inputname");
            Core.plugs.Get<Popups>()?.OpenPopupAsync<NoticePopup>();
            return;
        }
        
        string id = Core.networkManager.appleAuth?.appleUser;
        Core.networkManager.ReqUpdateUserName(id, userName, OnSuccessed, OnFailed);
    }

    void OnSuccessed(string data)
    {
        if(data == "success")
        {
            m_UserInfo.SetUserInfo(m_UserName.text);   
        }
        else
        {
            m_UserInfo.SetUserInfo(Core.networkManager.appleAuth?.appleUser);
        }

        gameObject.SetActive(false);
    }

    void OnFailed(string data)
    {
        //Apple UserId를 넣는다.
        m_UserInfo.SetUserInfo(Core.networkManager.appleAuth?.appleUser);
        gameObject.SetActive(false);
    }

    private void Awake()
    {
        m_Confirm.onClick.AddListener(OnConfirm);
    }

}
