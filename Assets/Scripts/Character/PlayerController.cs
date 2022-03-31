using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerController : MonoBehaviourPunCallbacks, IOnEventCallback
{
	public enum State { IDLE, RUN, JUMPING, ROLLING, ATTACKING, VICTORY, DIE, RELOADING }
	private enum cameraType { FP, TP }

	[SerializeField] cameraType m_CameraType;
	[SerializeField] State m_State;
	[SerializeField] CharacterOption m_COption;
	[SerializeField] CameraOption m_FPCamera;
	[SerializeField] CameraOption m_TPCamera;
	[SerializeField] AnimationParameters m_AniParam;
	[SerializeField] CharacterController m_Controller;
	[SerializeField] Animator m_Animator;
	
	PlayerActionsScript m_PlayerActions;
	Vector3 m_MoveDir;

	void Rotation()
	{
	
	}

	void Move()
	{
		Vector2 move = m_PlayerActions.Player.Move.ReadValue<Vector2>();
		Vector3 dir = (transform.forward * move.y + transform.right * move.x) * Time.deltaTime * m_COption.movingSpeed;
		m_Controller.Move(dir);

		m_Animator.SetFloat(m_AniParam.horizontal, move.x);
		m_Animator.SetFloat(m_AniParam.vertical, move.y);
	}

	void Update()
	{
		Move();
		Rotation();
	}

	public override void OnEnable()
	{
		m_PlayerActions.Player.Enable();
		PhotonNetwork.AddCallbackTarget(this);
	}

	public override void OnDisable()
	{
		m_PlayerActions.Player.Disable();
		PhotonNetwork.RemoveCallbackTarget(this);
	}
	
	void Awake()
	{
		m_PlayerActions = new PlayerActionsScript();
	}

	public void OnEvent(EventData photonEvent)
	{
		if (!photonView.IsMine) { return; } //Dev
		if (photonEvent.Code == (byte)PhotonEventCode.ROUNDDONE_CHARACTER_DIE)
		{
			object[] data = (object[])photonEvent.CustomData;
			int status = (int)data[0];
			bool isMaster = (bool)data[1];
			Core.gameManager.SetState((GamePlayManager.Status)status);

			if (isMaster)
			{
				Core.state.playerWinCount += 1;
			}
			else
			{
				Core.state.masterWinCount += 1;
			}
		}
	}

	/*
	[SerializeField] CinemachineVirtualCamera m_Camera;
	[SerializeField] CharacterController m_Character;
	[SerializeField] Animator m_Animator;
	[SerializeField] float m_JumpForce;
	[SerializeField] float m_MoveSpeed;
	[SerializeField] float m_MaxDistance = 0.5f;
	[SerializeField] float m_AnimationPlayTransition = 0.15f;
	[SerializeField] float m_RollingMoveDist;

	float m_Gravitiy = 9.8f;
	PlayerActionsScript m_PlayerActions;
	string m_RollingName;
	Vector3 m_JumpDirection = Vector3.zero;
	Vector2 m_RollingDirection = Vector2.zero;

	bool m_IsAttacking = false;
	int jumpAnimation; //Jump 애니메이션 이름이 포함된다

	public State GetState() => m_State;
	public void SetState(State state) => m_State = state;

	public CinemachineVirtualCamera GetCamera() => m_Camera;

	public void DoAttack()
	{
		if (m_State == State.RELOADING || m_State == State.JUMPING || m_State == State.ROLLING) { return; }
		m_IsAttacking = !m_IsAttacking;
		m_State = State.ATTACKING;
	}

	public void DoReload()
	{
		Debug.Log(m_State);
		m_State = State.RELOADING;
		m_Animator.SetTrigger("Reload");
	}

	public void DoRolling()
	{
		if (m_State != State.RUN) { return; }
		m_State = State.ROLLING;
		Vector2 value = m_PlayerActions.Player.Move.ReadValue<Vector2>();
		m_RollingDirection = value;

		switch (value)
		{
			case Vector2 v when v.Equals(Vector2.up):
				m_Animator.SetTrigger("RollingFWD");
				m_RollingName = "RollingFWD";
				break;
			case Vector2 v when v.Equals(Vector2.down):
				m_Animator.SetTrigger("RollingBWD");
				m_RollingName = "RollingBWD";
				break;
			case Vector2 v when v.Equals(Vector2.left):
				m_Animator.SetTrigger("RollingLeft");
				m_RollingName = "RollingLeft";
				break;
			case Vector2 v when v.Equals(Vector2.right):
				m_Animator.SetTrigger("RollingRight");
				m_RollingName = "RollingRight";
				break;
		}
	}

	public void DoJump()
	{
		if (m_State == State.ROLLING || m_State == State.RELOADING || m_State == State.ATTACKING) { return; }

		if (m_Character.isGrounded || IsGrounded())
		{
			m_State = State.JUMPING;
			m_Animator.SetFloat("Horizontal", 0);
			m_Animator.SetFloat("Vertical", 0);
			m_Animator.CrossFade(jumpAnimation, m_AnimationPlayTransition);
			Vector2 value = m_PlayerActions.Player.Move.ReadValue<Vector2>();
			m_JumpDirection = new Vector3(value.x, m_JumpForce, value.y);
			m_Character.Move(m_JumpDirection * m_MoveSpeed * Time.deltaTime);
		}
	}

	bool IsGrounded()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position, Vector3.down, out hit, m_MaxDistance))
			return true;
		else
			return false;
	}

	void Idle()
	{
		Vector2 value = m_PlayerActions.Player.Move.ReadValue<Vector2>();
		m_Animator.SetFloat("Horizontal", 0);
		m_Animator.SetFloat("Vertical", 0);
		if (value != Vector2.zero)
		{
			m_State = State.RUN;
		}
	}

	void DoRun()
	{
		Vector2 value = m_PlayerActions.Player.Move.ReadValue<Vector2>();
		if (value == Vector2.zero)
		{
			m_State = State.IDLE;
			return;
		}
		Move(value);
	}

	void Move(Vector2 direction)
	{
		m_Animator.SetFloat("Horizontal", direction != Vector2.zero ? direction.x : 0);
		m_Animator.SetFloat("Vertical", direction != Vector2.zero ? direction.y : 0);
		m_Character.Move((direction.x * transform.right + direction.y * transform.forward) * m_MoveSpeed * Time.deltaTime);
	}

	void Jumping()
	{
		m_JumpDirection += Vector3.down * m_Gravitiy * Time.deltaTime;
		m_Character.Move(m_JumpDirection * m_MoveSpeed * Time.deltaTime);
		if ((m_Character.isGrounded || IsGrounded()) && m_JumpDirection.y < 0)
		{
			m_State = State.IDLE;
		}
	}

	void Reloading()
	{
		Vector2 value = m_PlayerActions.Player.Move.ReadValue<Vector2>();
		AnimatorStateInfo info = m_Animator.GetCurrentAnimatorStateInfo(1);
		if (info.IsName("Reload") && info.normalizedTime >= 0.8f) //애매모한 수치...
		{
			m_State = value == Vector2.zero ? State.IDLE : State.RUN;
			return;
		}

		Move(value);
	}

	void Attcking()
	{
		m_Animator.SetBool("Attack", m_IsAttacking);
		Vector2 value = m_PlayerActions.Player.Move.ReadValue<Vector2>();
		Move(value);

		m_State = !m_IsAttacking && value == Vector2.zero ? State.IDLE : State.RUN;
	}

	void Rolling()
	{
		AnimatorStateInfo info = m_Animator.GetCurrentAnimatorStateInfo(0);
		if (info.IsName(m_RollingName) && info.normalizedTime >= 0.8f) //애매모한 수치...
		{
			Vector2 value = m_PlayerActions.Player.Move.ReadValue<Vector2>();
			m_State = value == Vector2.zero ? State.IDLE : State.RUN;
			return;
		}

		m_Animator.SetFloat("Horizontal", 0);
		m_Animator.SetFloat("Vertical", 0);
		m_Character.Move((m_RollingDirection.x * transform.right + m_RollingDirection.y * transform.forward) * m_RollingMoveDist * m_MoveSpeed * Time.deltaTime);
	}

	public void Victory()
	{
		m_Animator.SetTrigger("Victory");
	}

	public void DoDie()
	{
		m_Animator.SetTrigger("Die");

		object[] content = { GamePlayManager.Status.RoundDone, PhotonNetwork.IsMasterClient };
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
		PhotonNetwork.RaiseEvent((byte)PhotonEventCode.ROUNDDONE_CHARACTER_DIE, content, raiseEventOptions, SendOptions.SendReliable);

	}

	public void OnEvent(EventData photonEvent)
	{
		if (!photonView.IsMine) { return; } //Dev
		if (photonEvent.Code == (byte)PhotonEventCode.ROUNDDONE_CHARACTER_DIE)
		{
			object[] data = (object[])photonEvent.CustomData;
			int status = (int)data[0];
			bool isMaster = (bool)data[1];
			Core.gameManager.SetState((GamePlayManager.Status)status);

			if (isMaster)
			{
				Core.state.playerWinCount += 1;
			}
			else
			{
				Core.state.masterWinCount += 1;
			}
		}
	}


	private void Update()
	{
		if (!photonView.IsMine) { return; }

		switch (m_State)
		{
			case State.IDLE:
				Idle();
				break;
			case State.RUN:
				DoRun();
				break;
			case State.ATTACKING:
				Attcking();
				break;
			case State.DIE:
				DoDie();
				break;
			case State.JUMPING:
				Jumping();
				break;
			case State.ROLLING:
				Rolling();
				break;
			case State.VICTORY:
				Victory();
				break;
			case State.RELOADING:
				Reloading();
				break;
		}
	}

	/// <summary>
	/// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
	/// </summary>
	void LateUpdate()
	{

	}

	void Awake()
	{
		m_PlayerActions = new PlayerActionsScript();
	}

	private void Start()
	{
		if (!transform.TryGetComponent<PhotonView>(out var view))
		{
			gameObject.AddComponent<PhotonView>();
		}
	}

	public override void OnEnable()
	{
		
		m_PlayerActions.Player.Jump.started += _ => DoJump();
		m_PlayerActions.Player.Attack.started += _ => DoAttack();
		m_PlayerActions.Player.Reload.started += _ => DoReload();
		m_PlayerActions.Player.Rolling.started += _ => DoRolling();
		
		m_PlayerActions.Player.Enable();
		jumpAnimation = Animator.StringToHash("Jump");
		PhotonNetwork.AddCallbackTarget(this);
	}

	public override void OnDisable()
	{
		
		m_PlayerActions.Player.Jump.started -= _ => DoJump();
		m_PlayerActions.Player.Attack.started -= _ => DoAttack();
		m_PlayerActions.Player.Reload.started -= _ => DoReload();
		m_PlayerActions.Player.Rolling.started -= _ => DoRolling();
		
		m_PlayerActions.Player.Disable();
		PhotonNetwork.RemoveCallbackTarget(this);
	}

	[ContextMenu("Die")]
	public void TestDie()
	{
		DoDie();
	}
*/
}
