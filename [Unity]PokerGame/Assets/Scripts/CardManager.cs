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
    [SerializeField] List<Card> myCards;            // 현재 나의 카드 리스트
    [SerializeField] List<Card> dealerCards;        // 현재 딜러 카드 리스트
    [SerializeField] Transform cardSpawnPoint;      // 최초 카트 생성 위치 (시작점)
    [SerializeField] Transform[] myCardSpawnPos;    // 플레이어의 카드 위치
    [SerializeField] Transform[] dealerCardSpawnPos;// 딜러의 카드 위치

    List<Item> cardBuffer;      // 카드 덱
    List<char> cardShape;       // 0-12 는 클로버, 13-25 는 다이아, 26-38 은 하트, 39-51 은 스페이드
    List<int> cardNum;          // 0-12 는 1-13이 저장되어, 클로버의 숫자를 나타냄, 이후도 동일하게 작동

    void Start()
    {
        SetupCard();            // 게임 시작시 카드 덱 셔플
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

        for(int i = 0; i < cardBuffer.Count; i++)       // 덱 셔플 로직
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

    // Update is called once per frame
    void Update()           // 테스트용 치트
    {                       // 1은 최대 2번, 2는 최대 5번만 누를것.
        if (Input.GetKeyDown(KeyCode.Keypad1))
            AddCard(true);
        if (Input.GetKeyDown(KeyCode.Keypad2))
            AddCard(false);
    }

    void AddCard(bool isMine)   // 카드 생성 로직
    {
        // 카드 오브젝트 생성
        var cardObject = Instantiate(cardPrefabs, cardSpawnPoint.position, Quaternion.identity);
        // 카드의 Card 스크립트를 가져와 덱의 0번째 카드로 셋업 
        var card = cardObject.GetComponent<Card>();
        card.Setup(PopItem(), isMine);

        // 카드를 알맞은 리스트에 추가
        (isMine ? myCards : dealerCards).Add(card);
        int nowCardIndex = (isMine ? myCards : dealerCards).Count - 1;
        // 카드 움직임 함수 호출
        CardMoveToPos(isMine, nowCardIndex);
    }

    void CardMoveToPos(bool isMine, int cardIndex)  // DOTween을 이용한 카드 움직임 구현
    {
        var targetCards = isMine ? myCards : dealerCards;
        var targetPos = isMine ? myCardSpawnPos : dealerCardSpawnPos;
        var targetCard = targetCards[cardIndex];

        targetCard.transform.DOMove(targetPos[cardIndex].position, 0.7f);
        targetCard.transform.DORotateQuaternion(Quaternion.identity, 0.7f);
        targetCard.transform.DOScale(Vector3.one * 4.5f, 0.7f);
    }
}
