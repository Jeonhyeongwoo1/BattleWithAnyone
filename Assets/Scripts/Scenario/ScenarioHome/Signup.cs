using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class Signup : MonoBehaviour
{
	[SerializeField] InputField m_Id;
	[SerializeField] InputField m_Password;
	[SerializeField] InputField m_UserName;
	[SerializeField] InputField m_Email;
	[SerializeField] InputField m_FirstTelephone;
	[SerializeField] InputField m_MiddleTelephone;
	[SerializeField] InputField m_LastTelephone;
	[SerializeField] Button m_Signup;
	[SerializeField] Button m_CheckUserId;

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
			//
			Debug.LogError("Input sign id");
			return;
		}

		Core.networkManager.ReqCheckUserId(id, CheckUserIdSuccessed, CheckUserIdFailed);
	}

	public void OnSignup()
	{

		if (!m_IsCheckedUserId)
		{
			Debug.Log("Check User Id");
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

		Core.networkManager.ReqSignup(id, password, email, phoneNumber, name, SignupSccuessed, SignupFailed);
	}

	void CheckUserIdSuccessed(string data)
	{

		if (!string.IsNullOrEmpty(data))
		{
			m_IsCheckedUserId = true;
			StartCoroutine(ShowAvailableIdPhrases("사용가능한 아이디 입니다."));
		}
		else
		{
			m_IsCheckedUserId = false;
			StartCoroutine(ShowAvailableIdPhrases("사용 불가능한 아이디 입니다."));
		}
	}

	IEnumerator ShowAvailableIdPhrases(string text)
	{
		m_CheckId.text = text;
		m_CheckId.gameObject.SetActive(true);
		yield return new WaitForSeconds(2f);
		m_CheckId.gameObject.SetActive(false);
		m_CheckId.text = "아이디를 확인해 주세요.";
	}

	void CheckUserIdFailed(string error)
	{
		//error
		m_IsCheckedUserId = false;
	}

	void SignupSccuessed(string data)
	{
		Debug.Log(data);

		//Open Popup
		gameObject.SetActive(false);

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

	void SignupFailed(string error)
	{
		//Popup open

		Init();


	}

	void Start()
	{
		Init();
	}

	private void Awake()
	{
		m_Signup.onClick.AddListener(OnSignup);
		m_CheckUserId.onClick.AddListener(CheckUserId);
	}

}
