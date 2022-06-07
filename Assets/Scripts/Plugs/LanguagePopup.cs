using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public class LanguagePopup : BasePopup
{

    [Serializable]
    public struct Language : IEquatable<Language>
    {
        public string name;
        public XSettings.Language language;
        public Transform check;

        public bool Equals(Language language)
        {
            return this.name.Equals(language.name);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [SerializeField] List<Language> languages = new List<Language>();
    [SerializeField] Button m_Close;

    public void OnSelectLanguage(string lang)
    {
        Language l = languages.Find((v) => v.name == lang);
        if (l.Equals(default(Language))) { return; }

        Core.settings.language = l.language;
        Core.xEvent?.Raise("Language.Changed", null);
        languages.ForEach((v)=> v.check.gameObject.SetActive(false));
        l.check.gameObject.SetActive(true);
    }

    public override void OpenAsync(UnityAction done = null)
    {
        gameObject.SetActive(true);
        done?.Invoke();
    }

    public override void CloseAsync(UnityAction done = null)
    {
        gameObject.SetActive(false);
        done?.Invoke();
    }

    private void Awake()
    {
        m_Close.onClick.AddListener(() => CloseAsync(() => removeOpenedPopup?.Invoke()));
    }

    private void OnEnable()
    {
        Language l = languages.Find((v) => v.language == Core.settings.language);
        l.check.gameObject.SetActive(true);
    }


}
