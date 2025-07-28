using UnityEngine;

/// <summary>
/// ���� ��� ��ũ���� �����ϴ� Ŭ����
/// ������ Sprite���� ��ȯ���Ѽ� �ڿ������� �̾����� ��� ȿ���� �����
/// </summary>
public class Background : MonoBehaviour
{
    public float speed;         // ����� ��ũ�ѵǴ� �ӵ�
    public int startIndex;      // ���� ȭ�� ���ʿ� �ִ� ��������Ʈ �ε���
    public int endIndex;        // ���� ȭ�� �Ʒ��ʿ� �ִ� ��������Ʈ �ε���
    public Transform[] sprites; // ��� ��������Ʈ �迭

    private float viewHeight;   // ī�޶��� �� ���� (��� ���ġ�� ���)

    private void Awake()
    {
        // ī�޶��� orthographicSize�� �̿��� ȭ�� ���̸� ���
        viewHeight = Camera.main.orthographicSize * 2;
    }

    void Update()
    {
        Move();
        Scrolling();
    }

    /// <summary>
    /// ��� ��ü�� �Ʒ��� �̵�
    /// </summary>
    private void Move()
    {
        Vector3 curPos = transform.position;
        Vector3 nextPos = Vector3.down * speed * Time.deltaTime;
        transform.position = curPos + nextPos;
    }

    /// <summary>
    /// ȭ�� ������ ���� ��������Ʈ�� ���� �÷� ����
    /// </summary>
    private void Scrolling()
    {
        if (sprites[endIndex].position.y < viewHeight * (-1))
        {
            // ��������Ʈ ��ġ ���ġ
            Vector3 backSpritePos = sprites[startIndex].localPosition;
            Vector3 frontSpritePos = sprites[endIndex].localPosition;
            sprites[endIndex].transform.localPosition = backSpritePos + Vector3.up * viewHeight;

            // �ε��� ���� (��ȯ)
            int startIndexSave = startIndex;
            startIndex = endIndex;
            endIndex = (startIndexSave - 1 == -1) ? sprites.Length - 1 : startIndexSave - 1;
        }
    }
}
