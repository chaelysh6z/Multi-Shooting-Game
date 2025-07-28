using Photon.Pun;
using System;
using UnityEngine;

/// <summary>
/// 플레이어와 적 비행기 모두가 상속받는 추상 클래스
/// 체력, 속도, 공격 딜레이 등 공통적인 상태와 데미지 처리를 담당
/// </summary>
public abstract class Aircraft : MonoBehaviourPun
{
    /// <summary>
    /// 비행기의 기본 상태 데이터 (타입, HP, 속도)
    /// 직렬화 가능하도록 내부 클래스 형태로 구현
    /// </summary>
    [Serializable]
    public class AircraftStatus
    {
        public AircraftType Type;
        public int Hp;
        public float Speed;
    }

    [SerializeField] public AircraftStatus status;  // 비행기 상태값

    [SerializeField] protected float curShotDelay;  // 현재 발사 딜레이
    [SerializeField] protected float maxShotDelay;  // 최대 발사 딜레이

    protected int initHp;   // 초기 HP 값 저장

    /// <summary>
    /// 초기 HP 값 설정
    /// </summary>
    protected virtual void Awake()
    {
        initHp = status.Hp;
    }

    /// <summary>
    /// 오브젝트가 활성화될 때 HP를 초기화
    /// </summary>
    protected virtual void OnEnable()
    {
        status.Hp = initHp;
    }

    /// <summary>
    /// 발사 딜레이를 매 프레임 증가시키는 공통 메서드
    /// </summary>
    protected void Reload()
    {
        curShotDelay += Time.deltaTime;
    }

    /// <summary>
    /// 피격 처리
    /// 마스터 클라이언트에서만 데미지를 적용
    /// </summary>
    /// <param name="dmg"></param>
    public virtual void OnHit(int dmg)
    {
        if (status.Hp <= 0) return;

        if (PhotonNetwork.IsMasterClient)
            ApplyDamage(dmg);
    }

    /// <summary>
    /// 실제 데미지 적용 로직
    /// HP가 0 이하가 되면 사망 처리 RPC 호출
    /// </summary>
    /// <param name="dmg"></param>
    protected virtual void ApplyDamage(int dmg)
    {
        status.Hp -= dmg;

        if (status.Hp <= 0)
            photonView.RPC(nameof(RPC_OnDead), RpcTarget.All);
        else
            photonView.RPC(nameof(RPC_OnDamaged), RpcTarget.All, status.Hp);
    }

    /// <summary>
    /// 데미지 동기화 RPC
    /// HP를 모든 클라이언트에 반영
    /// </summary>
    /// <param name="hp"></param>
    [PunRPC]
    protected virtual void RPC_OnDamaged(int hp)
    {
        status.Hp = hp;
    }

    /// <summary>
    /// 사망 처리 RPC
    /// 기본적으로 오브젝트를 비활성화
    /// </summary>
    [PunRPC]
    protected virtual void RPC_OnDead()
    {
        OnDeath();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 사망 시 추가 동작이 필요한 경우 자식 클래스에서 오버라이드
    /// </summary>
    protected virtual void OnDeath() { }
}

