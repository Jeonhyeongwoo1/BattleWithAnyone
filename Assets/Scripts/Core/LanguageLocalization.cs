using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

public class LanguageLocalization : MonoBehaviour
{

    [Serializable]
    public struct UIMessage
    {
        public string id;
        public string kor;
        public string eng;
    }

    private List<UIMessage> m_UIMessage;
    private Dictionary<string, string> m_NotifyMessage;

    public string GetUIMessage(string id)
    {
        switch (Core.settings.language)
        {
            case XSettings.Language.ENG:
                return m_UIMessage.Find((v) => v.id == id).eng;
            case XSettings.Language.KOR:
                return m_UIMessage.Find((v) => v.id == id).kor;
            default:
                return null;
        }
    }

    public string GetNotifyMessage(string key)
    {
        if (!m_NotifyMessage.ContainsKey(key)) { return null; }

        return m_NotifyMessage[key];
    }

    void OnLanguageChanged(string key, object o)
    {
        switch (key)
        {
            case "Language.Changed":
                //OnLoadUIMessage();
                OnLoadNotifyMessage();
                break;
        }
    }

    void OnLoadNotifyMessage()
    {
        var jsonData = Resources.Load<TextAsset>(Core.settings.messageCommonPath);
        if (jsonData == null)
        {
            Debug.LogError("Message Json Data를 찾을 수 없습니다.");
            return;
        }
        
        m_NotifyMessage = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData.text);
    }

    void OnLoadUIMessage()
    {
        var jsonData = Resources.Load<TextAsset>(Core.settings.uiMessagePath);
        if (jsonData == null)
        {
            Debug.LogError("UI Message Json Data를 찾을 수 없습니다.");
            return;
        }

        m_UIMessage = JsonConvert.DeserializeObject<List<UIMessage>>(jsonData.text);
    }

    private void OnEnable()
    {
        Core.xEvent?.Watch("Language.Changed", OnLanguageChanged);
    }

    private void OnDisable()
    {
        Core.xEvent?.Stop("Language.Changed", OnLanguageChanged);
    }

    void Awake()
    {
        OnLoadNotifyMessage();
        OnLoadUIMessage();
    }
}
