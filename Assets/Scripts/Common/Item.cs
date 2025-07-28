using Photon.Pun;
using UnityEngine;

/// <summary>
/// ������ ������Ʈ�� �̵� �� �浹 ó���� ���
/// </summary>
public class Item : MonoBehaviour
{
    public ItemType type;   // ������ Ÿ��

    private Rigidbody2D rigid;
    private PhotonView photonView;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
    }

    /// <summary>
    /// ������ Ȱ��ȭ �� �Ʒ��� ���������� �ӵ� ����
    /// </summary>
    private void OnEnable()
    {
        rigid.linearVelocity = Vector2.down * 1f;
    }

    /// <summary>
    /// Ǯ�� ��ȯ�� �� PhotonView ID �ʱ�ȭ
    /// </summary>
    private void OnDisable()
    {
        photonView.ViewID = 0;
    }

    /// <summary>
    /// ȭ�� ��(BorderBullet)�� ������ ��Ȱ��ȭ
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
