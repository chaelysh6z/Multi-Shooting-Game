using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// �� ĳ������ ������ ����ϴ� Ŭ����
/// Aircraft�� ��ӹ޾� HP, �߻� ����, ������ ���, ��Ʈ��ũ ����ȭ ó��
/// </summary>
public class Enemy : Aircraft
{
    [SerializeField] private Sprite[] sprites;  // 0: �⺻ ����, 1: �ǰ� ����

    [SerializeField] private int score;         // óġ �� ȹ�� ����

    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// ������ Ŭ���̾�Ʈ������ ���� �߻�/������ ó��
    /// </summary>
    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Shot();
        Reload();
    }

    /// <summary>
    /// ȸ�� �ʱ�ȭ, ��������Ʈ �⺻ ���·� ����
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        transform.rotation = Quaternion.identity;

        ReturnSprite();
    }

    /// <summary>
    /// ��Ȱ��ȭ �� PhotonView ID �ʱ�ȭ �� �ڷ�ƾ ����
    /// </summary>
    private void OnDisable()
    {
        photonView.ViewID = 0;

        StopAllCoroutines();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("BorderBullet"))    // �Ѿ� ��迡 ����� ��
        {
            transform.rotation = Quaternion.identity;
            gameObject.SetActive(false);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))   // �÷��̾� �Ѿ˿� ����� ��
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            OnHit(bullet.dmg);

            collision.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// �� ���� �� ��ġ/���� �ʱ�ȭ
    /// point ���� ���� �¿�/���� ���� ���� ����
    /// </summary>
    /// <param name="point"></param>
    public void InitTransform(int point)
    {
        Rigidbody2D rigid = GetComponent<Rigidbody2D>();

        if (point == 5 || point == 6) // Right Spawn
        {
            transform.Rotate(Vector3.back * 90);
            rigid.linearVelocity = new Vector2(status.Speed * (-1), -1);
        }
        else if (point == 7 || point == 8) // Left Spawn
        {
            transform.Rotate(Vector3.forward * 90);
            rigid.linearVelocity = new Vector2(status.Speed, -1);
        }
        else // Front Spawn
        {
            rigid.linearVelocity = new Vector2(0, status.Speed * (-1));
        }
    }

    #region Shot
    /// <summary>
    /// �� �߻� ����
    /// �÷��̾� �������� źȯ ���� �� RPC�� ����ȭ
    /// </summary>
    private void Shot()
    {
        if (curShotDelay < maxShotDelay)
        {
            return;
        }

        Vector3 force;
        PoolType poolType;
        Player target;

        // �� �÷��̾� �� ������ Ÿ�� ����
        if (GameManager.Instance.OtherPlayer != null)
        {
            target = Random.value < 0.5f ? GameManager.Instance.MyPlayer
            : GameManager.Instance.OtherPlayer;
        }
        else
        {
            target = GameManager.Instance.MyPlayer;
        }

        // ���� ��: �ܹ� ���
        if (status.Type == AircraftType.EnemyS)
        {
            poolType = PoolType.BulletEnemyA;
            Vector3 dirVec = target.transform.position - transform.position;
            force = dirVec.normalized * 3;

            photonView.RPC(nameof(RPC_Shot), RpcTarget.All, (int)poolType, transform.position, force);
        }
        // ���� ��: ���ʿ��� �ι� ���
        else if (status.Type == AircraftType.EnemyL)
        {
            poolType = PoolType.BulletEnemyB;

            float[] offsets = { 0.3f, -0.3f };

            foreach (float offset in offsets)
            {
                Vector3 position = transform.position + Vector3.right * offset;
                Vector3 dirVec = target.transform.position - position;
                force = dirVec.normalized * 4;

                photonView.RPC(nameof(RPC_Shot), RpcTarget.All, (int)poolType, position, force);
            }
        }

        curShotDelay = 0;
    }

    /// <summary>
    /// ��Ʈ��ũ �󿡼� źȯ ���� �� �� ����
    /// </summary>
    /// <param name="type"></param>
    /// <param name="position"></param>
    /// <param name="force"></param>
    [PunRPC]
    private void RPC_Shot(int type, Vector3 position, Vector3 force)
    {
        GameObject bullet = PoolManager.Instance.MakeObj((PoolType)type);
        bullet.transform.position = position;

        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        rigid.AddForce(force, ForceMode2D.Impulse);
    }
    #endregion

    #region Damage & Item
    /// <summary>
    /// �ǰ� �� �ǰ� ��������Ʈ ���� �� ��� �� ����
    /// </summary>
    /// <param name="hp"></param>
    [PunRPC]
    protected override void RPC_OnDamaged(int hp)
    {
        base.RPC_OnDamaged(hp);
        spriteRenderer.sprite = sprites[1];
        CoroutineUtils.DelayCall(this, 0.1f, ReturnSprite);
    }

    /// <summary>
    /// ��������Ʈ�� �⺻ ���·� ����
    /// </summary>
    private void ReturnSprite()
    {
        spriteRenderer.sprite = sprites[0];
    }

    /// <summary>
    /// ��� RPC
    /// ���� ó���� OnDeath���� ����
    /// </summary>
    [PunRPC]
    protected override void RPC_OnDead()
    {
        OnDeath();
    }

    /// <summary>
    /// �� ��� ó��
    /// ���� ����Ʈ, ������ Ŭ���̾�Ʈ������ ���� ����, ������ ���
    /// </summary>
    protected override void OnDeath()
    {
        GameUIManager.Instance.Explosion(transform.position, status.Type);

        if (!PhotonNetwork.IsMasterClient) return;

        GameManager.Instance.UpdateScore(score);
        SpawnItem();
    }

    /// <summary>
    /// ���� Ȯ���� ����/�Ŀ�/��ź ����
    /// ��ο��� RPC�� ���� ó��
    /// </summary>
    private void SpawnItem()
    {
        int ran = Random.Range(0, 10);

        PoolType itemType = PoolType.ItemCoin;
        if (ran < 6) itemType = PoolType.ItemCoin;
        else if (ran < 8) itemType = PoolType.ItemPower;
        else if (ran < 10) itemType = PoolType.ItemBoom;

        int viewID = PhotonNetwork.AllocateViewID(true);

        photonView.RPC(nameof(RPC_SpawnItem), RpcTarget.All, (int)itemType, transform.position, viewID);
    }

    /// <summary>
    /// ������ ���� ����ȭ
    /// ������ �������� PhotonView ID ����ȭ
    /// </summary>
    /// <param name="type"></param>
    /// <param name="position"></param>
    /// <param name="viewID"></param>
    [PunRPC]
    private void RPC_SpawnItem(int type, Vector3 position, int viewID)
    {
        PoolType poolType = (PoolType)type;

        GameObject item = PoolManager.Instance.MakeObj(poolType);
        item.transform.position = position;

        PhotonView view = item.GetComponent<PhotonView>();
        view.ViewID = viewID;

        GameUIManager.Instance.Explosion(transform.position, status.Type);
        gameObject.SetActive(false);
    }
    #endregion
}