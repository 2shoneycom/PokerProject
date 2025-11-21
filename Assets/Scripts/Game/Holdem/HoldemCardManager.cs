using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Photon.Realtime;
using Photon.Pun.Demo.PunBasics;
using UnityEditor;
using Photon.Pun;

public class HoldemCardManager
{
    const int FULL_CARD_LEN = 52;

    const int DEALER_CARD_NUM = 5;
    const float DEALER_CARD_SPACE = 6.5f;

    const float CARD_ANIMATION_TIME = 0.7f;

    public const int PLAYER_CARD_NUM = 2;
    const float ICON_OFFSET = 3f;
    const float CARD_OFFSET = 4f;

    const string MAKE_DEALER_CARD = "MAKEDEALERCARD";

    WaitForSeconds cardMoveDelay = new WaitForSeconds(0.4f);

    List<int> cardBuffer;
    List<char> cardShape;       // 0-12 : Clover , 13-25 : Diamond, 26-38 : Heart, 39-51 : Spade
    List<Sprite> cardSprites;   // same with above
    List<int> cardNum;          // 0-12 : Clover's 1 to 13, 13-25 : Diamond's 1 to 13 ,,,,

    List<GameObject> leavePlayerCard;

    Transform cardDeckPos;
    Vector3[] dealerCardPos;

    GameObject[] dealerCardList;
    int[] dealerCardDetail;

    Vector3[,] playerCardPos;
    public int CardLen { get { return _control.StageCount == 5 ? 0 : 1; } }

    UI_Holdem _holdemUI;

    HoldemGameControl _control;

    public static Action<string> OnAddCard;
    public static Action OnAddCardToDealer;

    bool isInited = false;

    public HoldemCardManager(HoldemGameControl control)
    {
        _control = control;
    }

    public void Init()
    {
        if (isInited)
            return;

        isInited = true;
        Setup();

        OnAddCard -= AddCardOrPopCard;
        OnAddCard += AddCardOrPopCard;
        OnAddCard -= Managers.Audio.PlayCardSFX;
        OnAddCard += Managers.Audio.PlayCardSFX;

        OnAddCardToDealer -= AddCardToDealer;
        OnAddCardToDealer += AddCardToDealer;
        OnAddCardToDealer -= Managers.Audio.PlayCardSFX;
        OnAddCardToDealer += Managers.Audio.PlayCardSFX;
    }

    void Setup()
    {
        _holdemUI = (UI_Holdem)Managers.UI.SceneUI;

        dealerCardPos = new Vector3[DEALER_CARD_NUM];
        for (int i = 0; i < DEALER_CARD_NUM; i++)
        {
            if (i == 0)
                dealerCardPos[i] = GameObject.FindGameObjectWithTag("DealerCardPivot").transform.position;
            else
            {
                dealerCardPos[i] = dealerCardPos[i - 1] + new Vector3(DEALER_CARD_SPACE, 0, 0);
            }
        }
        cardDeckPos = GameObject.FindGameObjectWithTag("Deck").transform;

        playerCardPos = new Vector3[HoldemGameControl.MAX_PLAYER_NUM, PLAYER_CARD_NUM];
        SetupPlayerCardPos();

        dealerCardList = new GameObject[DEALER_CARD_NUM];
        dealerCardDetail = new int[DEALER_CARD_NUM];

        cardBuffer = new List<int>(FULL_CARD_LEN);
        cardSprites = new List<Sprite>(FULL_CARD_LEN);
        cardShape = new List<char>(FULL_CARD_LEN);
        cardNum = new List<int>(FULL_CARD_LEN);
        leavePlayerCard = new List<GameObject>();

        for (int i = 0; i < FULL_CARD_LEN; i++)
        {
            int cS = i / 13;
            int cN = i % 13 + 1;
            Sprite cardSprite = null;
            switch (cS)
            {
                case 0:     // Clover
                    cardShape.Add('C');
                    cardSprite = Managers.Resource.Load<Sprite>($"Art/Cards/Clubs/{cN}_club");
                    break;
                case 1:     // Diamond
                    cardShape.Add('D');
                    cardSprite = Managers.Resource.Load<Sprite>($"Art/Cards/Diamonds/{cN}_diamond");
                    break;
                case 2:     // Heart
                    cardShape.Add('H');
                    cardSprite = Managers.Resource.Load<Sprite>($"Art/Cards/Hearts/{cN}_heart");
                    break;
                case 3:     // Spade
                    cardShape.Add('S');
                    cardSprite = Managers.Resource.Load<Sprite>($"Art/Cards/Spades/{cN}_spade");
                    break;
            }
            cardNum.Add(cN);
            cardSprites.Add(cardSprite);
        }
    }

