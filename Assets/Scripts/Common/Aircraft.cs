using Photon.Pun;
using System;
using UnityEngine;

/// <summary>
/// �÷��̾�� �� ����� ��ΰ� ��ӹ޴� �߻� Ŭ����
/// ü��, �ӵ�, ���� ������ �� �������� ���¿� ������ ó���� ���
/// </summary>
public abstract class Aircraft : MonoBehaviourPun
{
    /// <summary>
    /// ������� �⺻ ���� ������ (Ÿ��, HP, �ӵ�)
    /// ����ȭ �����ϵ��� ���� Ŭ���� ���·� ����
    /// </summary>
    [Serializable]
    public class AircraftStatus
    {
        public AircraftType Type;
        public int Hp;
        public float Speed;
    }

    [SerializeField] public AircraftStatus status;  // ����� ���°�

    [SerializeField] protected float curShotDelay;  // ���� �߻� ������
    [SerializeField] protected float maxShotDelay;  // �ִ� �߻� ������

    protected int initHp;   // �ʱ� HP �� ����

    /// <summary>
    /// �ʱ� HP �� ����
    /// </summary>
    protected virtual void Awake()
    {
        initHp = status.Hp;
    }

    /// <summary>
    /// ������Ʈ�� Ȱ��ȭ�� �� HP�� �ʱ�ȭ
    /// </summary>
    protected virtual void OnEnable()
    {
        status.Hp = initHp;
    }

    /// <summary>
    /// �߻� �����̸� �� ������ ������Ű�� ���� �޼���
    /// </summary>
    protected void Reload()
    {
        curShotDelay += Time.deltaTime;
    }

    /// <summary>
    /// �ǰ� ó��
    /// ������ Ŭ���̾�Ʈ������ �������� ����
    /// </summary>
    /// <param name="dmg"></param>
    public virtual void OnHit(int dmg)
    {
        if (status.Hp <= 0) return;

        if (PhotonNetwork.IsMasterClient)
            ApplyDamage(dmg);
    }

    /// <summary>
    /// ���� ������ ���� ����
    /// HP�� 0 ���ϰ� �Ǹ� ��� ó�� RPC ȣ��
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
    /// ������ ����ȭ RPC
    /// HP�� ��� Ŭ���̾�Ʈ�� �ݿ�
    /// </summary>
    /// <param name="hp"></param>
    [PunRPC]
    protected virtual void RPC_OnDamaged(int hp)
    {
        status.Hp = hp;
    }

    /// <summary>
    /// ��� ó�� RPC
    /// �⺻������ ������Ʈ�� ��Ȱ��ȭ
    /// </summary>
    [PunRPC]
    protected virtual void RPC_OnDead()
    {
        OnDeath();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// ��� �� �߰� ������ �ʿ��� ��� �ڽ� Ŭ�������� �������̵�
    /// </summary>
    protected virtual void OnDeath() { }
}

