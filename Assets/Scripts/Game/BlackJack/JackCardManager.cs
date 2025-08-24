using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using UnityEditor;
using UnityEngine;

public class JackCardManager
{
    const int FULL_CARD_LEN = 52;
    const int FULL_CARD_DECK_LEN = 52 * 5;

    const float CARD_ANIMATION_TIME = 0.5f;

    public const int PLAYER_CARD_NUM = 10;
    public const int DEALER_CARD_NUM = 5;
    const float CARD_OFFSET = 50f;

    const string MAKE_DEALER_CARD = "MAKEDEALERCARD";
    const string CARD_PREFAB_PATH = "UI/SubItem/UI_Card";

    WaitForSeconds cardMoveDelay = new WaitForSeconds(0.5f);

    List<int> cardBuffer;
    List<char> cardShape;       // 0-12 : Clover , 13-25 : Diamond, 26-38 : Heart, 39-51 : Spade
    List<Sprite> cardSprites;   // same with above
    List<int> cardNum;          // 0-12 : Clover's 1 to 13, 13-25 : Diamond's 1 to 13 ,,,,

    List<GameObject> leavePlayerCard;

    Transform cardDeckPos;

    Vector3[] dealerCardPos;
    Vector3[,] playerCardPos;

    GameObject[] dealerCardList;
    int[] dealerCardDetail;
    Tuple<int, int> dealerCardScore;

    UI_BlackJack _jackUI;

    public static Action<string> OnAddCard;
    public static Action OnAddCardToDealer;

    bool isInited = false;

    public void Init()
    {
        if (isInited)
            return;

        isInited = true;
        Setup();

        OnAddCard -= AddCardOrPopCard;
        OnAddCard += AddCardOrPopCard;

        OnAddCardToDealer -= AddCardToDealer;
        OnAddCardToDealer += AddCardToDealer;
    }

