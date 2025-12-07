using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ����ʵ��
    public static GameManager Instance;

    [Header("����ģ������")]
    public Deck Deck;
    public Hand Hand;
    public Pile DiscardPile;
    public GameObject Player;

    [Header("Ԥ��������")]
    public GameObject CardPrefab;

    [Header("UI������")]
    public Transform handTransform; // ���Ƶĸ�����UGUI��Canvas�����壩

    [Header("�غ�����")]
    public int defaultDrawCount = 5; // �� Inspector �е���Ĭ�ϳ�����

    [Header("��������")]
    public int maxCost = 3; // �����ã�ÿ�غ����ޣ�
    public int currentCost;  // ��ǰ���÷���

    private void Awake()
    {
        // ������ʼ��
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ResetCost()
    {
        currentCost = maxCost;
        Debug.Log($"�غϿ�ʼ����������Ϊ��{currentCost}/{maxCost}");
        // ��ѡ������UI��ʾ���ã�����UIˢ�·�����
        UpdateCostUI();
    }

    // �۳����ã������Ƿ�۳��ɹ���
    public bool SpendCost(int cost)
    {
        if (currentCost >= cost)
        {
            currentCost -= cost;
            Debug.Log($"�۳����ã�{cost}��ʣ����ã�{currentCost}");
            UpdateCostUI(); // ����UI
            return true;
        }
        else
        {
            Debug.LogWarning($"���ò��㣡��ǰ��{currentCost}����Ҫ��{cost}");
            return false;
        }
    }

    public void AddCost(int amount)
    {
        currentCost = Mathf.Min(currentCost + amount, maxCost); // ������������
        UpdateCostUI();
    }

    public void UpdateCostUI()
    {
        if (CostUIManager.Instance != null)
        {
            CostUIManager.Instance.UpdateCostUI(currentCost, maxCost);
        }
    }

    public void StartTurn() => StartTurn(defaultDrawCount);

    public void StartTurn(int drawCount)
    {
        Debug.Log($"=== �غϿ�ʼ === drawCount={drawCount}");
        var gmInst = Instance;
        Debug.Log($"GameManager.Instance != null: {gmInst != null}");
        Debug.Log($"Deck reference set: {Deck != null} | Deck.Instance: {Deck?.GetRemainingCards() ?? -1}");
        if (Deck != null)
        {
            Debug.Log($"Deck.initialCards count: {Deck.initialCards?.Count ?? -1} | Deck pool: {Deck.GetRemainingCards()}");
        }

        if (Instance == null)
        {
            Debug.LogError("StartTurn ����ʱ GameManager.Instance Ϊ null��ȷ�� GameManager ���ڳ����в����á�");
            return;
        }
        if (Deck == null)
        {
            Debug.LogError("StartTurn��Deck ����Ϊ null������ Inspector �� Deck��");
            return;
        }
        if (Hand == null)
        {
            Debug.LogError("StartTurn��Hand ����Ϊ null������ Inspector �� Hand��");
            return;
        }

        ResetCost(); // ���ӡ����������־

        for (int i = 0; i < drawCount; i++)
        {
            Debug.Log($"StartTurn: ���Գ��� i={i}");
            Card drawnCard = Deck.DrawCard();
            Debug.Log($"StartTurn: DrawCard ���� {(drawnCard == null ? "null" : drawnCard.name)}");
            if (drawnCard == null)
            {
                Debug.LogWarning($"StartTurn: �� {i} �γ��Ʒ��� null�������ƿ�Ϊ�ջ�Ԥ����/�ű���������");
            }
            else
            {
                Hand.AddCard(drawnCard);
                Debug.Log($"StartTurn: �� {i} �γ鵽��Ƭ {drawnCard.name}");
            }
        }

        // ��ϣ��г� handTransform / HandPanel �µ��Ӷ���
        if (handTransform != null)
        {
            Debug.Log($"StartTurn: handTransform childCount = {handTransform.childCount}");
            for (int i = 0; i < handTransform.childCount; i++)
            {
                Debug.Log($" - hand child[{i}] = {handTransform.GetChild(i).name}");
            }
        }
        if (Hand != null && Hand.HandPanel != null)
        {
            Debug.Log($"StartTurn: Hand.HandPanel childCount = {Hand.HandPanel.childCount}");
            for (int i = 0; i < Hand.HandPanel.childCount; i++)
            {
                Debug.Log($" - HandPanel child[{i}] = {Hand.HandPanel.GetChild(i).name}");
            }
        }

        Debug.Log("StartTurn: ����ѭ������");
    }

    public void EndTurn()
    {
        Debug.Log("=== �غϽ��� ===");
        Hand.DiscardAllCards();
    }
}
