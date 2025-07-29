using Photon.Pun;
using UnityEngine;

/// <summary>
/// �÷��̾��� ĳ������ ������ ����ϴ� Ŭ����
/// Aircraft�� ��ӹ޾� HP, �̵�, �߻�, ������ ó�� �� ���� �÷��� �ٽ� ������ ����
/// </summary>
public class Player : Aircraft
{
    public int maxPower;  // �ִ� �Ŀ� �ܰ�
    public int maxBoom;   // �ִ� ��ź ����

    public int power = 1; // ���� �Ŀ� �ܰ�
    public int boom = 0;  // ���� ��ź ����

    private SpriteRenderer spriteRenderer;

    // ȭ�� ��� üũ �÷���
    private bool isTouchTop;
    private bool isTouchBottom;
    private bool isTouchRight;
    private bool isTouchLeft;

    // ���̽�ƽ �� ��ư �Է� ���� ����
    private bool[] joyControl;
    private bool isControl;
    private bool isButtonA;
    private bool isButtonB;

    // ���� �÷���
    private bool isControllable;    // �÷��̾� ���� ���� ����
    private bool isHit;             // �ǰ� ����
    private bool isBoomTime;        // ��ź ��� �� ����
    private bool isRespawnTime;     // ������ ���� �ð� ����
    private bool isDead = false;    // ��� ����

    public bool IsDead => isDead;

    protected override void Awake()
    {
        base.Awake();

        spriteRenderer = GetComponent<SpriteRenderer>();

        joyControl = new bool[9];

        InitTransform();

        GameManager.Instance.RegistPlayer(this);

        if (photonView.IsMine) 
            InputBridge.Instance.RegisterPlayer(this);
    }

    private void OnDestroy()
    {
        if (photonView.IsMine)
            InputBridge.Instance.UnregisterPlayer(this);
    }

    void Update()
    {
        if (!photonView.IsMine || !isControllable)
            return;

        Move();
        Shot();
        Boom();
        Reload();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine || !isControllable) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Border"))  // ȭ�� ���� ����� ��
        {
            TouchBorder(collision.gameObject.name, true);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy")   // ���ͳ� ���� �Ѿ˿� ����� ��
            || collision.gameObject.layer == LayerMask.NameToLayer("EnemyBullet"))
        {
            if (isRespawnTime || isHit)
                return;

            isHit = true;
            OnHit(1);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Item"))   // �����ۿ� ����� ��
        {
            PhotonView itemView = collision.gameObject.GetComponent<PhotonView>();

            // ������ ���� ��û�� RPC�� ��� Ŭ���̾�Ʈ�� ����
            if (PhotonNetwork.IsMasterClient)
                photonView.RPC(nameof(RPC_GetItem), RpcTarget.All, itemView.ViewID);
            else
                photonView.RPC(nameof(RequestPickUpItem), RpcTarget.MasterClient, itemView.ViewID);
        } 
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Border")   // ȭ�� ������ �������� ��
            TouchBorder(collision.gameObject.name, false);
    }


    #region Move
    /// <summary>
    /// �̵�ó��
    /// ��� üũ �� �ӵ��� ���� ��ġ ����
    /// </summary>
    private void Move()
    {
        float h = 0f;
        float v = 0f;

#if UNITY_STANDALONE || UNITY_EDITOR
        // Keyboard Control Value
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        if ((isTouchRight && h == 1) || (isTouchLeft && h == -1))
        {
            h = 0;
        }
        if ((isTouchTop && v == 1) || (isTouchBottom && v == -1))
        {
            v = 0;
        }
#endif

#if UNITY_ANDROID || UNITY_IOS
        // Joy Control Value
        if (joyControl[0]) { h = -1; v = 1; }
        if (joyControl[1]) { h = 0; v = 1; }
        if (joyControl[2]) { h = 1; v = 1; }
        if (joyControl[3]) { h = -1; v = 0; }
        if (joyControl[4]) { h = 0; v = 0; }
        if (joyControl[5]) { h = 1; v = 0; }
        if (joyControl[6]) { h = -1; v = -1; }
        if (joyControl[7]) { h = 0; v = -1; }
        if (joyControl[8]) { h = 1; v = -1; }

        if ((isTouchRight && h == 1) || (isTouchLeft && h == -1) || !isControl)
        {
            h = 0;
        }
        if ((isTouchTop && v == 1) || (isTouchBottom && v == -1) || !isControl)
        {
            v = 0;
        }
#endif

        Vector3 curPos = transform.position;
        Vector3 nextPos = new Vector3(h, v, 0) * status.Speed * Time.deltaTime;

        transform.position = curPos + nextPos;
    }

