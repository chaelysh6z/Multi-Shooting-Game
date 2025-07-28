using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �÷��̾ ����ٴϸ� ���� ����(followDelay) �� ��ġ�� �����ϴ� �ȷο� Ŭ����
/// ��Ƽ �÷��̷� ��ȯ �� ������� ����
/// </summary>
public class Follower : Aircraft
{
    [SerializeField] private Transform parent;  // ����ٴ� ��� (�÷��̾�)
    [SerializeField] private int followDelay;   // ����ٴϴ� ���� ������ ��

    private Vector3 followPos;                  // ���� ���󰡾� �ϴ� ��ġ
    private Queue<Vector3> parentPos;           // �θ��� �̵� ��� ť

    private float watchTime;                    // ��ġ ��� Ÿ�̹� ������

    protected override void Awake()
    {
        parentPos = new Queue<Vector3>();
    }

    void Update()
    {
        watchTime += Time.deltaTime;

        Watch();
        Follow();
        Shot();
        Reload();
    }

    /// <summary>
    /// ���� �ð����� �θ��� ��ġ�� ����ϰ� ť���� ���� ������ ��ġ�� ����
    /// </summary>
    private void Watch()
    {
        if (watchTime < 0.01f)
            return;

        // �θ� ��ġ ��� (�ߺ� ����)
        if (!parentPos.Contains(parent.position))
            parentPos.Enqueue(parent.position);

        // followDelay ��ŭ �����̵� ��ġ�� ����
        if (parentPos.Count > followDelay)
            followPos = parentPos.Dequeue();
        else if (parentPos.Count < followDelay)
            followPos = parent.position;

        watchTime = 0;
    }

    /// <summary>
    /// �ȷο��� followPos ��ġ�� �̵�
    /// </summary>
    private void Follow()
    {
        transform.position = followPos;
    }

    /// <summary>
    /// �÷��̾��� �Է��� ���� �Ѿ� �߻�
    /// </summary>
    private void Shot()
    {
        if (curShotDelay < maxShotDelay)
        {
            return;
        }

        GameObject bullet = PoolManager.Instance.MakeObj(PoolType.BulletFollower);
        bullet.transform.position = transform.position;

        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();

        rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);

        curShotDelay = 0;
    }
}