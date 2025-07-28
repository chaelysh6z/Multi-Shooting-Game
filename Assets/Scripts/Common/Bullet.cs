using UnityEngine;

/// <summary>
/// �÷��̾� �� ���� �Ѿ� ������Ʈ
/// </summary>
public class Bullet : MonoBehaviour
{
    public int dmg;         // �Ѿ��� ���ݷ�
    public bool isRotate;   // ȸ�� ����

    private void Update()
    {
        // isRotate�� true�� �Ѿ��� ȸ���ϸ鼭 �̵�
        if (isRotate)
        {
            transform.Rotate(Vector3.forward * 10);
        }
    }

    /// <summary>
    /// ȭ�� ��(BorderBullet)�� ������ ��Ȱ��ȭ
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("BorderBullet"))
        {
            gameObject.SetActive(false);
        }
    }
}