    void Setup()
    {
        _jackUI = (UI_BlackJack)Managers.UI.SceneUI;

        cardDeckPos = GameObject.FindGameObjectWithTag("Deck").transform;
        dealerCardPos = new Vector3[DEALER_CARD_NUM];
        dealerCardList = new GameObject[DEALER_CARD_NUM];
        dealerCardDetail = new int[DEALER_CARD_NUM];
        dealerCardScore = Tuple.Create(-1, -1);
        SetupDealerCardPos();

        playerCardPos = new Vector3[JackGameControl.MAX_PLAYER_NUM, PLAYER_CARD_NUM];
        SetupPlayerCardPos();

        cardBuffer = new List<int>(FULL_CARD_DECK_LEN);
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

    private void SetupDealerCardPos()
    {
        for(int i = 0; i < DEALER_CARD_NUM; i++)
        {
            dealerCardList[i] = null;
            if (i == 0)
            {
                dealerCardPos[i] = cardDeckPos.position;
                dealerCardPos[i].y -= 75f;
            }
            else
            {
                dealerCardPos[i] = dealerCardPos[i - 1];
                dealerCardPos[i].x += CARD_OFFSET;
            }
        }
    }

    private void SetupPlayerCardPos()
    {
        for (int i = 0; i < JackGameControl.MAX_PLAYER_NUM; i++)
        {
            int seatedIndex = i;
            GameObject destGO = _jackUI.GetPlayerGameObjcet(seatedIndex);
            RectTransform reference = destGO.GetComponent<RectTransform>();

            Vector3 worldPos = reference.position;
            playerCardPos[i, 0] = worldPos;

            for (int j = 1; j < PLAYER_CARD_NUM; j++)
            {
                worldPos.x += CARD_OFFSET;
                playerCardPos[i, j] = worldPos;
            }
        }
    }

    public void ShuffleCard()
    {
        cardBuffer.Clear();

        for (int i = 0; i < FULL_CARD_DECK_LEN; i++)
        {
            int num = i % FULL_CARD_LEN;
            cardBuffer.Add(num);
        }

        for (int i = 0; i < FULL_CARD_DECK_LEN; i++)
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

        if(cardBuffer.Count == 0)
        {
            ShuffleCard();
            JackGameControl.Control.RequestDeckShuffle();
        }

        return nowCard;
    }

    public int[] GetCardDeck()
    {
        return cardBuffer.ToArray();
    }

    public void SetCardDeck(int[] cardDeck)
    {
        cardBuffer.Clear();

        for (int i = 0; i < FULL_CARD_DECK_LEN; i++)
        {
            cardBuffer.Add(cardDeck[i]);
        }

        JackGameControl.Control.NextStage();
    }

    public IEnumerator DealingCard(int toPlayer = -1)
    {
        yield return cardMoveDelay;
        Debug.Log("1");

        if (toPlayer == -1)
        {
            SyncSystem.Sync.JackDealerCard();
        }
        else
        {
            Debug.Log("2");

            string pUID = JackGameControl.Players.GetPlayerUID(toPlayer);
            SyncSystem.Sync.JackAddCard(pUID);
        }
    }

    public void AddCardToPlayerStarter(string playerUID = "")
    {
        Debug.Log("3");

        OnAddCard?.Invoke(playerUID);
    }

    public void AddCardToDealerStarter()
    {
        OnAddCardToDealer?.Invoke();
    }

    private void AddCardOrPopCard(string playerUID = "")
    {
        int popedCard = PopCard();

        if (!PhotonNetwork.IsMasterClient)
            return;

        AddCardToPlayer(popedCard, playerUID);
        //SyncSystem.Sync.JackNextStage_V2(1);
    }

    private void AddCardToDealer()
    {
        int popedCard = PopCard();

        if (!PhotonNetwork.IsMasterClient)
            return;

        AddCardToPlayer(popedCard);
        //SyncSystem.Sync.JackNextStage_V2(1);
    }

    public void SetDealerCard(int cardViewID, int index, int cardDetail)    // 딜러 카드 오브젝트 저장 및 앞면 보이게 설정
    {
        GameObject cardGO = PhotonView.Find(cardViewID).gameObject;
        cardGO.transform.SetParent(cardDeckPos);
        UI_Card card = cardGO.GetOrAddComponent<UI_Card>();

        if (PhotonNetwork.IsMasterClient)
        {
            card.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            CardMoveToPosDealer(cardGO, index);
        }

        dealerCardList[index] = cardGO;
        dealerCardDetail[index] = cardDetail;
        card.SetCardImage(cardDetail);
        CalculateDealerCardScore();

        JackGameControl.Control.NextStage();
    }

    void CalculateDealerCardScore()
    {
        int[] score = new int[22];
        if (dealerCardScore.Item1 == -1)
        {
            score[0] = 1;
        }
        else
        {
            score[dealerCardScore.Item1] = 1;

            if (dealerCardScore.Item2 != -1)
                score[dealerCardScore.Item2] = 1;
        }

        int i = -1;

        for (int ii = 0; ii < dealerCardList.Length; ii++)
        {
            if (dealerCardList[ii] == null)
                break;

            i = ii;
        }

        int cardscore = GetCardNum(dealerCardDetail[i]);
        if (cardscore >= 10)
            cardscore = 10;

        int[] tmp = new int[22];
        for (int j = 0; j < 22; j++)
        {
            if (score[j] == 1)
            {
                int s = j + cardscore;

                if (s <= 21 && tmp[s] == 0)
                    tmp[s] = 1;

                if (cardscore == 1)
                {
                    s = j + 11;

                    if (s <= 21 && tmp[s] == 0)
                        tmp[s] = 1;
                }
            }
        }
        score = tmp;

        int a = -1;
        int b = -1;

        for (int j = 0; j < 22; j++)
        {
            if (score[j] == 1)
            {
                if (a == -1)
                    a = j;
                else
                    b = j;
            }
        }

        dealerCardScore = Tuple.Create(a, b);
    }

    public Tuple<int,int> GetDealerCardScore()
    {
        return dealerCardScore;
    }

    void CardMoveToPosDealer(GameObject cardGO, int index)
    {
        Vector3 targetPos = dealerCardPos[index];
        //if (targetPos == null)
        //    targetPos = GameObject.Find($"Dealer Card Pos {index + 1}").GetOrAddComponent<Transform>();

        cardGO.transform.DOMove(targetPos, CARD_ANIMATION_TIME);
        cardGO.transform.DORotateQuaternion(Quaternion.identity, CARD_ANIMATION_TIME);
        //cardGO.transform.DOScale(Vector3.one * 5f, CARD_ANIMATION_TIME);
    }

    private void AddCardToPlayer(int popedCard, string pUID = MAKE_DEALER_CARD)
    {
        if (cardDeckPos == null)
            cardDeckPos = GameObject.FindGameObjectWithTag("Deck").transform;

        GameObject cardGO = Managers.Resource.PhotonInstantiate(CARD_PREFAB_PATH, cardDeckPos);
        cardGO.GetOrAddComponent<UI_Card>();
        cardGO.GetComponent<PhotonView>().OwnershipTransfer = OwnershipOption.Takeover;

        if(pUID == MAKE_DEALER_CARD)
        {
            int cardIndex = GetDealerCardLen();
            SyncSystem.Sync.SyncJackDealerCard(cardGO, cardIndex, popedCard);
        }
        else
        {
            SyncSystem.Sync.SyncJackPlayerCard(pUID, cardGO, popedCard);
        }
    }

    public void SetPlayerCard(string pUID, int cardViewID, int cardDetail)
    {
        GameObject cardGO = PhotonView.Find(cardViewID).gameObject;
        cardGO.transform.SetParent(cardDeckPos);

        if (PhotonNetwork.IsMasterClient)
        {
            UI_Card card = cardGO.GetOrAddComponent<UI_Card>();

            card.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            CardMoveToPosPlayer(cardGO, pUID);
        }

        JackGameControl.Players.SetPlayerCard(pUID, cardViewID, cardDetail);
    }

    void CardMoveToPosPlayer(GameObject cardGO, string pUID)
    {
        int playerIndex = JackGameControl.Players.GetPlayerGameIndexByUID(pUID);
        int cardIndex = JackGameControl.Players.GetPlayerCardLen(playerIndex);
        Vector3 destPos = playerCardPos[playerIndex, cardIndex];

        cardGO.transform.DOMove(destPos, CARD_ANIMATION_TIME);
        cardGO.transform.DORotateQuaternion(Quaternion.identity, CARD_ANIMATION_TIME);
        //cardGO.transform.DOScale(Vector3.one * 3.5f, CARD_ANIMATION_TIME);
    }

    public Sprite GetRightCardImage(int cardIndex)
    {
        return cardSprites[cardIndex];
    }

    public int GetCardNum(int cardIndex)
    {
        return cardNum[cardIndex];
    }

    public int GetDealerCardDetail(int cardIndex)
    {
        return dealerCardDetail[cardIndex];
    }

    public int GetDealerCardLen()
    {
        for(int i = 0; i < dealerCardList.Length; i++)
        {
            if (dealerCardList[i] == null)
                return i;
        }
        return -1;
    }

    public void ClearDealerCard()
    {
        for (int i = 0; i < DEALER_CARD_NUM; i++)
        {
            if (PhotonNetwork.IsMasterClient)
                Managers.Resource.PhotonDestroy(dealerCardList[i]);

            dealerCardList[i] = null;
        }
        leavePlayerCard.Clear();
    }
}
