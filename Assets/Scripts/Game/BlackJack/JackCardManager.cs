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

    public const int DEALER_CARD_NUM = 5;
    const float CARD_X_OFFSET = 50f;
    const float CARD_Y_OFFSET = 150f;

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
    GameObject[] dealerCardList;
    int[] dealerCardDetail;
    Tuple<int, int> dealerCardScore;

    UI_BlackJack _jackUI;
    JackGameControl _control;

    public static Action<string> OnAddCard;
    public static Action OnAddCardToDealer;

    bool isInited = false;
    bool isShuffled = false;
    bool isBurst = false;
    int curPlayerSplitNum = -1;

    public JackCardManager(JackGameControl control)
    {
        _control = control;
    }

    public void Init()
    {
        isBurst = false;
        dealerCardScore = Tuple.Create(-1, -1);

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
        _jackUI = (UI_BlackJack)Managers.UI.SceneUI;

        cardDeckPos = GameObject.FindGameObjectWithTag("Deck").transform;
        dealerCardPos = new Vector3[DEALER_CARD_NUM];
        dealerCardList = new GameObject[DEALER_CARD_NUM];
        dealerCardDetail = new int[DEALER_CARD_NUM];
        SetupDealerCardPos();

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
        for (int i = 0; i < DEALER_CARD_NUM; i++)
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
                dealerCardPos[i].x += CARD_X_OFFSET;
            }
        }
    }

    public Vector3 GetPlayerCardPos(int playerIndex, int splitNum, int cardIndex)
    {
        GameObject destGO = _jackUI.GetPlayerGameObjcet(playerIndex);
        RectTransform reference = destGO.GetComponent<RectTransform>();

        Vector3 worldPos = reference.position;
        worldPos.x += CARD_X_OFFSET * cardIndex;
        worldPos.y += CARD_Y_OFFSET * splitNum;

        return worldPos;
    }

    public void ShuffleCard()
    {
        if (isShuffled) return;
        isShuffled = true;

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

        if (cardBuffer.Count == 0)
        {
            ShuffleCard();
            _control.RequestDeckShuffle();
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

        for (int i = 0; i < cardDeck.Length; i++)
        {
            cardBuffer.Add(cardDeck[i]);
        }

        _control.NextStage();
    }

    public IEnumerator DealingCard(int toPlayer = -1, int splitNum = -1)
    {
        yield return cardMoveDelay;
        Debug.Log("1");

        if (toPlayer == -1)
        {
            _control.Sync.JackDealerCard();
        }
        else
        {
            Debug.Log("2");

            string pUID = _control.Players.GetPlayerUID(toPlayer);
            _control.Sync.JackAddCard(pUID, splitNum);
        }
    }

    public void AddCardToPlayerStarter(string playerUID, int splitNum)
    {
        Debug.Log("3");

        curPlayerSplitNum = splitNum;
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
    }

    private void AddCardToDealer()
    {
        int popedCard = PopCard();

        if (!PhotonNetwork.IsMasterClient)
            return;

        AddCardToPlayer(popedCard);
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

        if(index != 1)
            card.SetCardImage(cardDetail, _control);
        CalculateDealerCardScore(cardDetail);

        if (_control.StageCount < 10)
            _control.NextStage();
        else
            _control.NextStage(1);
    }

    public void SetDealerCardOpen()
    {
        UI_Card card = dealerCardList[1].GetOrAddComponent<UI_Card>();
        int cardDetail = dealerCardDetail[1];
        card.SetCardImage(cardDetail, _control);
    }

    void CalculateDealerCardScore(int cardDetail)
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

        int cardscore = GetCardNum(cardDetail);
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

        if(_control.StageCount > 10)
        {
            if (dealerCardScore.Item1 == -1 && dealerCardScore.Item2 == -1)
            {
                isBurst = true;
                _jackUI.UpdateDealerStatusText("Bust...");

                foreach (GameObject cardGO in dealerCardList)
                {
                    if (cardGO != null)
                    {
                        UI_Card card = cardGO.GetOrAddComponent<UI_Card>();
                        card.UILoseBlockSwitch(true);
                    }
                }
                return;
            }
            UpdateDealerScoreText();
        }
    }

    public void UpdateDealerScoreText()
    {
        string text = "";

        text += dealerCardScore.Item1.ToString();
        if (dealerCardScore.Item2 != -1)
        {
            text += "/";
            text += dealerCardScore.Item2.ToString();
        }
        _jackUI.UpdateDealerStatusText(text);
    }

    public bool GetDealerIsBurst()
    {
        return isBurst;
    }

    public Tuple<int, int> GetDealerCardScore()
    {
        return dealerCardScore;
    }

    void CardMoveToPosDealer(GameObject cardGO, int index)
    {
        Vector3 targetPos = dealerCardPos[index];

        cardGO.transform.DOMove(targetPos, CARD_ANIMATION_TIME);
        cardGO.transform.DORotateQuaternion(Quaternion.identity, CARD_ANIMATION_TIME);
    }

    private void AddCardToPlayer(int popedCard, string pUID = MAKE_DEALER_CARD)
    {
        if (cardDeckPos == null)
            cardDeckPos = GameObject.FindGameObjectWithTag("Deck").transform;

        GameObject cardGO = Managers.Resource.PhotonInstantiate(CARD_PREFAB_PATH, cardDeckPos);
        cardGO.GetOrAddComponent<UI_Card>();
        cardGO.GetComponent<PhotonView>().OwnershipTransfer = OwnershipOption.Takeover;

        if (pUID == MAKE_DEALER_CARD)
        {
            int cardIndex = GetDealerCardLen();
            _control.Sync.SyncJackDealerCard(cardGO, cardIndex, popedCard);
        }
        else
        {
            _control.Sync.SyncJackPlayerCard(pUID, cardGO, popedCard);
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

        _control.Players.SetPlayerCard(pUID, curPlayerSplitNum, cardViewID, cardDetail);
    }

    void CardMoveToPosPlayer(GameObject cardGO, string pUID)
    {
        int playerIndex = _control.Players.GetPlayerGameIndexByUID(pUID);
        int cardIndex = _control.Players.GetPlayerCardLen(playerIndex, curPlayerSplitNum);
        Vector3 destPos = GetPlayerCardPos(playerIndex, curPlayerSplitNum, cardIndex);

        cardGO.transform.DOMove(destPos, CARD_ANIMATION_TIME);
        cardGO.transform.DORotateQuaternion(Quaternion.identity, CARD_ANIMATION_TIME);
    }

    public void SplittedCardMove(int playerIndex, int splitNum, GameObject cardGO)
    {
        Vector3 destPos = GetPlayerCardPos(playerIndex, splitNum, 0);
        cardGO.transform.DOMove(destPos, CARD_ANIMATION_TIME);
        cardGO.transform.DORotateQuaternion(Quaternion.identity, CARD_ANIMATION_TIME);
    }

    public void CurTurnPlayerCardBigger(int playerIndex, int splitNum, int cardIndex = -1)
    {
        if (cardIndex != -1)
        {
            GameObject cardGO = _control.Players.GetPlayerCardGO(playerIndex, splitNum, cardIndex);
            cardGO.transform.DOScale(Vector3.one * 1.3f, CARD_ANIMATION_TIME);
            return;
        }

        for (int i = 0; i < _control.Players.GetPlayerCardLen(playerIndex, splitNum); i++)
        {
            GameObject cardGO = _control.Players.GetPlayerCardGO(playerIndex, splitNum, i);
            cardGO.transform.DOScale(Vector3.one * 1.3f, CARD_ANIMATION_TIME);
        }
    }

    public void CurTurnPlayerCardBigger(GameObject cardGO)
    {
        cardGO.transform.DOScale(Vector3.one * 1.3f, CARD_ANIMATION_TIME);
    }

    public void CurTurnPlayerCardOrigin(int playerIndex, int splitNum, int cardIndex = -1)
    {
        if (cardIndex != -1)
        {
            GameObject cardGO = _control.Players.GetPlayerCardGO(playerIndex, splitNum, cardIndex);
            cardGO.transform.DOScale(Vector3.one, CARD_ANIMATION_TIME);
            return;
        }

        for (int i = 0; i < _control.Players.GetPlayerCardLen(playerIndex, splitNum); i++)
        {
            GameObject cardGO = _control.Players.GetPlayerCardGO(playerIndex, splitNum, i);
            cardGO.transform.DOScale(Vector3.one, CARD_ANIMATION_TIME);
        }
    }

    public void CurTurnPlayerCardOrigin(GameObject cardGO)
    {
        cardGO.transform.DOScale(Vector3.one, CARD_ANIMATION_TIME);
    }

    public void CardScaleBigger(GameObject cardGO)
    {
        cardGO.transform.DOScale(Vector3.one * 1.7f, CARD_ANIMATION_TIME);
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
        for (int i = 0; i < dealerCardList.Length; i++)
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
