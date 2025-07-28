using UnityEngine;

/// <summary>
/// ���� ����Ʈ�� �����ϴ� ��ũ��Ʈ
/// </summary>
public class Explosion : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    /// <summary>
    /// Ȱ��ȭ �� 2�� �� �ڵ� ��Ȱ��ȭ
    /// </summary>
    private void OnEnable()
    {
        CoroutineUtils.DelayCall(this, 2, Disable);
    }

    /// <summary>
    /// Ǯ�� ��ȯ �� �ڷ�ƾ ����
    /// </summary>
    private void Disable()
    {
        gameObject.SetActive(false);

        StopAllCoroutines();
    }

    /// <summary>
    /// ���� �ִϸ��̼� ��� �� ũ�� ����
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
