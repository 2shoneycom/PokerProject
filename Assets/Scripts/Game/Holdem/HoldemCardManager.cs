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

    const int PLAYER_CARD_NUM = 2;
    const float ICON_OFFSET = 3f;
    const float CARD_OFFSET = 4f;

    WaitForSeconds delay05 = new WaitForSeconds(0.5f);

    List<int> cardBuffer;
    List<char> cardShape;       // 0-12 : Clover , 13-25 : Diamond, 26-38 : Heart, 39-51 : Spade
    List<Sprite> cardSprites;   // same with upper
    List<int> cardNum;          // 0-12 : Clover's 1 to 13, 13-25 : Diamond's 1 to 13 ,,,,

    List<GameObject> leavePlayerCard;

    Transform cardDeckPos;
    Transform[] dealerCardPos;

    GameObject[] dealerCardList;
    int[] dealerCardDetail;

    Vector3[,] playerCardPos;

    HoldemScene _holdemScene;

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
        _holdemScene = (HoldemScene)Managers.Scene.CurrentScene;

        dealerCardPos = new Transform[DEALER_CARD_NUM];
        for(int i = 0; i < DEALER_CARD_NUM; i++)
        {
            dealerCardPos[i] = new GameObject($"Dealer Card Pos {i + 1}").GetOrAddComponent<Transform>();
            if (i == 0)
                dealerCardPos[i].position = GameObject.FindGameObjectWithTag("DealerCardPivot").transform.position;
            else
            {
                dealerCardPos[i].position = dealerCardPos[i - 1].position + new Vector3(DEALER_CARD_SPACE, 0, 0);
            }  
        }
        cardDeckPos = GameObject.FindGameObjectWithTag("Deck").transform;

        playerCardPos = new Vector3[HoldemGameControl.MAX_PLAYER_NUM, PLAYER_CARD_NUM];
        /////////////////////////////


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
            switch(cS)
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

    public void ShuffleCard()
    {
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
        if (state == 0)      // 플레이어에게 카드 배분                         로직 수정 필요//////////////////////////////////
        {
            string pUID = HoldemGameControl.Players.GetPlayerUID(toPlayer);
            yield return delay05;
            SyncSystem.Instacne.HoldemAddCard(pUID);
        }
        else                // 딜러에게 카드 배분
        {
            yield return delay05;
            SyncSystem.Instacne.HoldemDealerCard();
        }
    }

    public void AddCardToPlayerStarter(string playerUID = "")
    {
        OnAddCard?.Invoke(playerUID);
    }

    public void AddCardToDealerStarter()
    {
        OnAddCardToDealer?.Invoke();
    }

    private void AddCardOrPopCard(string playerUID = "")
    {
        int popedCard = PopCard();

        HoldemGameControl.Players.test(playerUID, HoldemGameControl.Control.StageCount == 5 ? 0 : 1, popedCard);

        if (playerUID == User.NowUser.GetNickName())
        {
            AddCardToPlayer(false, popedCard);

            Debug.Log($"case {HoldemGameControl.Control.StageCount} / stage detail {HoldemGameControl.Control.StageDetail} 종료, nextStage");
            SyncSystem.Instacne.HoldemNextStage_V2(1);
        }

        //Debug.Log($"case {HoldemGameControl.Control.StageCount} / stage detail {HoldemGameControl.Control.StageDetail} 종료, nextStage");
        //HoldemGameControl.Control.NextStage(1);
    }

    private void AddCardToDealer()
    {
        int popedCard = PopCard();

        if (!PhotonNetwork.IsMasterClient)
            return;

        AddCardToPlayer(true, popedCard);

        HoldemGameControl.Control.NextStage(1);
    }

    private void AddCardToPlayer(bool isDealer, int popedCard)         // 카드 살짝 버벅임 있음
    {
        if(cardDeckPos == null)                                     // 버그있음     왜 인진 모르겟지만 자꾸 null이 되네
            cardDeckPos = GameObject.FindGameObjectWithTag("Deck").transform;

        if (HoldemGameControl.Control.StageCount == 4 && User.NowHoldemPlayer.GetCardLen() == 1)        // 버그 처리용
            return;

        if (HoldemGameControl.Control.StageCount == 5 && User.NowHoldemPlayer.GetCardLen() == 2)        // 버그 처리용
            return;

        GameObject cardGO = Managers.Resource.PhotonInstantiate("Game/Card", cardDeckPos);

        // 카드 앞면 처리       어차피 내꺼 아니면 안보여도 됨.
        cardGO.GetComponent<SpriteRenderer>().sprite = GetRightCardImage(popedCard);

        if(isDealer == false)
        {
            User.NowHoldemPlayer.AddCardToList(cardGO, popedCard);

            CardMoveToPosPlayer();
        }
        else
        {
            int i;
            for(i = 0; i < DEALER_CARD_NUM; i++)
            {
                if (dealerCardList[i] == null)
                {
                    dealerCardList[i] = cardGO;
                    dealerCardDetail[i] = popedCard;
                    SyncSystem.Instacne.SyncHoldemDealerCard(cardGO, i, popedCard);
                    break;
                }
            }
            CardMoveToPosDealer(i);
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
        Transform targetPos = dealerCardPos[index];
        if(targetPos == null)
            targetPos = GameObject.Find($"Dealer Card Pos {index + 1}").GetOrAddComponent<Transform>();

        targetCardGO.transform.DOMove(targetPos.position, 0.7f);
        targetCardGO.transform.DORotateQuaternion(Quaternion.identity, 0.7f);
        targetCardGO.transform.DOScale(Vector3.one * 5f, 0.7f);
    }

    void CardMoveToPosPlayer()
    {
        GameObject targetCardGO = User.NowHoldemPlayer.GetLastAddedCard();
        Vector3 destPos = User.NowHoldemPlayer.GetCardPos();

        targetCardGO.transform.DOMove(destPos, 0.7f);
        targetCardGO.transform.DORotateQuaternion(Quaternion.identity, 0.7f);
        targetCardGO.transform.DOScale(Vector3.one * 3.5f, 0.7f);
    }

    public int[] GetCardDeck()
    {
        return cardBuffer.ToArray();
    }

    public void SetCardDeck(int[] cardDeck)
    {
        cardBuffer.Clear();

        for (int i = 0; i < FULL_CARD_LEN; i++)
        {
            cardBuffer.Add(cardDeck[i]);
        }

        Debug.Log("case 1 종료, nextStage");
        HoldemGameControl.Control.NextStage();
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

    Sprite GetRightCardImage(int cardIndex)
    {
        return cardSprites[cardIndex];
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
