using UnityEngine;

/// <summary>
/// 플레이어 및 적의 총알 오브젝트
/// </summary>
public class Bullet : MonoBehaviour
{
    public int dmg;         // 총알의 공격력
    public bool isRotate;   // 회전 여부

    private void Update()
    {
        // isRotate가 true면 총알이 회전하면서 이동
        if (isRotate)
        {
            transform.Rotate(Vector3.forward * 10);
        }
    }

    /// <summary>
    /// 화면 밖(BorderBullet)에 닿으면 비활성화
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