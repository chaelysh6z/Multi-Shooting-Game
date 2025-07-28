using UnityEngine;

/// <summary>
/// 무한 배경 스크롤을 구현하는 클래스
/// 지정된 Sprite들을 순환시켜서 자연스럽게 이어지는 배경 효과를 만든다
/// </summary>
public class Background : MonoBehaviour
{
    public float speed;         // 배경이 스크롤되는 속도
    public int startIndex;      // 현재 화면 위쪽에 있는 스프라이트 인덱스
    public int endIndex;        // 현재 화면 아래쪽에 있는 스프라이트 인덱스
    public Transform[] sprites; // 배경 스프라이트 배열

    private float viewHeight;   // 카메라의 뷰 높이 (배경 재배치에 사용)

    private void Awake()
    {
        // 카메라의 orthographicSize를 이용해 화면 높이를 계산
        viewHeight = Camera.main.orthographicSize * 2;
    }

    void Update()
    {
        Move();
        Scrolling();
    }

    /// <summary>
    /// 배경 전체를 아래로 이동
    /// </summary>
    private void Move()
    {
        Vector3 curPos = transform.position;
        Vector3 nextPos = Vector3.down * speed * Time.deltaTime;
        transform.position = curPos + nextPos;
    }

    /// <summary>
    /// 화면 밖으로 나간 스프라이트를 위로 올려 재사용
    /// </summary>
    private void Scrolling()
    {
        if (sprites[endIndex].position.y < viewHeight * (-1))
        {
            // 스프라이트 위치 재배치
            Vector3 backSpritePos = sprites[startIndex].localPosition;
            Vector3 frontSpritePos = sprites[endIndex].localPosition;
            sprites[endIndex].transform.localPosition = backSpritePos + Vector3.up * viewHeight;

            // 인덱스 갱신 (순환)
            int startIndexSave = startIndex;
            startIndex = endIndex;
            endIndex = (startIndexSave - 1 == -1) ? sprites.Length - 1 : startIndexSave - 1;
        }
    }
}
