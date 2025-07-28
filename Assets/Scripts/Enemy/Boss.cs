using Photon.Pun;
using UnityEngine;

/// <summary>
/// ���� ���� ������ ����ϴ� Ŭ����
/// Aircraft�� ��ӹ޾� HP ����, ���� ����, ��Ʈ��ũ ����ȭ ó��
/// </summary>
public class Boss : Aircraft
{
    [SerializeField] private int score;             // óġ �� ȹ�� ����

    [SerializeField] private int patternIndex;      // ���� ���� �ε���
    [SerializeField] private int curPatternCount;   // ���� ���� �ݺ� Ƚ��
    [SerializeField] private int[] maxPatternCount; // �� ���� �ִ� �ݺ� ȸ��

    private Rigidbody2D rigid;
    private Animator anim;
    
    protected override void Awake()
    {
        base.Awake();

        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    /// <summary>
    /// �ϰ� ���� �� 2�� �� ����
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        rigid.linearVelocity = new Vector2(0, status.Speed * (-1));
        CoroutineUtils.DelayCall(this, 2, Stop);
    }

    /// <summary>
    /// ��Ȱ��ȭ �� �ڷ�ƾ ����
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerBullet")) // �÷��̾� �Ѿ˿� ����� ��
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            OnHit(bullet.dmg);

            collision.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// �ϰ� �� �����ϰ� ���� ���� ����
    /// </summary>
    private void Stop()
    {
        if (!gameObject.activeSelf)
            return;

        Rigidbody2D rigid = GetComponent<Rigidbody2D>();
        rigid.linearVelocity = Vector2.zero;

        if (!PhotonNetwork.IsMasterClient) return;

        CoroutineUtils.DelayCall(this, 2, Think);
    }

    /// <summary>
    /// ���� ���� ���� ���� �� ����
    /// ������ ��ȯ�ϸ鼭 �����
    /// </summary>
    private void Think()
    {
        patternIndex = patternIndex == 3 ? 0 : patternIndex + 1;
        curPatternCount = 0;

        switch (patternIndex)
        {
            case 0:
                FireForward();
                break;
            case 1:
                FireShot();
                break;
            case 2:
                FireArc();
                break;
            case 3:
                FireAround();
                break;
        }
    }

    #region Shot
    /// <summary>
    /// ���� 1: �������� 4�� ���� �߻�
    /// </summary>
    private void FireForward()
    {
        if (status.Hp <= 0)
        {
            return;
        }

        photonView.RPC(nameof(RPC_FireForword), RpcTarget.All);

        // Pattern Counting
        curPatternCount++;

        if (curPatternCount < maxPatternCount[patternIndex])
        {
            CoroutineUtils.DelayCall(this, 2, FireForward);
        }
        else
        {
            CoroutineUtils.DelayCall(this, 3, Think);
        }
    }

    /// <summary>
    /// ���� 4�� ���� �߻� ��Ʈ��ũ ����ȭ
    /// </summary>
    [PunRPC]
    private void RPC_FireForword()
    {
        // Fire 4 Bullet Forward
        float[] offsets = { 0.3f, 0.45f, -0.3f, -0.45f };

        foreach (float offset in offsets)
        {
            GameObject bullet = PoolManager.Instance.MakeObj(PoolType.BulletBossA);
            bullet.transform.position = transform.position + Vector3.right * offset;

            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            rigid.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
        }
    }

    /// <summary>
    /// ���� 2: �÷��̾ �ܳ��� ���� ��ź �߻�
    /// </summary>
    private void FireShot()
    {
        if (status.Hp <= 0)
        {
            return;
        }

        Player target;

        if (GameManager.Instance.OtherPlayer != null)
        {
            target = Random.value < 0.5f ? GameManager.Instance.MyPlayer
            : GameManager.Instance.OtherPlayer;
        }
        else
        {
            target = GameManager.Instance.MyPlayer;
        }

        Vector2[] ranVecs = new Vector2[5];
        for (int i = 0; i < 5; i++)
        {
            ranVecs[i] = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(0f, 2f));
        }

        photonView.RPC(nameof(RPC_FireShot), RpcTarget.All, target.transform.position, ranVecs);

        curPatternCount++;

