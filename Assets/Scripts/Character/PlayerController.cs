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

    public int bulletCount
    {
        get => m_COption.bulletCount;
    }

    public BulletAttribute.BulletType gunType
    {
        get => m_Bullet.bulletType;
    }

    public float speed
    {
        get => m_COption.movingSpeed;
        private set => m_COption.movingSpeed = value;
    }

    public int damage
    {
        get => m_Bullet.damage;
    }

    public bool roomCharacter = false;
    public bool isReloading
    {
        get => m_IsReloading;
        private set => m_IsReloading = value;
    }

    public bool isAttacking
    {
        get => m_IsAttacking;
        private set => m_IsAttacking = value;
    }

    public float rotationSpeed
    {
        get => m_CameraType == cameraType.TP ? m_TPCamera.rotationSpeed : m_FPCamera.rotationSpeed;
        set
        {
            if (m_CameraType == cameraType.TP)
            {
                m_TPCamera.rotationSpeed = value;
            }
            else
            {
                m_FPCamera.rotationSpeed = value;
            }

        }
    }

    [SerializeField] cameraType m_CameraType;
    [SerializeField] State m_State;
    [SerializeField] CharacterOption m_COption;
    [SerializeField] Rigidbody m_RBody;
    [SerializeField] CameraOption m_FPCamera;
    [SerializeField] CameraOption m_TPCamera;
    [SerializeField] AnimationParameters m_AniParam;
    [SerializeField] Animator m_Animator;
    [SerializeField] Transform m_ShootPoint;
    [SerializeField] LayerMask m_AimCollisionMask;
    [SerializeField] ParticleSystem m_GunfireEffect;
    [SerializeField] bool m_IsGround = false;
    [SerializeField] float m_GroundCheckRadius;
    [SerializeField] int m_BulletCreateCount = 10;
    [SerializeField] float m_FootStepInterval = 2f;
    [SerializeField] BulletBase m_Bullet;
    [SerializeField] BulletCollisionEffect m_CollisionEffect;
    [SerializeField] AudioSource m_PlayerAudio;
    [SerializeField] AudioSource m_WeaponAudio;
    [SerializeField] AudioClip m_WeaponAttackClip;
    [SerializeField] AudioClip m_ReloadClip;
    [SerializeField] AudioClip m_DieClip;

    PlayerActionScripts m_PlayerActions;
    Vector3 m_NormalizedMove;
    Vector3 m_NoramlizedRotate;
    Vector2 m_MoveParam;
    RaycastHit m_RaycastHit = new RaycastHit();

    float m_ScreenHalf = 0;
    float m_DeltaTime = 0f;
    float m_JumpCoolTime = 0;
    float m_FootStepCycle = 0;
    bool m_TakeDamangeByResidualFire = false;
    bool m_IsReloading = false;
    bool m_IsAttacking = false;

    public void UpdateHealth(int amount)
    {
        int health = Core.state.health;

        health += amount;
        if (health > 100)
        {
            health = 100;
        }

        Core.state.health = health;
    }

    public void IncreaseSpeedByItem(int amount, float duration)
    {
        float origin = speed;
        speed = amount;
        m_FootStepInterval *= 2;
        StartCoroutine(ApplyingItemEffect(duration, () => { speed = origin; m_FootStepInterval *= 0.5f; }));
    }

    public CinemachineVirtualCamera GetCamera() => m_CameraType == cameraType.TP ? m_TPCamera.camera : m_FPCamera.camera;

    public void CreateBullet()
    {
        if (!Core.poolManager.Has(nameof(Bullet)))
        {
            if (m_Bullet)
            {
                ObjectPool bulletPool = new ObjectPool(m_BulletCreateCount, m_BulletCreateCount * 2, transform.parent, m_Bullet.gameObject);
                Core.poolManager.Add(nameof(Bullet), bulletPool);
                Core.poolManager.Initialize(nameof(Bullet), XSettings.bulletPath + m_Bullet.name);
            }
        }
    }

    public void CreateCollisionEffect()
    {
        if (!Core.poolManager.Has(nameof(BulletCollisionEffect)))
        {
            if (m_CollisionEffect)
            {
                ObjectPool effectPool = new ObjectPool(m_BulletCreateCount, m_BulletCreateCount * 2, transform.parent, m_CollisionEffect.gameObject);
                Core.poolManager.Add(nameof(BulletCollisionEffect), effectPool);
                Core.poolManager.Initialize(nameof(BulletCollisionEffect), XSettings.bulletImpactPath + m_CollisionEffect.name);
            }
        }
    }

    public void Jump(UnityAction done)
    {
        if (!photonView.IsMine || m_JumpCoolTime > 0 || !m_IsGround || m_State == State.ROLLING || m_State == State.DIE || m_State == State.VICTORY)
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
        if (!photonView.IsMine || Core.state.bulletCount <= 0 || isReloading || isAttacking || m_State == State.ROLLING || m_State == State.DIE || m_State == State.VICTORY)
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
            Vector3 screenCenter = GetScreenCenterPosition();
            Vector3 dir = (screenCenter - m_ShootPoint.position).normalized;
            bullet.Shoot(dir, m_ShootPoint.position, m_ShootPoint.rotation);
            photonView.RPC(nameof(NotifyPlayWeaponSound), RpcTarget.All, "Attack");
            Core.state.bulletCount--;
        }

        if (m_GunfireEffect != null)
        {
            if (m_GunfireEffect.isPlaying)
            {
                m_GunfireEffect.Stop();
            }

            m_GunfireEffect.Play();
        }

        StartCoroutine(SkillCoolDownTime(m_COption.shootingCoolTime, () =>
        {
            isAttacking = false;
            done?.Invoke();
        }));
    }

    public void Reload(UnityAction done)
    {
        if (!photonView.IsMine || isAttacking || isReloading || m_State == State.DIE || m_State == State.VICTORY)
        {
            done?.Invoke();
            return;
        }

        isReloading = true;
        m_Animator.SetTrigger(m_AniParam.reload);
        photonView.RPC(nameof(NotifyPlayWeaponSound), RpcTarget.All, "Reload");
        StartCoroutine(AnimationProgressing(m_AniParam.reload, 1, () =>
        {
            isReloading = false;
            Core.state.bulletCount = m_COption.bulletCount;
            done?.Invoke();
        }));
    }

    public void Roll(UnityAction done)
    {
        if (!photonView.IsMine || m_NormalizedMove == Vector3.zero || !m_IsGround || m_State == State.DIE || m_State == State.VICTORY)
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
        m_Animator.SetBool(m_AniParam.die, true);
        m_PlayerAudio.PlayOneShot(m_DieClip);
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent((byte)PhotonEventCode.ROUNDDONE_CHARACTER_DIE, null, raiseEventOptions, SendOptions.SendReliable);

        object content = PhotonNetwork.IsMasterClient;
        RaiseEventOptions raiseEvent = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)PhotonEventCode.ROUNDDONE, content, raiseEvent, SendOptions.SendReliable);
    }

    public void Victory()
    {
        m_State = State.VICTORY;
        m_Animator.SetTrigger(m_AniParam.victory);
    }

    public void TakeDamagedByResidualFire(float duration, int damage, float interval)
    {
        if (m_TakeDamangeByResidualFire) { return; }

        StartCoroutine(TakingDamageByResidualFire(duration, damage, interval));
    }

    public void TakeDamange(int amount)
    {
        if (Core.state.health <= 0) { return; }

        Core.state.health -= amount;
        if (Core.state.health <= 0)
        {
            Die();
        }

        Core.state.totalDamangeReceived += amount;
        photonView.RPC(nameof(NotifyUpdateTotalDamage), RpcTarget.Others, amount);

        if (Core.state.health < 0)
        {
            amount = Core.state.health;
            Core.state.totalDamangeReceived += amount;
            photonView.RPC(nameof(NotifyUpdateTotalDamage), RpcTarget.Others, amount);
        }
    }

    void Rotation()
    {

#if UNITY_EDITOR
        if (Input.GetMouseButton(1))
        {
            m_NoramlizedRotate = Vector3.Lerp(m_NoramlizedRotate, new Vector2(Input.GetAxisRaw("Mouse X"), -Input.GetAxisRaw("Mouse Y")), m_DeltaTime * 50);
        }

        if (Input.GetMouseButtonUp(1))
        {
            m_NoramlizedRotate = Vector3.zero;
        }
#endif

#if !UNITY_EDITOR && UNITY_IOS
    //	if (m_ScreenHalf > m_TouchStartPos.x) { return; } //오른쪽 절반만 사용 가능 
#endif
        Vector3 camRot = m_TPCamera.camera.transform.localEulerAngles;
        float axisX = camRot.x + m_NoramlizedRotate.y * m_TPCamera.rotationSpeed;
        float axisY = transform.localEulerAngles.y + m_NoramlizedRotate.x * m_TPCamera.rotationSpeed;
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

    void PlayFootStepSound()
    {
        if (m_IsGround && m_State == State.RUN)
        {
            m_FootStepCycle += m_RBody.velocity.magnitude * m_DeltaTime; // Speed
            if (m_FootStepCycle > m_FootStepInterval)
            {
                if (m_RaycastHit.collider == null) { return; }
                m_FootStepCycle = 0;
                Transform ground = m_RaycastHit.collider.transform;
                int groundType = 0;
                if (ground.TryGetComponent<ModelGroundType>(out var g)) { groundType = (int)g.groundType; }
                photonView.RPC(nameof(NotifyPlayerSound), RpcTarget.All, "FootStep", groundType);
            }
        }
    }

    void CheckDistFromGround()
    {
        Vector3 origin = transform.position + Vector3.up;
        Vector3 dir = Vector3.down;

        Ray ray = new Ray(origin, dir);
        float rayDist = 100f;
        float thrshold = 0.01f;

        bool cast = Physics.SphereCast(ray, m_GroundCheckRadius, out m_RaycastHit, rayDist, m_COption.jumpOption.groundLayerMask);
        float distFromGround = cast ? (m_RaycastHit.distance - 1f + m_GroundCheckRadius) : float.MaxValue;
        m_IsGround = distFromGround <= m_GroundCheckRadius + thrshold;
        XTheme.OnJumpInteravtable = m_IsGround;
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

    Vector3 GetScreenCenterPosition()
    {
        Vector2 screenPoint = new Vector2((Screen.width / 2), (Screen.height / 2));
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, m_AimCollisionMask))
        {
            //    Debug.DrawRay(ray.origin, ray.direction * Mathf.Infinity, Color.red);
            return raycastHit.point;
        }
        else
        {
            return ray.origin + ray.direction * 100;
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case (byte)PhotonEventCode.ROUNDDONE_CHARACTER_DIE:
                //Victory
                if (photonView.IsMine)
                {
                    Victory();
                }
                break;
            case (byte)PhotonEventCode.GAME_END:
                if (!photonView.IsMine) { break; }

                int winCount = PhotonNetwork.IsMasterClient ? Core.state.masterWinCount : Core.state.playerWinCount;
                if (winCount == Core.state.mapPreferences.numberOfRound)
                {
                    Victory();
                }
                else
                {
                    m_State = State.DIE;
                    m_Animator.SetBool(m_AniParam.die, true);
                    m_PlayerAudio.PlayOneShot(m_DieClip);
                }

                m_Animator.SetFloat(m_AniParam.horizontal, 0);
                m_Animator.SetFloat(m_AniParam.vertical, 0);
                break;
        }

    }

    IEnumerator ApplyingItemEffect(float duration, UnityAction done = null)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        done?.Invoke();
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

    IEnumerator TakingDamageByResidualFire(float duration, int damage, float interval)
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

    [PunRPC]
    void NotifyUpdateTotalDamage(int value)
    {
        Core.state.totalTakeDamange += value;
    }

    [PunRPC]
    public void NotifyResetState()
    {
        m_State = State.IDLE;
        Core.state.health = 100;
        Core.state.bulletCount = m_COption.bulletCount;
        m_NoramlizedRotate = Vector3.zero;
        m_NormalizedMove = Vector3.zero;
        m_MoveParam = Vector2.zero;
        m_Animator.SetBool(m_AniParam.die, false);
    }

    [PunRPC]
    public void SetParent(bool isMaster)
    {
        IModel model = Core.models.Get();
        Transform[] players = model.playerCreatePoints;
        int index = isMaster ? 0 : 1;
        transform.SetParent(players[index].parent);
        transform.name = index == 0 ? "Master" : "Player";
    }

    [PunRPC]
    void NotifyPlayWeaponSound(string soundType)
    {
        switch (soundType)
        {
            case "Attack":
                m_WeaponAudio.PlayOneShot(m_WeaponAttackClip);
                break;
            case "Reload":
                m_WeaponAudio.PlayOneShot(m_ReloadClip);
                break;
        }
    }

    [PunRPC]
    void NotifyPlayerSound(string soundType, int groundType)
    {
        switch (soundType)
        {
            case "FootStep":
                AudioClip footStep = Core.audioManager.GetFootStepAudio((AudioManager.GroundType)groundType);
                if (footStep != null)
                {
                    m_PlayerAudio.PlayOneShot(footStep);
                }
                break;
        }
    }

    public void OnVolumChanged(string key, object o)
    {
        float volume = (float)o;
        switch (key)
        {
            case nameof(Core.state.playerSound):
                m_PlayerAudio.volume = volume;
                m_WeaponAudio.volume = volume;
                break;
        }
    }

    public void OnSoundMute(string key, object o)
    {
        bool mute = (bool)o;
        switch (key)
        {
            case nameof(Core.state.playerSoundMute):
                m_PlayerAudio.mute = mute;
                m_WeaponAudio.mute = mute;
                break;
        }
    }

    void OnRotSensitivityChanged(string key, object o)
    {
        float value = (float)o; //0~1
        if (value <= 1f) { value = 1; }
        rotationSpeed = value;
    }

    public override void OnEnable()
    {
        if (roomCharacter) { return; }
        m_PlayerActions.Player.Enable();
        PhotonNetwork.AddCallbackTarget(this);
        Core.state?.Listen(nameof(Core.state.playerSound), OnVolumChanged);
        Core.state?.Listen(nameof(Core.state.playerSoundMute), OnSoundMute);
        Core.state?.Listen(nameof(Core.state.playerRotSensitivity), OnRotSensitivityChanged);
    }

    public override void OnDisable()
    {
        if (roomCharacter) { return; }
        m_PlayerActions.Player.Disable();
        PhotonNetwork.RemoveCallbackTarget(this);
        Core.xEvent?.Stop(nameof(Core.state.playerSound), OnVolumChanged);
        Core.xEvent?.Stop(nameof(Core.state.playerSoundMute), OnSoundMute);
        Core.state?.Stop(nameof(Core.state.playerRotSensitivity), OnRotSensitivityChanged);
    }

    void Awake()
    {
        if (roomCharacter) { return; }
        m_PlayerActions = new PlayerActionScripts();
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
        m_PlayerActions.Player.Move.canceled += ctx => { m_NormalizedMove = Vector2.zero; Core.xEvent?.Raise("Move.Stop", null); };
        m_PlayerActions.Player.Look.performed += ctx => m_NoramlizedRotate = Vector3.Lerp(m_NoramlizedRotate, new Vector2(ctx.ReadValue<Vector2>().x, -ctx.ReadValue<Vector2>().y).normalized, m_DeltaTime * 50);
        m_PlayerActions.Player.Look.canceled += ctx => m_NoramlizedRotate = Vector2.zero;
        m_ScreenHalf = Screen.width / 2;
#if UNITY_EDITOR
        m_PlayerActions.Player.Jump.started += ctx => Jump(null);
        //  m_PlayerActions.Player.Attack.started += ctx => Attack(null);
        m_PlayerActions.Player.Reload.started += ctx => Reload(null);
        m_PlayerActions.Player.Rolling.started += ctx => Roll(null);
#endif
    }

    void Start()
    {
        Core.state.bulletCount = m_COption.bulletCount;
        Core.state.health = m_COption.health;
    }

    void Update()
    {
        if (roomCharacter) { return; }
        if (!photonView.IsMine) { return; }
        if (m_State == State.VICTORY || m_State == State.DIE) { return; }

        m_DeltaTime = Time.deltaTime;
        CheckDistFromGround();
        Move();
        Rotation();
        UpdateAniParams();
        GetScreenCenterPosition();
        PlayFootStepSound();

        if (m_JumpCoolTime > 0)
        {
            m_JumpCoolTime -= m_DeltaTime;
        }

    }

    [ContextMenu("Die")]
    public void TestDie()
    {
        Core.state.health = 0;
        Die();
    }
}
