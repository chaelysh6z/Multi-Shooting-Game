using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어를 따라다니며 일정 지연(followDelay) 후 위치를 복제하는 팔로워 클래스
/// 멀티 플레이로 전환 후 사용하지 않음
/// </summary>
public class Follower : Aircraft
{
    [SerializeField] private Transform parent;  // 따라다닐 대상 (플레이어)
    [SerializeField] private int followDelay;   // 따라다니는 지연 프레임 수

    private Vector3 followPos;                  // 현재 따라가야 하는 위치
    private Queue<Vector3> parentPos;           // 부모의 이동 기록 큐

    private float watchTime;                    // 위치 기록 타이밍 조정용

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
    /// 일정 시간마다 부모의 위치를 기록하고 큐에서 꺼내 지연된 위치를 적용
    /// </summary>
    private void Watch()
    {
        if (watchTime < 0.01f)
            return;

        // 부모 위치 기록 (중복 방지)
        if (!parentPos.Contains(parent.position))
            parentPos.Enqueue(parent.position);

        // followDelay 만큼 딜레이된 위치를 적용
        if (parentPos.Count > followDelay)
            followPos = parentPos.Dequeue();
        else if (parentPos.Count < followDelay)
            followPos = parent.position;

        watchTime = 0;
    }

    /// <summary>
    /// 팔로워를 followPos 위치로 이동
    /// </summary>
    private void Follow()
    {
        transform.position = followPos;
    }

    /// <summary>
    /// 플레이어의 입력을 따라 총알 발사
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