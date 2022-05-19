using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UISoundPlay : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] AudioManager.UIType uIType;    

    public void OnPointerClick(PointerEventData eventData)
    {
        if (uIType == AudioManager.UIType.NONE) { return; }
        Core.audioManager.PlayUIAudio(uIType);
    }
}
