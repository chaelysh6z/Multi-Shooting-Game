using UnityEngine;

/// <summary>
/// 폭발 이펙트를 제어하는 스크립트
/// </summary>
public class Explosion : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    /// <summary>
    /// 활성화 시 2초 후 자동 비활성화
    /// </summary>
    private void OnEnable()
    {
        CoroutineUtils.DelayCall(this, 2, Disable);
    }

    /// <summary>
    /// 풀로 반환 및 코루틴 정리
    /// </summary>
    private void Disable()
    {
        gameObject.SetActive(false);

        StopAllCoroutines();
    }

    /// <summary>
    /// 폭발 애니메이션 재생 및 크기 조절
    /// </summary>
    /// <param name="target"></param>
    public void StartExplosion(AircraftType target)
    {
        anim.SetTrigger("OnExplosion");

        switch (target)
        {
            case AircraftType.EnemyS:
                transform.localScale = Vector3.one * 0.7f;
                break;
            case AircraftType.Player:
            case AircraftType.EnemyM:
                transform.localScale = Vector3.one * 1f;
                break;
            case AircraftType.EnemyL:
                transform.localScale = Vector3.one * 2f;
                break;
            case AircraftType.EnemyB:
                transform.localScale = Vector3.one * 3f;
                break;
        }
    }
}
