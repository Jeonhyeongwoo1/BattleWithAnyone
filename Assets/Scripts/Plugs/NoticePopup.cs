using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NoticePopup : BasePopup
{

    [SerializeField] Text m_Content;
    [SerializeField, Range(0, 3)] float m_NoticeOpenTime;


	public override void OpenAsync(UnityAction done = null) 
    {

    }

	public override void CloseAsync(UnityAction done = null) 
    {

    }



}
