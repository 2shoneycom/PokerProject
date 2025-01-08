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
    [SerializeField] List<Card> myCards;            // ���� ���� ī�� ����Ʈ
    [SerializeField] List<Card> dealerCards;        // ���� ���� ī�� ����Ʈ
    [SerializeField] Transform cardSpawnPoint;      // ���� īƮ ���� ��ġ (������)
    [SerializeField] Transform[] myCardSpawnPos;    // �÷��̾��� ī�� ��ġ
    [SerializeField] Transform[] dealerCardSpawnPos;// ������ ī�� ��ġ

    List<Item> cardBuffer;      // ī�� ��
    List<char> cardShape;       // 0-12 �� Ŭ�ι�, 13-25 �� ���̾�, 26-38 �� ��Ʈ, 39-51 �� �����̵�
    List<int> cardNum;          // 0-12 �� 1-13�� ����Ǿ�, Ŭ�ι��� ���ڸ� ��Ÿ��, ���ĵ� �����ϰ� �۵�

    void Start()
    {
        SetupCard();            // ���� ���۽� ī�� �� ����
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
            cardNum.Add((i + 1) % 14);
            cardBuffer.Add(itemSO.items[i]);
        }

        for(int i = 0; i < cardBuffer.Count; i++)       // �� ���� ����
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
    void Update()           // �׽�Ʈ�� ġƮ
    {                       // 1�� �ִ� 2��, 2�� �ִ� 5���� ������.
        if (Input.GetKeyDown(KeyCode.Keypad1))
            AddCard(true);
        if (Input.GetKeyDown(KeyCode.Keypad2))
            AddCard(false);
    }

    void AddCard(bool isMine)   // ī�� ���� ����
    {
        // ī�� ������Ʈ ����
        var cardObject = Instantiate(cardPrefabs, cardSpawnPoint.position, Quaternion.identity);
        // ī���� Card ��ũ��Ʈ�� ������ ���� 0��° ī��� �¾� 
        var card = cardObject.GetComponent<Card>();
        card.Setup(PopItem(), isMine);

        // ī�带 �˸��� ����Ʈ�� �߰�
        (isMine ? myCards : dealerCards).Add(card);
        int nowCardIndex = (isMine ? myCards : dealerCards).Count - 1;
        // ī�� ������ �Լ� ȣ��
        CardMoveToPos(isMine, nowCardIndex);
    }

    void CardMoveToPos(bool isMine, int cardIndex)  // DOTween�� �̿��� ī�� ������ ����
    {
        var targetCards = isMine ? myCards : dealerCards;
        var targetPos = isMine ? myCardSpawnPos : dealerCardSpawnPos;
        var targetCard = targetCards[cardIndex];

        targetCard.transform.DOMove(targetPos[cardIndex].position, 0.7f);
        targetCard.transform.DORotateQuaternion(Quaternion.identity, 0.7f);
        targetCard.transform.DOScale(Vector3.one * 4.5f, 0.7f);
    }
}
