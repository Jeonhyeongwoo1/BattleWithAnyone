using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class GameSettings : BasePopup
{
    enum GraphicLevel
    {
        Low = 1,
        Medium = 2,
        High = 5
    }

    [SerializeField] Button m_Close;

    //Settings
    [SerializeField] Toggle m_Low;
    [SerializeField] Toggle m_Medium;
    [SerializeField] Toggle m_High;
    [SerializeField] TextMeshProUGUI m_LowTxt;
    [SerializeField] TextMeshProUGUI m_MediumTxt;
    [SerializeField] TextMeshProUGUI m_HighTxt;
    [SerializeField] Color m_Selected;
    [SerializeField] Color m_DisSelect;

    //Sounds
    [SerializeField] Slider m_BGM;
    [SerializeField] Slider m_EffectSound;
    [SerializeField] Transform m_BGMMute;
    [SerializeField] Transform m_EffectSoundMute;
    [SerializeField] Image m_BGMFill;
    [SerializeField] Image m_BGMHandle;
    [SerializeField] Image m_EffectFill;
    [SerializeField] Image m_EffectHandle;
    [SerializeField] TextMeshProUGUI m_BGMOff;
    [SerializeField] TextMeshProUGUI m_EffectSoundMuteOff;
    [SerializeField] TextMeshProUGUI m_BGMOn;
    [SerializeField] TextMeshProUGUI m_EffectSoundMuteOn;
    [SerializeField] Sprite m_MuteOn;
    [SerializeField] Sprite m_MuteOff;
    [SerializeField] Sprite m_FillMuteOn;
    [SerializeField] Sprite m_FillMuteOff;
    [SerializeField] Sprite m_HandleMuteOn;
    [SerializeField] Sprite m_HandleMuteOff;

    //Player
    [SerializeField] Slider m_PlayerSensitivity;
    [SerializeField] Text m_PlayerSensitivityValue;
    [SerializeField] Slider m_PlayerSound;
    [SerializeField] Transform m_PlayerSoundMute;
    [SerializeField] Image m_PlayerSoundFill;
    [SerializeField] Image m_PlayerSoundHandle;

    [SerializeField] float m_MuteOffPosX = -40;
    [SerializeField] float m_MuteOnPosX = 40;
    [SerializeField, Range(0, 1)] float m_SoundOnOffDuration = 0.3f;
    [SerializeField] AnimationCurve m_Curve;

    public void OnMutePlayerSound(bool mute)
    {
        float start = mute ? m_MuteOffPosX : m_MuteOnPosX;
        float end = mute ? m_MuteOnPosX : m_MuteOffPosX;

        if (m_PlayerSoundMute.TryGetComponent<Image>(out var image))
        {
            image.sprite = mute ? m_MuteOn : m_MuteOff;
        }

        StartCoroutine(CoUtilize.Lerp((v) => m_PlayerSoundMute.localPosition = new Vector3(v, 0, 0), start, end, m_SoundOnOffDuration, null, m_Curve));
        m_PlayerSound.enabled = mute;
        m_PlayerSoundFill.sprite = mute ? m_FillMuteOn : m_FillMuteOff;
        m_PlayerSoundHandle.sprite = mute ? m_HandleMuteOn : m_HandleMuteOff;
        Core.state.playerSoundMute = !mute;
    }

    public void OnMuteBgm(bool mute)
    {
        float start = mute ? m_MuteOffPosX : m_MuteOnPosX;
        float end = mute ? m_MuteOnPosX : m_MuteOffPosX;

        if(m_BGMMute.TryGetComponent<Image>(out var image))
        {
            image.sprite = mute ? m_MuteOn : m_MuteOff;
        }

        StartCoroutine(CoUtilize.Lerp((v) => m_BGMMute.localPosition = new Vector3(v, 0, 0), start, end, m_SoundOnOffDuration, null, m_Curve));
        m_BGM.enabled = mute;
        m_BGMFill.sprite = mute ? m_FillMuteOn : m_FillMuteOff;
        m_BGMHandle.sprite = mute ? m_HandleMuteOn : m_HandleMuteOff;
        Core.xEvent?.Raise("Audio.Background.Mute", !mute);
    }

    public void OnMuteEffect(bool mute)
    {
        float start = mute ? m_MuteOffPosX : m_MuteOnPosX;
        float end = mute ? m_MuteOnPosX : m_MuteOffPosX;

        if (m_EffectSoundMute.TryGetComponent<Image>(out var image))
        {
            image.sprite = mute ? m_MuteOn : m_MuteOff;
        }

        StartCoroutine(CoUtilize.Lerp((v) => m_EffectSoundMute.localPosition = new Vector3(v, 0, 0), start, end, m_SoundOnOffDuration, null, m_Curve));
        m_EffectSound.enabled = mute;
        m_EffectFill.sprite = mute ? m_FillMuteOn : m_FillMuteOff;
        m_EffectHandle.sprite = mute ? m_HandleMuteOn : m_HandleMuteOff;
        Core.xEvent?.Raise("Audio.Effect.Mute", !mute);
    }
    
    
    public void OnChangeGraphicsLevel(int value)
    {
        if (QualitySettings.GetQualityLevel() == value) { return; }

        QualitySettings.SetQualityLevel(value);

        m_LowTxt.color = m_DisSelect;
        m_MediumTxt.color = m_DisSelect;
        m_HighTxt.color = m_DisSelect;

        switch (value)
        {
            case (int)GraphicLevel.Low:
                m_Low.isOn = true;
                m_LowTxt.color = m_Selected;
                break;
            case (int)GraphicLevel.Medium:
                m_Medium.isOn = true;
                m_MediumTxt.color = m_Selected;
                break;
            case (int)GraphicLevel.High:
                m_High.isOn = true;
                m_HighTxt.color = m_Selected;
                break;
        }
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

    void OnPlayerSensitivityChanged(float value)
    {
        float sensitivity = m_PlayerSensitivity.value * 10;
        if(sensitivity <= 1)
        {
            sensitivity = 1;
        }

        m_PlayerSensitivityValue.text = string.Format("{0:0.0}", sensitivity);
        Core.state.playerRotSensitivity = sensitivity;
    }

    private void Awake()
    {
        m_Close.onClick.AddListener(() => CloseAsync(() => removeOpenedPopup?.Invoke()));
        m_Low.onValueChanged.AddListener((on) => { if (on) OnChangeGraphicsLevel(1); });
        m_Medium.onValueChanged.AddListener((on) => { if (on) OnChangeGraphicsLevel(2); });
        m_High.onValueChanged.AddListener((on) => { if (on) OnChangeGraphicsLevel(5); });
        m_BGM.onValueChanged.AddListener((v) => Core.xEvent?.Raise("Audio.Background.Volume", v));
        m_EffectSound.onValueChanged.AddListener((v) => Core.xEvent?.Raise("Audio.Effect.Volume", v));
        m_PlayerSound.onValueChanged.AddListener((v) => Core.state.playerSound = v);
        m_PlayerSensitivity.onValueChanged.AddListener(OnPlayerSensitivityChanged);
    }

}