    /// <summary>
    /// ȭ�� ��� ��ġ ���� ����
    /// </summary>
    /// <param name="name"></param>
    /// <param name="isTouched"></param>
    private void TouchBorder(string name, bool isTouched)
    {
        switch (name)
        {
            case "Top":
                isTouchTop = isTouched;
                break;
            case "Bottom":
                isTouchBottom = isTouched;
                break;
            case "Right":
                isTouchRight = isTouched;
                break;
            case "Left":
                isTouchLeft = isTouched;
                break;
        }
    }
#endregion

    #region Joystick
    public void JoyPanel(int type)
    {
        for (int index = 0; index < joyControl.Length; index++)
            joyControl[index] = index == type;
    }

    public void JoyDown() => isControl = true;
    public void JoyUp() => isControl = false;

    public void ButtonADown() => isButtonA = true;
    public void ButtonAUp() => isButtonA = false;

    public void ButtonBDown() => isButtonB = true;
    #endregion

    #region Shot
    /// <summary>
    /// �÷��̾��� ���� �߻� ����
    /// ���� �Ŀ� �ܰ迡 ���� RPC�� �Ѿ� ����
    /// </summary>
    private void Shot()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        if (!Input.GetButton("Fire1"))
            return;
#endif

#if UNITY_ANDROID || UNITY_IOS
        if (!isButtonA)
            return;
#endif

        if (curShotDelay < maxShotDelay)
        {
            return;
        }

        switch (power)
        {
            case 1:
                photonView.RPC(nameof(RPC_OneShot), RpcTarget.All, transform.position);
                break;
            case 2:
                photonView.RPC(nameof(RPC_TwoShot), RpcTarget.All, transform.position);
                break;
            default:
                photonView.RPC(nameof(RPC_ThreeShot), RpcTarget.All, transform.position);
                break;
        }

        curShotDelay = 0;
    }

    [PunRPC]
    private void RPC_OneShot(Vector3 position)
    {
        GameObject bullet = PoolManager.Instance.MakeObj(PoolType.BulletPlayerA);
        bullet.transform.position = position;

        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();

        rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
    }

    [PunRPC]
    private void RPC_TwoShot(Vector3 position)
    {
        float[] offsets = { 0.1f, -0.1f };

        foreach (float offset in offsets)
        {
            GameObject bullet = PoolManager.Instance.MakeObj(PoolType.BulletPlayerA);
            bullet.transform.position = position + Vector3.right * offset;

            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
        }
    }

    [PunRPC]
    private void RPC_ThreeShot(Vector3 position)
    {
        (PoolType bulletType, float offset)[] bullets = {
                    (PoolType.BulletPlayerA,  0.35f),
                    (PoolType.BulletPlayerB,  0f),
                    (PoolType.BulletPlayerA, -0.35f)
                };

        foreach (var (type, offset) in bullets)
        {
            GameObject bullet = PoolManager.Instance.MakeObj(type);
            bullet.transform.position = position + Vector3.right * offset;

            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
        }
    }
    #endregion

    #region Boom
    /// <summary>
    /// ��ź ��� ó��
    /// ���� �� źȯ ���� �� ����Ʈ ǥ��
    /// </summary>
    private void Boom()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        if (!Input.GetButton("Fire2"))
            return;
#endif

#if UNITY_ANDROID || UNITY_IOS
        if (!isButtonB)
            return;
