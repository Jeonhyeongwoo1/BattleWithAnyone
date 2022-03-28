using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public enum State
    {
        IDLE,
        RUN,
        JUMPING,
        ROLLING,
        ATTACKING,
        VICTORY,
        DIE,
        RELOADING
    }

    [SerializeField] State m_State;
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

    void DoAttack(InputAction.CallbackContext obj)
    {
        if (m_State == State.RELOADING || m_State == State.JUMPING || m_State == State.ROLLING) { return; }
        m_IsAttacking = !m_IsAttacking;
        m_State = State.ATTACKING;
    }

    void DoReload(InputAction.CallbackContext obj)
    {
        m_State = State.RELOADING;
        m_Animator.SetTrigger("Reload");
    }

    void DoRolling(InputAction.CallbackContext obj)
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

    void DoJump(InputAction.CallbackContext obj)
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

    public void Victory()
    {
        m_Animator.SetTrigger("Victory");
    }

    public void DoDie()
    {
        m_Animator.SetTrigger("Die");
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
        if (Input.GetMouseButton(1))
        {
            
        }
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

    void OnEnable()
    {
        m_PlayerActions.Player.Jump.started += DoJump;
        m_PlayerActions.Player.Attack.started += DoAttack;
        m_PlayerActions.Player.Reload.started += DoReload;
        m_PlayerActions.Player.Rolling.started += DoRolling;
        m_PlayerActions.Player.MousePosition.started += _ => LateUpdate();
        m_PlayerActions.Player.Enable();

        jumpAnimation = Animator.StringToHash("Jump");
    }

    void OnDisable()
    {
        m_PlayerActions.Player.Jump.started -= DoJump;
        m_PlayerActions.Player.Attack.started -= DoAttack;
        m_PlayerActions.Player.Reload.started -= DoReload;
        m_PlayerActions.Player.Rolling.started -= DoRolling;
        m_PlayerActions.Player.MousePosition.started -= _ => LateUpdate();
        m_PlayerActions.Player.Disable();
    }

}
