using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���� �� UI(���ھ�, ������, �������� ����, ���ӿ��� ��)�� �����ϴ� �Ŵ���
/// </summary>
public class GameUIManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    private static GameUIManager instance;
    public static GameUIManager Instance => instance;

    [Header("UI Object")]
    [SerializeField] private Button startButton;        // ���� ���� ��ư
    [SerializeField] private Button lobbyButton;        // �κ�� ���ư��� ��ư
    [SerializeField] private Button retryButton;        // ����� ��ư
    [SerializeField] private TMP_Text scoreText;        // ���� ǥ�� �ؽ�Ʈ
    [SerializeField] private Image[] lifeImage;         // �÷��̾� ��� ������
    [SerializeField] private Image[] boomImage;         // �÷��̾� ��ź ������
    [SerializeField] private GameObject gameOverText;   // ���� ���� �ؽ�Ʈ
    [SerializeField] private GameObject gameClearText;  // ���� Ŭ���� �ؽ�Ʈ
    [SerializeField] private GameObject boomEffect;     // ��ź ��� �� ȿ��

    [Header("Animator")]
    [SerializeField] private Animator stageAnim;        // �������� ���� �ִϸ��̼�
    [SerializeField] private Animator clearAnim;        // �������� Ŭ���� �ִϸ��̼�
    [SerializeField] private Animator fadeAnim;         // ȭ�� ��ȯ ���̵� �ִϸ��̼�

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // ������ Ŭ���̾�Ʈ�� ���� ��ư Ȱ��ȭ
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.gameObject.SetActive(true);
            startButton.onClick.AddListener(OnClickStartButton);
        }

        // ��ư �̺�Ʈ �ʱ�ȭ
        lobbyButton.onClick.AddListener(OnClickLobbyButton);
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ������ ����
        startButton?.onClick.RemoveListener(OnClickStartButton);
        lobbyButton?.onClick.RemoveListener(OnClickLobbyButton);
    }

    /// <summary>
    /// ���� ���� ��ư Ŭ�� �̺�Ʈ
    /// </summary>
    private void OnClickStartButton()
    {
        GameManager.Instance.StartStage();
        startButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// �κ� ��ư Ŭ�� �̺�Ʈ
    /// </summary>
    private void OnClickLobbyButton()
    {
        NetworkManager.Instance.LeaveRoom();
    }

    /// <summary>
    /// �������� ���� ������ Ȱ��ȭ
    /// </summary>
    /// <param name="stage"></param>
    public void ActivateStageUI(int stage)
    {
        // Stage UI Load
        stageAnim.GetComponent<TMP_Text>().text = "Stage " + stage + "\nStart";

        stageAnim.SetTrigger("On");

        // Fade In
        fadeAnim.SetTrigger("In");
    }

    /// <summary>
    /// �������� Ŭ���� ������ Ȱ��ȭ
    /// </summary>
    /// <param name="stage"></param>
    public void ActivateClearUI(int stage)
    {
        // Clear UI Load
        clearAnim.GetComponent<TMP_Text>().text = "Stage " + stage + "\nClear!";
        clearAnim.SetTrigger("On");

        // Fade Out
        fadeAnim.SetTrigger("Out");
    }

    /// <summary>
    /// ���� Ŭ���� UI Ȱ��ȭ
    /// </summary>
    /// <param name="isActivae"></param>
    public void ActivateGameClearUI(bool isActivae)
    {
        gameClearText.SetActive(isActivae);

        if (PhotonNetwork.IsMasterClient)
            retryButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// ���� ���� UI Ȱ��ȭ
    /// </summary>
    /// <param name="isActive"></param>
    public void ActivateGameOverUI(bool isActive)
    {
        gameOverText.SetActive(isActive);

        if (PhotonNetwork.IsMasterClient)
            retryButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// ���� UI ������Ʈ
    /// </summary>
    /// <param name="score"></param>
    public void UpdateScoreUI(int score)
    {
        // UI Score Update
        scoreText.text = string.Format("{0:n0}", score);
    }

    /// <summary>
    /// �÷��̾� ��� ������ ������Ʈ
    /// </summary>
    /// <param name="life"></param>
    public void UpdateLifeUI(int life)
    {
        // UI Life Init Disable
        for (int index = 0; index < lifeImage.Length; index++)
        {
            lifeImage[index].color = new Color(1, 1, 1, 0);
        }

        // UI Life Active
        for (int index = 0; index < life; index++)
        {
            lifeImage[index].color = new Color(1, 1, 1, 1);
        }
    }

    /// <summary>
    /// �÷��̾� ��ź ������ ������Ʈ
    /// </summary>
    /// <param name="boom"></param>
    public void UpdateBoomUI(int boom)
    {
        // UI Boom Init Disable
        for (int index = 0; index < boomImage.Length; index++)
        {
            boomImage[index].color = new Color(1, 1, 1, 0);
        }

        // UI Boom Active
        for (int index = 0; index < boom; index++)
        {
            boomImage[index].color = new Color(1, 1, 1, 1);
        }
    }

    /// <summary>
    /// ���� ����Ʈ�� Ǯ���� ������ ����
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="type"></param>
    public void Explosion(Vector3 pos, AircraftType type)
    {
        var go = PoolManager.Instance.MakeObj(PoolType.Explosion);
        var explosion = go.GetComponent<Explosion>();

        go.transform.transform.position = pos;
        explosion.StartExplosion(type);
    }

    /// <summary>
    /// ��ź ȿ�� Ȱ��ȭ
    /// </summary>
    /// <param name="isActive"></param>
    public void ActivateBoomEffect(bool isActive)
    {
        boomEffect.SetActive(isActive);
    }
}