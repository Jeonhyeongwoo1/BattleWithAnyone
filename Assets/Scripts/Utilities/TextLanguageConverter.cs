using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextLanguageConverter : MonoBehaviour
{
    public string textValue => m_Text?.text;
    public string id => m_Id;

    [SerializeField] string m_Id;
    [SerializeField] Text m_Text;

    void SetLanguage()
    {
        string value = Core.language.GetUIMessage(m_Id);

        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("Can't convert language : " + m_Id);
            return;
        }

        if (m_Text != null)
        {
            m_Text.text = value;
        }
    }

    void OnLanguageChanged(string key, object o)
    {
        switch (key)
        {
            case "Language.Changed":
                SetLanguage();
                break;
        }
    }

    private void OnEnable()
    {
        SetLanguage();
        Core.xEvent?.Watch("Language.Changed", OnLanguageChanged);
    }

    private void OnDisable()
    {
        Core.xEvent?.Stop("Language.Changed", OnLanguageChanged);
    }

}
