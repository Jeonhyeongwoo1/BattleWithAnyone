using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class Signup : MonoBehaviour
{
    public bool isAppleSignup = false;

    [SerializeField] InputField m_Id;
    [SerializeField] InputField m_Password;
    [SerializeField] InputField m_UserName;
    [SerializeField] InputField m_Email;
    [SerializeField] InputField m_FirstTelephone;
    [SerializeField] InputField m_MiddleTelephone;
    [SerializeField] InputField m_LastTelephone;
    [SerializeField] Button m_Signup;
    [SerializeField] Button m_CheckUserId;
    [SerializeField] Button m_Close;

    [Header("[오류 텍스트]")]
    [SerializeField] Text m_CheckId;
    [SerializeField] Text m_CheckPassword;
    [SerializeField] Text m_CheckEmail;
    [SerializeField] Text m_CheckUserName;
    [SerializeField] Text m_CheckTelephone;

    bool m_IsCheckedUserId = false;

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void CheckUserId()
    {
        string id = m_Id.text;

        if (string.IsNullOrEmpty(id))
        {
            NoticePopup.content = MessageCommon.Get("login.inputid");
            Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
            return;
        }

        Core.networkManager.ReqCheckUserId(id, CheckUserIdSuccessed, CheckUserIdFailed);
    }

    public void OnSignup()
    {
        if (!m_IsCheckedUserId)
        {
            NoticePopup.content = MessageCommon.Get("login.checkid");
            Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
            return;
        }

        m_CheckId.gameObject.SetActive(false);
        m_CheckEmail.gameObject.SetActive(false);
        m_CheckPassword.gameObject.SetActive(false);
        m_CheckTelephone.gameObject.SetActive(false);
        m_CheckUserName.gameObject.SetActive(false);

        string id = m_Id.text;
        string password = m_Password.text;
        string name = m_UserName.text;
        string email = m_Email.text;
        string first = m_FirstTelephone.text;
        string middle = m_MiddleTelephone.text;
        string last = m_LastTelephone.text;

        if (string.IsNullOrEmpty(id))
        {
            m_CheckId.gameObject.SetActive(true);
            m_Id.ActivateInputField();
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            m_CheckPassword.gameObject.SetActive(true);
            m_Password.ActivateInputField();
            return;
        }

        if (string.IsNullOrEmpty(name))
        {
            m_CheckUserName.gameObject.SetActive(true);
            m_UserName.ActivateInputField();
            return;
        }

        if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(middle) || string.IsNullOrEmpty(last))
        {
            m_CheckTelephone.gameObject.SetActive(true);
            if (string.IsNullOrEmpty(first)) { m_FirstTelephone.ActivateInputField(); }
            if (string.IsNullOrEmpty(middle)) { m_MiddleTelephone.ActivateInputField(); }
            if (string.IsNullOrEmpty(last)) { m_LastTelephone.ActivateInputField(); }
            return;
        }

        if (string.IsNullOrEmpty(email))
        {
            m_CheckEmail.gameObject.SetActive(true);
            m_Email.ActivateInputField();
            return;
        }

        string phoneNumber = first + "-" + middle + "-" + last;
        Regex number = new Regex(@"^01[01678]-[0-9]{4}-[0-9]{4}$");
        if (!number.IsMatch(phoneNumber))
        {
            m_CheckTelephone.gameObject.SetActive(true);
            return;
        }

        Regex e = new Regex(@"^([0-9a-zA-Z]+)@([0-9a-zA-Z]+)(\.[0-9a-zA-Z]+){1,}$");
        if (!e.IsMatch(email))
        {
            m_CheckEmail.gameObject.SetActive(true);
            m_Email.ActivateInputField();
            return;
        }

        if (isAppleSignup)
        {
            Core.networkManager.ReqAppleSignup(id, password, email, phoneNumber, name, SignupSuccessed, SignupFailed);
            isAppleSignup = false;
        }
        else
        {
            Core.networkManager.ReqSignup(id, password, email, phoneNumber, name, SignupSuccessed, SignupFailed);
        }

    }

    void CheckUserIdSuccessed(string data)
    {
        if (!string.IsNullOrEmpty(data))
        {
            m_IsCheckedUserId = true;
            StartCoroutine(ShowAvailableIdPhrases(MessageCommon.Get("login.availableid")));
        }
        else
        {
            m_IsCheckedUserId = false;
            StartCoroutine(ShowAvailableIdPhrases(MessageCommon.Get("login.notavailableid")));
        }
    }

    IEnumerator ShowAvailableIdPhrases(string text)
    {
        m_CheckId.text = text;
        m_CheckId.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        m_CheckId.gameObject.SetActive(false);
        m_CheckId.text = MessageCommon.Get("login.checkid");
    }

    void CheckUserIdFailed(string error)
    {
        //error
        m_IsCheckedUserId = false;
    }

    void SignupSuccessed(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            SignupFailed(null);
            return;
        }

        ConfirmPopup.content = MessageCommon.Get("signup.signupsucceesd");
        Core.plugs.Get<Popups>().OpenPopupAsync<ConfirmPopup>();

        gameObject.SetActive(false);
    }

    void SignupFailed(string error)
    {
        Init();
		Core.networkManager.appleAuth = null;
		Core.networkManager.appleAuthManager = null;
        NoticePopup.content = MessageCommon.Get("signup.signupfailed");
        Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
    }

    void Init()
    {
        m_CheckId.gameObject.SetActive(false);
        m_CheckEmail.gameObject.SetActive(false);
        m_CheckPassword.gameObject.SetActive(false);
        m_CheckTelephone.gameObject.SetActive(false);
        m_CheckUserName.gameObject.SetActive(false);

        m_Id.text = null;
        m_Password.text = null;
        m_UserName.text = null;
        m_Email.text = null;
        m_FirstTelephone.text = null;
        m_MiddleTelephone.text = null;
        m_LastTelephone.text = null;

    }

    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        Init();
    }

    private void Awake()
    {
        m_Close.onClick.AddListener(Close);
        m_Signup.onClick.AddListener(OnSignup);
        m_CheckUserId.onClick.AddListener(CheckUserId);
    }

}