        if (curPatternCount < maxPatternCount[patternIndex])
        {
            CoroutineUtils.DelayCall(this, 3.5f, FireShot);
        }
        else
        {
            CoroutineUtils.DelayCall(this, 3, Think);
        }
    }

    /// <summary>
    /// ��ź �߻� ��Ʈ��ũ ����ȭ
    /// </summary>
    /// <param name="position"></param>
    /// <param name="ranVecs"></param>
    [PunRPC]
    private void RPC_FireShot(Vector3 position, Vector2[] ranVecs)
    {
        for (int index = 0; index < 5; index++)
        {
            GameObject bullet = PoolManager.Instance.MakeObj(PoolType.BulletEnemyB);
            bullet.transform.position = transform.position;
            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Vector2 dirVec = position - transform.position;
            dirVec += ranVecs[index];
            rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);
        }
    }

    /// <summary>
    /// ���� 3: ��ä�� ���� �߻�
    /// </summary>
    private void FireArc()
    {
        if (status.Hp <= 0)
        {
            return;
        }

        photonView.RPC(nameof(RPC_FireArc), RpcTarget.All, curPatternCount, patternIndex);

        // Pattern Counting
        curPatternCount++;

        if (curPatternCount < maxPatternCount[patternIndex])
        {
            CoroutineUtils.DelayCall(this, 0.15f, FireArc);
        }
        else
        {
            CoroutineUtils.DelayCall(this, 3, Think);
        }
    }

    /// <summary>
    /// ��ä�� �߻� ��Ʈ��ũ ����ȭ
    /// </summary>
    /// <param name="count"></param>
    /// <param name="index"></param>
    [PunRPC]
    private void RPC_FireArc(int count, int index)
    {
        // Fire Arc Continue Fire
        GameObject bullet = PoolManager.Instance.MakeObj(PoolType.BulletEnemyA);
        bullet.transform.position = transform.position;
        bullet.transform.rotation = Quaternion.identity;

        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        Vector2 dirVec = new Vector2(Mathf.Cos(Mathf.PI * 10 * count / maxPatternCount[index]), -1);
        rigid.AddForce(dirVec.normalized * 5, ForceMode2D.Impulse);
    }

    /// <summary>
    /// ���� 4: 360�� ���� źȯ �߻�
    /// </summary>
    private void FireAround()
    {
        if (status.Hp <= 0)
        {
            return;
        }

        photonView.RPC(nameof(RPC_FireAround), RpcTarget.All, curPatternCount);

        curPatternCount++;

        if (curPatternCount < maxPatternCount[patternIndex])
        {
            CoroutineUtils.DelayCall(this, 0.7f, FireAround);
        }
        else
        {
            CoroutineUtils.DelayCall(this, 3, Think);
        }
    }

    /// <summary>
    /// ���� źȯ �߻� ��Ʈ��ũ ����ȭ
    /// </summary>
    /// <param name="count"></param>
    [PunRPC]
    private void RPC_FireAround(int count)
    {
        // Fire Around
        int roundNumA = 50;
        int roundNumB = 40;
        int roundNum = count % 2 == 0 ? roundNumA : roundNumB;

        for (int index = 0; index < roundNum; index++)
        {
            GameObject bullet = PoolManager.Instance.MakeObj(PoolType.BulletBossB);
            bullet.transform.position = transform.position;
            bullet.transform.rotation = Quaternion.identity;

            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Vector2 dirVec = new Vector2(Mathf.Cos(Mathf.PI * 2 * index / roundNum),
                                            Mathf.Sin(Mathf.PI * 2 * index / roundNum));
            rigid.AddForce(dirVec.normalized * 2, ForceMode2D.Impulse);

            Vector3 rotVec = Vector3.forward * 360 * index / roundNum + Vector3.forward * 90;
            bullet.transform.Rotate(rotVec);
        }
    }
    #endregion

    /// <summary>
    /// �ǰ� �� �ִϸ��̼� Ʈ���� �ߵ�
    /// </summary>
    /// <param name="hp"></param>
    [PunRPC]
    protected override void RPC_OnDamaged(int hp)
    {
        base.RPC_OnDamaged(hp);
        //status.Hp = hp;
        anim.SetTrigger("OnHit");
    }

    /// <summary>
    /// ���� ��� ó��
    /// ���� ����Ʈ, ������ Ŭ���̾�Ʈ�� ���� ������Ʈ, �������� ����
    /// </summary>
    protected override void OnDeath()
    {
        GameUIManager.Instance.Explosion(transform.position, AircraftType.EnemyB);
        GameManager.Instance.MyPlayer.UnbeatablePlayer();

        if (!PhotonNetwork.IsMasterClient) return;

        GameManager.Instance.UpdateScore(score);
        GameManager.Instance.EndStage();
    }
}