    private void SetupPlayerCardPos()       ////////////////////////////////////////////////////
    {
        for (int i = 0; i < HoldemGameControl.MAX_PLAYER_NUM; i++)
        {
            int seatedIndex = _control.ConvertGameToUI(i);
            GameObject destGO = _holdemUI.GetPlayerGameObjcet(seatedIndex);
            RectTransform reference = destGO.GetComponent<RectTransform>();
            // 기준 RectTransform의 가로 길이
            float width = reference.rect.width;
            // 피벗 기준으로 오른쪽 끝 로컬 좌표 계산
            // (1 - pivot.x)을 곱하면 피벗 위치에서 오른쪽 끝까지 거리
            Vector3 localEdge = Vector3.zero;
            Vector3 worldPos = Vector3.zero;

            if (seatedIndex % 2 == 0 && seatedIndex != 0)
            {
                localEdge = new Vector3(-reference.pivot.x * width, 0f, 0f);
                worldPos = reference.TransformPoint(localEdge);
                worldPos.x -= ICON_OFFSET;
                playerCardPos[i, 0] = worldPos;

                worldPos.x -= CARD_OFFSET;
                playerCardPos[i, 1] = worldPos;
            }
            else
            {
                localEdge = new Vector3((1f - reference.pivot.x) * width, 0f, 0f);
                worldPos = reference.TransformPoint(localEdge);
                worldPos.x += ICON_OFFSET;
                playerCardPos[i, 0] = worldPos;

                worldPos.x += CARD_OFFSET;
                playerCardPos[i, 1] = worldPos;
            }
        }
    }

    public void ShuffleCard()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemCardManager.cs 파일의 ShuffleCard 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        cardBuffer.Clear();

        for (int i = 0; i < FULL_CARD_LEN; i++)
        {
            cardBuffer.Add(i);
        }

