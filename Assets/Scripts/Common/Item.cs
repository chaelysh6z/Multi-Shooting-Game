using Photon.Pun;
using UnityEngine;

/// <summary>
/// 아이템 오브젝트의 이동 및 충돌 처리를 담당
/// </summary>
public class Item : MonoBehaviour
{
    public ItemType type;   // 아이템 타입

    private Rigidbody2D rigid;
    private PhotonView photonView;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
    }

    /// <summary>
    /// 아이템 활성화 시 아래로 떨어지도록 속도 설정
    /// </summary>
    private void OnEnable()
    {
        rigid.linearVelocity = Vector2.down * 1f;
    }

    /// <summary>
    /// 풀에 반환될 때 PhotonView ID 초기화
    /// </summary>
    private void OnDisable()
    {
        photonView.ViewID = 0;
    }

    /// <summary>
    /// 화면 밖(BorderBullet)에 닿으면 비활성화
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("BorderBullet"))
        {
            transform.rotation = Quaternion.identity;
            gameObject.SetActive(false);
        }
    }
}
