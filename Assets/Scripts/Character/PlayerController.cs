using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.Events;

public class PlayerController : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public enum State { IDLE, RUN, JUMPING, ROLLING, VICTORY, DIE }
    private enum cameraType { FP, TP }

    public bool isReloading = false;
    public bool isAttacking = false;
    public bool isTestCharacter = false;

    [SerializeField] cameraType m_CameraType;
    [SerializeField] State m_State;
    [SerializeField] CharacterOption m_COption;
    [SerializeField] Rigidbody m_RBody;
    [SerializeField] CameraOption m_FPCamera;
    [SerializeField] CameraOption m_TPCamera;
    [SerializeField] AnimationParameters m_AniParam;
    [SerializeField] Animator m_Animator;
    [SerializeField] Transform m_ShootPoint;
    [SerializeField] BulletBase m_Bullet;
    [SerializeField] LayerMask m_AimCollisionMask;
    [SerializeField] ParticleSystem m_CollisionEffect;
    [SerializeField] ParticleSystem m_GunfireEffect;
    [SerializeField] bool m_IsGround = false;
    [SerializeField] float m_GroundCheckRadius;
    [SerializeField] int m_BulletCreateCount = 10;
    [SerializeField] float m_CurHealth;

    PlayerActionsScript m_PlayerActions;
    Vector3 m_NormalizedMove;
    Vector3 m_NoramlizedRotate;
    Vector3 m_TouchStartPos;
    Vector2 m_MoveParam;
    Vector3 m_ScreenCenterPosition;

    float m_DeltaTime = 0f;
    float m_JumpCoolTime = 0;
    bool m_TakeDamangeByResidualFire = false;

    public CinemachineVirtualCamera GetCamera() => m_CameraType == cameraType.TP ? m_TPCamera.camera : m_FPCamera.camera;

    public void Jump(UnityAction done)
    {
        if (!photonView.IsMine) { return; }
        if (m_JumpCoolTime > 0 || !m_IsGround)
        {
            done?.Invoke();
            return;
        }

        m_State = State.JUMPING;
        m_JumpCoolTime = m_COption.jumpCoolTime;
        m_Animator.SetTrigger(m_AniParam.jump);
        m_RBody.AddForce(Vector3.up * m_COption.jumpForce, ForceMode.VelocityChange);
        StartCoroutine(JumpCoolDownTime(done));
    }

    public void Attack(UnityAction done)
    {
        if (!photonView.IsMine) { return; }
        if (isReloading || m_State == State.ROLLING)
        {
            done?.Invoke();
            return;
        }

        isAttacking = true;
        m_Animator.SetTrigger(m_AniParam.attack);

        GameObject go = Core.poolManager.Spawn(nameof(Bullet));
        if (go == null) { return; }
        if (go.TryGetComponent<BulletBase>(out var bullet))
        {
            go.SetActive(true);
            bullet.transform.SetPositionAndRotation(m_ShootPoint.position, m_ShootPoint.rotation);
            Vector3 dir = (m_ScreenCenterPosition - m_ShootPoint.position).normalized;
            bullet.Init(dir, transform);
            bullet.Shoot();
        }

        StartCoroutine(SkillCoolDownTime(m_COption.shootingCoolTime, () =>
        {
            isAttacking = false;
            done?.Invoke();
        }));
    }

    public void Reload(UnityAction done)
    {
        if (!photonView.IsMine) { return; }
        if (isAttacking)
        {
            done?.Invoke();
            return;
        }

        m_Animator.SetTrigger(m_AniParam.reload);
        isReloading = true;
        StartCoroutine(AnimationProgressing(m_AniParam.reload, 1, () =>
        {
            isReloading = false;
            done?.Invoke();
        }));
    }

    public void Roll(UnityAction done)
    {
        if (!photonView.IsMine) { return; }
        if (m_NormalizedMove == Vector3.zero)
        {
            done?.Invoke();
            return;
        }

        float x = m_NormalizedMove.x;
        float y = m_NormalizedMove.y;
        string param = null;
        float range = 0.71f;

        m_State = State.ROLLING;

        if ((x >= -1 && x < 0) && (-range <= y && y <= range))
        {
            m_Animator.SetTrigger(m_AniParam.rolling.left);
            param = m_AniParam.rolling.left;
        }
        else if ((x > 0 && x <= 1) && (-range <= y && y <= range))
        {
            m_Animator.SetTrigger(m_AniParam.rolling.right);
            param = m_AniParam.rolling.right;
        }
        else if ((x >= -range && x <= range) && (0 < y && y <= 1))
        {
            m_Animator.SetTrigger(m_AniParam.rolling.forward);
            param = m_AniParam.rolling.forward;
        }
        else if ((x >= -range && x <= range) && (-1 <= y && y < 0))
        {
            m_Animator.SetTrigger(m_AniParam.rolling.back);
            param = m_AniParam.rolling.back;
        }

        Vector3 dir = (m_NormalizedMove.x * transform.right + m_NormalizedMove.y * transform.forward) * m_COption.rollingForce * m_COption.movingSpeed;
        m_RBody.velocity = new Vector3(dir.x, m_RBody.velocity.y, dir.z);

        StartCoroutine(AnimationProgressing(param, 0, () =>
        {
            m_State = State.IDLE;
            done?.Invoke();
        }));
    }

    public void Die()
    {
        m_State = State.DIE;
        m_Animator.SetTrigger(m_AniParam.die);
    }

    public void Victory()
    {
        m_State = State.VICTORY;
        m_Animator.SetTrigger(m_AniParam.victory);
    }

    public void TakeDamagedByResidualFire(float duration, float damage, float interval)
    {
        if (m_TakeDamangeByResidualFire) { return; }

        StartCoroutine(TakingDamageByResidualFire(duration, damage, interval));
    }

    public void TakeDamange(float amount)
    {
        m_CurHealth -= amount;
        Core.xEvent?.Raise(XTheme.Health, m_CurHealth);

        if (m_CurHealth <= 0)
        {
            Die();
        }
    }

    void Rotation()
    {

#if UNITY_EDITOR
        if (Input.GetMouseButton(1))
        {
            m_NoramlizedRotate = new Vector2(Input.GetAxisRaw("Mouse X"), -Input.GetAxisRaw("Mouse Y"));
        }
#endif

#if !UNITY_EDITOR && UNITY_IOS
    	if (m_ScreenHalf > m_TouchStartPos.x) { return; } //오른쪽 절반만 사용 가능 
#endif
        Vector2 newValue = m_NoramlizedRotate;
        Vector3 camRot = m_TPCamera.camera.transform.localEulerAngles;

        float axisX = camRot.x + newValue.y * m_TPCamera.rotationSpeed;
        float axisY = transform.localEulerAngles.y + newValue.x * m_TPCamera.rotationSpeed;
        bool isRotable = axisX > 360 + m_TPCamera.lookUpDegree || axisX < m_TPCamera.lookDownDegree;

        m_TPCamera.camera.transform.localEulerAngles = Vector3.right * (isRotable ? axisX : camRot.x);
        transform.localEulerAngles = Vector3.up * axisY;

    }

    void Move()
    {
        if (m_State == State.ROLLING || !m_IsGround) { return; }

        m_State = m_NormalizedMove == Vector3.zero ? State.IDLE : State.RUN;
        Vector3 dir = (m_NormalizedMove.x * transform.right + m_NormalizedMove.y * transform.forward) * m_COption.movingSpeed;
        m_RBody.velocity = new Vector3(dir.x, m_RBody.velocity.y, dir.z);
    }

    void CheckDistFromGround()
    {
        Vector3 origin = transform.position + Vector3.up;
        Vector3 dir = Vector3.down;

        Ray ray = new Ray(origin, dir);
        float rayDist = 100f;
        float thrshold = 0.01f;

        bool cast = Physics.SphereCast(ray, m_GroundCheckRadius, out var hit, rayDist, m_COption.jumpOption.groundLayerMask);
        float distFromGround = cast ? (hit.distance - 1f + m_GroundCheckRadius) : float.MaxValue;
        m_IsGround = distFromGround <= m_GroundCheckRadius + thrshold;
        Core.xEvent.Raise(XTheme.InteractableJump, m_IsGround);
    }

    void UpdateAniParams()
    {
        Vector2 move = Vector2.zero;
        float lerpSpeed = 0.05f;

        if (m_State == State.RUN)
        {
            move.x = m_NormalizedMove.x;
            move.y = m_NormalizedMove.y;
        }

        m_MoveParam = Vector2.Lerp(m_MoveParam, move, lerpSpeed);
        if (!m_IsGround || m_NormalizedMove == Vector3.zero)
        {
            m_MoveParam = Vector2.zero;
        }

        m_Animator.SetFloat(m_AniParam.horizontal, m_MoveParam.x);
        m_Animator.SetFloat(m_AniParam.vertical, m_MoveParam.y);
        m_Animator.SetBool(m_AniParam.isGrounded, m_IsGround);
    }

    IEnumerator AnimationProgressing(string param, int layer, UnityAction done)
    {
        float elapsed = 0;
        float duration = 10f;
        while (elapsed < duration) //10초가 지나면 자동종료..
        {
            AnimatorStateInfo info = m_Animator.GetCurrentAnimatorStateInfo(layer);
            if (info.IsName(param) && info.normalizedTime >= 0.9f)
            {
                done?.Invoke();
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator JumpCoolDownTime(UnityAction done)
    {
        float elapsed = 0;
        float duration = m_COption.jumpCoolTime;
        while (!m_IsGround || elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        done?.Invoke();
    }

    IEnumerator SkillCoolDownTime(float duration, UnityAction done = null)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        done?.Invoke();
    }

    IEnumerator TakingDamageByResidualFire(float duration, float damage, float interval)
    {
        m_TakeDamangeByResidualFire = true;
        float elapsed = 0;
        WaitForSeconds waitForSeconds = new WaitForSeconds(interval);
        while (elapsed < duration)
        {
            elapsed += interval;
            TakeDamange(damage);
            yield return waitForSeconds;
        }

        m_TakeDamangeByResidualFire = false;
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
        if (TryGetComponent<Rigidbody>(out var body))
        {
            m_RBody = body;
        }
        if (TryGetComponent<CapsuleCollider>(out var col))
        {
            m_GroundCheckRadius = col.radius;
        }

        m_Animator.SetFloat(m_AniParam.reloadingSpeed, m_COption.reloadTime);
        m_PlayerActions.Player.Move.performed += ctx => m_NormalizedMove = ctx.ReadValue<Vector2>();
        m_PlayerActions.Player.Move.canceled += ctx => m_NormalizedMove = Vector2.zero;
        m_PlayerActions.Player.Look.performed += ctx => m_NoramlizedRotate = new Vector2(ctx.ReadValue<Vector2>().x, -ctx.ReadValue<Vector2>().y);
        m_PlayerActions.Player.Look.canceled += ctx => m_NoramlizedRotate = Vector2.zero;
        m_PlayerActions.Player.Touch.performed += ctx => m_TouchStartPos = ctx.ReadValue<Vector2>();
        m_PlayerActions.Player.Touch.canceled += ctx => m_TouchStartPos = Vector2.zero;

#if UNITY_EDITOR
        m_PlayerActions.Player.Jump.started += ctx => Jump(null);
        //  m_PlayerActions.Player.Attack.started += ctx => Attack(null);
        m_PlayerActions.Player.Reload.started += ctx => Reload(null);
        m_PlayerActions.Player.Rolling.started += ctx => Roll(null);
#endif
    }

    void Start()
    {
        if (!Core.poolManager.Has(nameof(Bullet)))
        {
            ObjectPool bulletPool = new ObjectPool(m_BulletCreateCount, m_BulletCreateCount * 2, transform.parent, m_Bullet.gameObject);
            Core.poolManager.Add(nameof(Bullet), bulletPool);
            Core.poolManager.Initialize(nameof(Bullet));

            ObjectPool effectPool = new ObjectPool(m_BulletCreateCount, m_BulletCreateCount * 2, transform.parent, m_CollisionEffect.gameObject);
            Core.poolManager.Add(nameof(BulletCollisionEffect), effectPool);
            Core.poolManager.Initialize(nameof(BulletCollisionEffect));
        }

        m_CurHealth = m_COption.health;
    }

    void Update()
    {
        if (isTestCharacter) { return; }
        if (!photonView.IsMine) { return; }
        if (m_State == State.VICTORY || m_State == State.DIE) { return; }

        m_DeltaTime = Time.deltaTime;
        CheckDistFromGround();
        Move();
        Rotation();
        UpdateAniParams();
        GetScreenCenterPosition();

        if (m_JumpCoolTime > 0)
        {
            m_JumpCoolTime -= m_DeltaTime;
        }

    }

    void GetScreenCenterPosition()
    {
        Vector2 screenPoint = new Vector2((Screen.width / 2), (Screen.height / 2));
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        m_ScreenCenterPosition = Vector3.zero;
        if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, m_AimCollisionMask))
        {
            //    Debug.DrawRay(ray.origin, ray.direction * Mathf.Infinity, Color.red);
            m_ScreenCenterPosition = raycastHit.point;
        }
        else
        {
            m_ScreenCenterPosition = ray.origin + ray.direction * 100;
        }

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
}
