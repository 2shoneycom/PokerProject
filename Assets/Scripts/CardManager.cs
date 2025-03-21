using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] ItemSO itemSO;
    [SerializeField] GameObject cardPrefabs;        // 생성할 카드 프리펩
    [SerializeField] public List<Card> dealerCards;        // 현재 딜러 카드 리스트
    [SerializeField] Transform cardSpawnPoint;      // 최초 카드 생성 위치 (시작점)
    [SerializeField] Transform[] dealerCardSpawnPos;// 딜러의 카드 위치

    List<Item> cardBuffer;      // 카드 덱
    public List<char> cardShape;       // 0-12 는 클로버, 13-25 는 다이아, 26-38 은 하트, 39-51 은 스페이드
    public List<int> cardNum;          // 0-12 는 1-13이 저장되어, 클로버의 숫자를 나타냄, 이후도 동일하게 작동

    void Start()
    {
        SetupCard();            // 게임 시작시 카드 덱 셔플
        ShuffleCard();
        TurnManager.OnAddCard += AddCard;   // (승헌) 이 코드가 CardManager에 있어야 하는 이유는?
                                            // OnAddCard라는 변수자체는 TurnManager에 선언되어있음
                                            // 근데 OnAddCard에 어떤 메소드가 연결되는지는 여기에 있음
                                            // 그래서 난 처음에 OnAddCard가 어디서 연결되는 건지 찾기 어려웠는데
                                            // 꼭 여기에 있어야 하는 이유가 따로 있는 건가
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

        for (int i = 0; i < cardBuffer.Count; i++)       // 덱 셔플 로직
        {
            int rand = Random.Range(i, cardBuffer.Count);
            Item temp = cardBuffer[i];
            cardBuffer[i] = cardBuffer[rand];
            cardBuffer[rand] = temp;
        }
    }

    public Item PopItem()           // 0번째 카드덱 pop
    {
        Item item = cardBuffer[0];
        cardBuffer.RemoveAt(0);
        return item;
    }

    void AddCard(int playerIndex)   // 카드 생성 로직
    {
        // 카드 오브젝트 생성
        var cardObject = Instantiate(cardPrefabs, cardSpawnPoint.position, Quaternion.identity);
        // 카드의 Card 스크립트를 가져와 덱의 0번째 카드로 셋업
        var card = cardObject.GetComponent<Card>();
        card.Setup(PopItem(), playerIndex);

        // 카드를 알맞은 리스트에 추가
        if(playerIndex == GameManager.Inst.dealer)
            dealerCards.Add(card);
        else
        {
            Player nowPlayer = PlayerManager.Inst.players[playerIndex];
            nowPlayer?.myCards.Add(card);
        }
        // 카드 움직임 함수 호출
        CardMoveToPos(playerIndex);
    }

    void CardMoveToPos(int playerIndex)  // DOTween을 이용한 카드 움직임 구현
    {
        // 딜러한테 줄 때
        var targetCards = dealerCards;
        int cardIndex = dealerCards.Count - 1;  // 주려는 카드가 몇 번째 카드인지 (0~4 번째)
        Card targetCard = cardIndex >= 0 ? dealerCards[cardIndex] : null;
        var targetPos = cardIndex >= 0 ? dealerCardSpawnPos[cardIndex].position : new Vector3();    // 몇 번째 카드인지에 따라 tragetPos가 달라짐

        // 플레이어한테 줄 때
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