        for (int i = 0; i < FULL_CARD_LEN; i++)
        {
            int rand = UnityEngine.Random.Range(i, cardBuffer.Count);
            int temp = cardBuffer[i];
            cardBuffer[i] = cardBuffer[rand];
            cardBuffer[rand] = temp;
        }
    }

    public int PopCard()
    {
        int nowCard = cardBuffer[0];
        cardBuffer.RemoveAt(0);
        return nowCard;
    }

    public IEnumerator DealingCard(int state, int toPlayer = -1)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemCardManager.cs 파일의 DealingCard 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        yield return cardMoveDelay;

        if (state == 0)      // 플레이어에게 카드 배분                         로직 수정 필요//////////////////////////////////
        {
            string pUID = _control.Players.GetPlayerUID(toPlayer);

            _control.Sync.HoldemAddCard(pUID);
        }
        else                // 딜러에게 카드 배분
        {
            _control.Sync.HoldemDealerCard();
        }
    }

    public void AddCardToPlayerStarter(string playerUID = "")
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemCardManager.cs 파일의 AddCardToPlayerStarter 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        OnAddCard?.Invoke(playerUID);
    }

    public void AddCardToDealerStarter()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemCardManager.cs 파일의 AddCardToDealerStarter 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        OnAddCardToDealer?.Invoke();
    }

    private void AddCardOrPopCard(string playerUID = "")
    {
        int popedCard = PopCard();

        if (!PhotonNetwork.IsMasterClient)
            return;

        AddCardToPlayer(popedCard, playerUID);
        _control.Sync.HoldemNextStage_V2(1);
    }

    private void AddCardToDealer()
    {
        int popedCard = PopCard();

        if (!PhotonNetwork.IsMasterClient)
            return;

        AddCardToPlayer(popedCard);
        _control.Sync.HoldemNextStage_V2(1);
    }

    private void AddCardToPlayer(int popedCard, string pUID = MAKE_DEALER_CARD)         // 카드 살짝 버벅임 있음
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemCardManager.cs 파일의 AddCardToPlayer 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        if (cardDeckPos == null)                                     // 버그있음     왜 인진 모르겟지만 자꾸 null이 되네
            cardDeckPos = GameObject.FindGameObjectWithTag("Deck").transform;

        GameObject cardGO = Managers.Resource.PhotonInstantiate("Game/Card", cardDeckPos);
        cardGO.GetComponent<PhotonView>().OwnershipTransfer = OwnershipOption.Takeover;

        // 카드 동기화 처리
        if (pUID == MAKE_DEALER_CARD)       // 딜러인 경우
        {
            int i;
            for (i = 0; i < DEALER_CARD_NUM; i++)
            {
                if (dealerCardList[i] == null)
                {
                    _control.Sync.SyncHoldemDealerCard(cardGO, i, popedCard);
                    break;
                }
            }
            CardMoveToPosDealer(i);
        }
        else                                // 플레이어인 경우
        {
            _control.Sync.SyncHoldemPlayerCard(pUID, cardGO.GetComponent<PhotonView>().ViewID, popedCard);

            CardMoveToPosPlayer(cardGO, pUID);
        }
    }

    public void DealerCardSetting(int viewID, int index, int cardDetail)    // 딜러 카드 오브젝트 저장 및 앞면 보이게 설정
    {
        dealerCardList[index] = PhotonView.Find(viewID).gameObject;
        dealerCardDetail[index] = cardDetail;
        dealerCardList[index].GetComponent<SpriteRenderer>().sprite = GetRightCardImage(cardDetail);
    }

    void CardMoveToPosDealer(int index)
    {
        GameObject targetCardGO = dealerCardList[index];
        Vector3 targetPos = dealerCardPos[index];
        //if (targetPos == null)
        //    targetPos = GameObject.Find($"Dealer Card Pos {index + 1}").GetOrAddComponent<Transform>();

        targetCardGO.transform.DOMove(targetPos, CARD_ANIMATION_TIME);
        targetCardGO.transform.DORotateQuaternion(Quaternion.identity, CARD_ANIMATION_TIME);
        targetCardGO.transform.DOScale(Vector3.one * 5f, CARD_ANIMATION_TIME);
    }

    void CardMoveToPosPlayer(GameObject cardGO, string pUID)
    {
        Vector3 destPos = playerCardPos[_control.Players.GetPlayerGameIndexByUID(pUID), CardLen];

        cardGO.transform.DOMove(destPos, CARD_ANIMATION_TIME);
        cardGO.transform.DORotateQuaternion(Quaternion.identity, CARD_ANIMATION_TIME);
        cardGO.transform.DOScale(Vector3.one * 3.5f, CARD_ANIMATION_TIME);
    }

    public int[] GetCardDeck()
    {
        return cardBuffer.ToArray();
    }

    public void SetCardDeck(int[] cardDeck)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemCardManager.cs 파일의 SetCardDeck 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        cardBuffer.Clear();

        for (int i = 0; i < FULL_CARD_LEN; i++)
        {
            cardBuffer.Add(cardDeck[i]);
        }

        _control.NextStage();
    }

    public int[] GetDealerCardDetail()
    {
        return dealerCardDetail;
    }

    public int GetCardNum(int index)
    {
        return cardNum[index];
    }

    public char GetCardShape(int index)
    {
        return cardShape[index];
    }

    public Sprite GetRightCardImage(int cardIndex)
    {
        return cardSprites[cardIndex];
    }

    public void ClearDealerCard()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemCardManager.cs 파일의 ClearDealerCard 함수 실행"); // 디버깅 추적용 (25.11.12 승헌

        for (int i = 0; i < DEALER_CARD_NUM; i++)
        {
            if (PhotonNetwork.IsMasterClient)
                Managers.Resource.PhotonDestroy(dealerCardList[i]);

            dealerCardList[i] = null;
        }
        leavePlayerCard.Clear();
    }

    public void GiveHoldemCardManagerSyncData(Player newPlayer)
    {

    }
}
