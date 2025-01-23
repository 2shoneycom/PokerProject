using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] ItemSO itemSO;
    [SerializeField] GameObject cardPrefabs;        // ������ ī�� ������
    [SerializeField] public List<Card> dealerCards;        // ���� ���� ī�� ����Ʈ
    [SerializeField] Transform cardSpawnPoint;      // ���� ī�� ���� ��ġ (������)
    [SerializeField] Transform[] dealerCardSpawnPos;// ������ ī�� ��ġ

    List<Item> cardBuffer;      // ī�� ��
    public List<char> cardShape;       // 0-12 �� Ŭ�ι�, 13-25 �� ���̾�, 26-38 �� ��Ʈ, 39-51 �� �����̵�
    public List<int> cardNum;          // 0-12 �� 1-13�� ����Ǿ�, Ŭ�ι��� ���ڸ� ��Ÿ��, ���ĵ� �����ϰ� �۵�

    void Start()
    {
        SetupCard();            // ���� ���۽� ī�� �� ����
        ShuffleCard();
        TurnManager.OnAddCard += AddCard;
    }

    void OnDestroy()
    {
        TurnManager.OnAddCard -= AddCard;
    }

    void SetupCard()
    {
        cardShape = new List<char>(52);
        cardNum = new List<int>(52);
        cardBuffer = new List<Item>(52);

        for(int i = 0; i < 52; i++)
        {
            int cS = i / 13;
            switch (cS)
            {
                case 0: cardShape.Add('C'); break;
                case 1: cardShape.Add('D'); break;
                case 2: cardShape.Add('H'); break;
                case 3: cardShape.Add('S'); break;
            }
            cardNum.Add(i%13+1);
        }
    }

    public void ShuffleCard()
    {
        cardBuffer.Clear();

        for (int i = 0; i < 52; i++)
        {
            cardBuffer.Add(itemSO.items[i]);
        }

        for (int i = 0; i < cardBuffer.Count; i++)       // �� ���� ����
        {
            int rand = Random.Range(i, cardBuffer.Count);
            Item temp = cardBuffer[i];
            cardBuffer[i] = cardBuffer[rand];
            cardBuffer[rand] = temp;
        }
    }

    public Item PopItem()           // 0��° ī�嵦 pop
    {
        Item item = cardBuffer[0];
        cardBuffer.RemoveAt(0);
        return item;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void AddCard(int playerIndex)   // ī�� ���� ����
    {
        // ī�� ������Ʈ ����
        var cardObject = Instantiate(cardPrefabs, cardSpawnPoint.position, Quaternion.identity);
        // ī���� Card ��ũ��Ʈ�� ������ ���� 0��° ī��� �¾� 
        var card = cardObject.GetComponent<Card>();
        card.Setup(PopItem(), playerIndex);

        // ī�带 �˸��� ����Ʈ�� �߰�
        if(playerIndex == GameManager.Inst.dealer)
            dealerCards.Add(card);
        else
        {
            Player nowPlayer = PlayerManager.Inst.players[playerIndex];
            nowPlayer?.myCards.Add(card);
        }
        // ī�� ������ �Լ� ȣ��
        CardMoveToPos(playerIndex);
    }

    void CardMoveToPos(int playerIndex)  // DOTween�� �̿��� ī�� ������ ����
    {
        var targetCards = dealerCards;
        int cardIndex = dealerCards.Count - 1;
        Card targetCard = cardIndex >= 0 ? dealerCards[cardIndex] : null;
        var targetPos = cardIndex >= 0 ? dealerCardSpawnPos[cardIndex].position : new Vector3();

        if(playerIndex != GameManager.Inst.dealer)
        {
            Player nowPlayer = PlayerManager.Inst.players[playerIndex];

            targetCards = nowPlayer?.myCards;
            cardIndex = targetCards.Count - 1;
            targetCard = targetCards[cardIndex];

            if (cardIndex == 0)
                targetPos = nowPlayer.myCardLeft.position;
            else
                targetPos = nowPlayer.myCardRight.position;
        }

        targetCard.transform.DOMove(targetPos, 0.7f);
        targetCard.transform.DORotateQuaternion(Quaternion.identity, 0.7f);
        targetCard.transform.DOScale(Vector3.one * 4.5f, 0.7f);
    }

    public void RemoveDealerCard()
    {
        for (int i = 0; i < dealerCards.Count; i++)
        {
            var card = dealerCards[i];
            var cardObject = card.transform.gameObject;
            Destroy(cardObject);
        }
        dealerCards.Clear();
    }
}