#endif
        if (isBoomTime || boom == 0)
        {
            //isButtonB = false;
            return;
        }


        boom--;
        isBoomTime = true;
        GameUIManager.Instance.UpdateBoomUI(boom);

        photonView.RPC(nameof(RPC_Boom), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_Boom()
    {
        GameUIManager.Instance.ActivateBoomEffect(true);
        CoroutineUtils.DelayCall(this, 4, OffBoomEffect);

        if (!PhotonNetwork.IsMasterClient) return;

        // Ȱ��ȭ�� �� ����
        PoolType[] enemyKeys = { PoolType.EnemyB, PoolType.EnemyL, PoolType.EnemyM, PoolType.EnemyS };
        foreach (PoolType key in enemyKeys)
        {
            foreach (GameObject enemy in PoolManager.Instance.GetPool(key))
            {
                if (enemy.activeSelf)
                {
                    Aircraft enemyLogic = enemy.GetComponent<Aircraft>();
                    enemyLogic.OnHit(100);
                }
            }
        }

        // Ȱ��ȭ �� �� źȯ ����
        PoolType[] bulletKeys = { PoolType.BulletEnemyA, PoolType.BulletEnemyB };
        foreach (PoolType key in bulletKeys)
        {
            foreach (GameObject bullet in PoolManager.Instance.GetPool(key))
            {
                if (bullet.activeSelf)
                    bullet.SetActive(false);
            }
        }
    }

    private void OffBoomEffect()
    {
        GameUIManager.Instance.ActivateBoomEffect(false);
        isBoomTime = false;
        isButtonB = false;
    }
    #endregion

    #region Item
    /// <summary>
    /// ������ Ŭ���̾�Ʈ���� ������ ȹ�� ��û RPC
    /// </summary>
    /// <param name="itemViewID"></param>
    /// <param name="info"></param>
    [PunRPC]
    private void RequestPickUpItem(int itemViewID, PhotonMessageInfo info)
    {
        PhotonView itemView = PhotonView.Find(itemViewID);

        if (itemView != null && itemView.gameObject.activeSelf)
        {
            photonView.RPC(nameof(RPC_GetItem), RpcTarget.All, itemViewID);
        }
    }

    /// <summary>
    /// ������ ȹ�� ó�� RPC
    /// </summary>
    /// <param name="itemViewID"></param>
    [PunRPC]
    private void RPC_GetItem(int itemViewID)
    {
        PhotonView itemView = PhotonView.Find(itemViewID);
        Item item = itemView.gameObject.GetComponent<Item>();

        if (item == null)
            return;

        item.gameObject.SetActive(false);

        if (!photonView.IsMine) return;

        switch (item.type)
        {
            case ItemType.Coin:
                GameManager.Instance.RequestScore(1000);
                break;
            case ItemType.Power:
                if (power == maxPower)
                    GameManager.Instance.RequestScore(500);
                else
                {
                    power++;
                }
                break;
            case ItemType.Boom:
                if (boom == maxBoom)
                    GameManager.Instance.RequestScore(500);
                else
                {
                    boom++;
                    GameUIManager.Instance.UpdateBoomUI(boom);
                }
                break;
        }
    }
    #endregion

    #region Player
    /// <summary>
    /// �÷��̾� ���� �ʱ�ȭ �� ����ġ ����
    /// </summary>
    public void InitPlayer()
    {
        InitTransform();
        isHit = false;
        ActivateController(true);

        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    /// <summary>
    /// Photon ���ҿ� ���� �÷��̾� �ʱ� ��ġ ����
    /// </summary>
    private void InitTransform()
    {
        if (!photonView.IsMine) return;

        if (PhotonNetwork.IsMasterClient)
        {
            transform.position = Vector3.down * 3f + Vector3.left * 1.5f;
        }
        else
        {
            transform.position = Vector3.down * 3f + Vector3.right * 1.5f;
        }
    }

    private void DelayRespawnPlayer()
    {
        photonView.RPC(nameof(RPC_RespawnPlayer), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_RespawnPlayer()
    {
        InitPlayer();
        UnbeatablePlayer();
    }

    /// <summary>
    /// �÷��̾ ���� �ð� ���� ���� ���·� ��ȯ
    /// </summary>
    public void UnbeatablePlayer()
    {
        Unbeatable();
        CoroutineUtils.DelayCall(this, 3, Unbeatable);
    }

    /// <summary>
    /// ���� ���� �� �÷��̾� ���� ������ ó��
    /// </summary>
    private void Unbeatable()
    {
        isRespawnTime = !isRespawnTime;

        if (isRespawnTime)
            spriteRenderer.color = new Color(1, 1, 1, 0.5f);
        else
            spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    /// <summary>
    /// �÷��̾� ���� ���� ���� Ȱ��/��Ȱ��
    /// </summary>
    /// <param name="isActive"></param>
    public void ActivateController(bool isActive)
    {
        isControllable = isActive;
    }
    #endregion

    #region Damage
    /// <summary>
    /// �ǰ� ó�� RPC
    /// HP ���� �� ��� ���� üũ
    /// </summary>
    /// <param name="dmg"></param>
    public override void OnHit(int dmg)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ApplyDamage(dmg);
        }
        else
        {
            photonView.RPC(nameof(RPC_RequestDamage), RpcTarget.MasterClient, dmg);
        }
    }

    [PunRPC]
    private void RPC_RequestDamage(int dmg)
    {
        ApplyDamage(dmg);
    }

    [PunRPC]
    protected override void RPC_OnDamaged(int hp)
    {
        base.RPC_OnDamaged(hp);

        if (photonView.IsMine)
        {
            GameUIManager.Instance.UpdateLifeUI(status.Hp);
        }

        ActivateController(false);
        spriteRenderer.color = new Color(1, 1, 1, 0);

        GameUIManager.Instance.Explosion(transform.position, AircraftType.Player);

        if (!PhotonNetwork.IsMasterClient) return;
        CoroutineUtils.DelayCall(this, 2, DelayRespawnPlayer);
    }

    /// <summary>
    /// UI ����
    /// ���� ����Ʈ �� ���ӿ��� ó��
    /// </summary>
    protected override void OnDeath()
    {
        if (photonView.IsMine)
        {
            GameUIManager.Instance.UpdateLifeUI(0);
        }

        ActivateController(false);
        spriteRenderer.color = new Color(1, 1, 1, 0);

        isDead = true;
        GameUIManager.Instance.Explosion(transform.position, AircraftType.Player);

        if (!PhotonNetwork.IsMasterClient) return;
        
        GameManager.Instance.GameOver();
    }
    #endregion
}