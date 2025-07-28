using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 적 캐릭터의 로직을 담당하는 클래스
/// Aircraft를 상속받아 HP, 발사 패턴, 아이템 드랍, 네트워크 동기화 처리
/// </summary>
public class Enemy : Aircraft
{
    [SerializeField] private Sprite[] sprites;  // 0: 기본 상태, 1: 피격 상태

    [SerializeField] private int score;         // 처치 시 획득 점수

    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 마스터 클라이언트에서만 적의 발사/재장전 처리
    /// </summary>
    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Shot();
        Reload();
    }

    /// <summary>
    /// 회전 초기화, 스프라이트 기본 상태로 복구
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        transform.rotation = Quaternion.identity;

        ReturnSprite();
    }

    /// <summary>
    /// 비활성화 시 PhotonView ID 초기화 및 코루틴 정지
    /// </summary>
    private void OnDisable()
    {
        photonView.ViewID = 0;

        StopAllCoroutines();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("BorderBullet"))    // 총알 경계에 닿았을 때
        {
            transform.rotation = Quaternion.identity;
            gameObject.SetActive(false);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))   // 플레이어 총알에 닿았을 때
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            OnHit(bullet.dmg);

            collision.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 적 스폰 시 위치/방향 초기화
    /// point 값에 따라 좌우/전방 스폰 방향 설정
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
    /// 적 발사 로직
    /// 플레이어 방향으로 탄환 생성 후 RPC로 동기화
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

        // 두 플레이어 중 무작위 타겟 선택
        if (GameManager.Instance.OtherPlayer != null)
        {
            target = Random.value < 0.5f ? GameManager.Instance.MyPlayer
            : GameManager.Instance.OtherPlayer;
        }
        else
        {
            target = GameManager.Instance.MyPlayer;
        }

        // 소형 적: 단발 사격
        if (status.Type == AircraftType.EnemyS)
        {
            poolType = PoolType.BulletEnemyA;
            Vector3 dirVec = target.transform.position - transform.position;
            force = dirVec.normalized * 3;

            photonView.RPC(nameof(RPC_Shot), RpcTarget.All, (int)poolType, transform.position, force);
        }
        // 대형 적: 양쪽에서 두발 사격
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
    /// 네트워크 상에서 탄환 생성 및 힘 적용
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
    /// 피격 시 피격 스프라이트 변경 후 잠시 뒤 복구
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
    /// 스프라이트를 기본 상태로 복구
    /// </summary>
    private void ReturnSprite()
    {
        spriteRenderer.sprite = sprites[0];
    }

    /// <summary>
    /// 사망 RPC
    /// 실제 처리는 OnDeath에서 수행
    /// </summary>
    [PunRPC]
    protected override void RPC_OnDead()
    {
        OnDeath();
    }

    /// <summary>
    /// 적 사망 처리
    /// 폭발 이펙트, 마스터 클라이언트에서만 점수 갱신, 아이템 드랍
    /// </summary>
    protected override void OnDeath()
    {
        GameUIManager.Instance.Explosion(transform.position, status.Type);

        if (!PhotonNetwork.IsMasterClient) return;

        GameManager.Instance.UpdateScore(score);
        SpawnItem();
    }

    /// <summary>
    /// 랜덤 확률로 코인/파워/폭탄 선택
    /// 모두에게 RPC로 스폰 처리
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
    /// 아이템 스폰 동기화
    /// 스폰된 아이템의 PhotonView ID 동기화
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