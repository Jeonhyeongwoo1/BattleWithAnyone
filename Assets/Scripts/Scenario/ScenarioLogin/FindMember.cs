using UnityEngine;
using UnityEngine.UI;

public class FindMember : MonoBehaviour
{

	[SerializeField] GameObject m_FindForm;
	[SerializeField] GameObject m_IdForm;
	[SerializeField] GameObject m_PwForm;

	[Space, Header("[FindForm]")]
	[SerializeField] Button m_FindFormClose;
	[SerializeField] Button m_Id;
	[SerializeField] Button m_Password;

	[Space, Header("[FindId]")]
	[SerializeField] Button m_FindId_Close;
	[SerializeField] InputField m_FindId_Email;
	[SerializeField] InputField m_FindId_UserName;
	[SerializeField] Button m_FindId;

	[Space, Header("[FindPassword]")]
	[SerializeField] Button m_FindPw_Close;
	[SerializeField] InputField m_FindPw_Id;
	[SerializeField] InputField m_FindPw_Email;
	[SerializeField] Button m_FindPw;

	public void Open()
	{
		m_FindForm.SetActive(true);
	}

	public void Close()
	{
		m_FindForm.SetActive(false);
	}

	void FindId()
	{
		string email = m_FindId_Email.text;
		string userName = m_FindId_UserName.text;

		if (string.IsNullOrEmpty(email))
		{
			NoticePopup.content = "이메일을 입력해주세요.";
			Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
			m_FindId_Email.ActivateInputField();
			return;
		}

		if (string.IsNullOrEmpty(userName))
		{
			NoticePopup.content = "이름을 입력해주세요.";
			Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
			m_FindId_UserName.ActivateInputField();
			return;
		}

		Core.networkManager.ReqFindId(email, userName, FindIdSuccessed, FindIdFailed);
	}

	void FindPassword()
	{
		string id = m_FindPw_Id.text;
		string email = m_FindPw_Email.text;

		if (string.IsNullOrEmpty(id))
		{
			NoticePopup.content = "아이디를 입력해주세요.";
			Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
			m_FindPw_Id.ActivateInputField();
			return;
		}

		if (string.IsNullOrEmpty(email))
		{
			NoticePopup.content = "이메일을 입력해주세요.";
			Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
			m_FindPw_Email.ActivateInputField();
			return;
		}

		Core.networkManager.ReqFindPassword(id, email, FindPwSuccessed, FindPwFailed);
	}

	void FindPwSuccessed(string data)
	{
		if (string.IsNullOrEmpty(data))
		{
			NoticePopup.content = "비밀번호를 찾을 수 없습니다.";
			Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
			return;
		}

		Member member = JsonUtility.FromJson<Member>(data);
		string password = member.mbr_pwd;

		ConfirmPopup.content = "회원님의 비밀번호는 : " + password + " 입니다.";
		Core.plugs.Get<Popups>().OpenPopupAsync<ConfirmPopup>();

	}

	void FindPwFailed(string error)
	{
		Debug.LogError(error);

		NoticePopup.content = "비밀번호를 찾을 수 없습니다.";
		Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();

		m_FindPw_Email.text = null;
		m_FindPw_Id.text = null;
	}

	void FindIdSuccessed(string data)
	{
		if (string.IsNullOrEmpty(data))
		{
			NoticePopup.content = "아이디를 찾을 수 없습니다.";
			Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();
			return;
		}

		Member member = JsonUtility.FromJson<Member>(data);
		string id = member.mbr_id;

		ConfirmPopup.content = "회원님의 Id는 : " + id + " 입니다.";
		Core.plugs.Get<Popups>().OpenPopupAsync<ConfirmPopup>();
	}

	void FindIdFailed(string error)
	{
		Debug.LogError(error);

		NoticePopup.content = "아이디를 찾을 수 없습니다.";
		Core.plugs.Get<Popups>().OpenPopupAsync<NoticePopup>();

		m_FindId_Email.text = null;
		m_FindId_UserName.text = null;
	}

	void CloseFindIdForm()
	{
		m_FindId_Email.text = null;
		m_FindId_UserName.text = null;
		m_IdForm.SetActive(false);
	}

	void CloseFindPwForm()
	{
		m_FindPw_Email.text = null;
		m_FindPw_Id.text = null;
		m_PwForm.SetActive(false);
	}

	void OpenFindIdForm() => m_IdForm.SetActive(true);
	void OpenFindPasswordForm() => m_PwForm.SetActive(true);

	private void Awake()
	{
		m_FindFormClose.onClick.AddListener(Close);
		m_Id.onClick.AddListener(OpenFindIdForm);
		m_Password.onClick.AddListener(OpenFindPasswordForm);
		m_FindId_Close.onClick.AddListener(CloseFindIdForm);
		m_FindPw_Close.onClick.AddListener(CloseFindPwForm);
		m_FindPw.onClick.AddListener(FindPassword);
		m_FindId.onClick.AddListener(FindId);
	}
}
