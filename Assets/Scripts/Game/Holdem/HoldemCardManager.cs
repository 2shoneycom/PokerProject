using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class HoldemCardManager : MonoBehaviour
{
    private static HoldemCardManager instance;
    public static HoldemCardManager Instacne
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }
    }

    const int FULL_CARD_LEN = 52;
    const int DEALER_CARD_NUM = 5;
    const float DEALER_CARD_SPACE = 6.5f;
    const float ICON_OFFSET = 3f;
    const float CARD_OFFSET = 4f;

    List<int> cardBuffer;
    List<char> cardShape;       // 0-12 : Clover , 13-25 : Diamond, 26-38 : Heart, 39-51 : Spade
    List<Sprite> cardSprites;   // same with upper
    List<int> cardNum;          // 0-12 : Clover's 1 to 13, 13-25 : Diamond's 1 to 13 ,,,,

    Transform cardDeckPos;
    Transform[] dealerCardPos;

    GameObject[] dealerCardList;

    HoldemScene _holdemScene;

    void Start()
    {
        Setup();
        ShuffleCard();
    }

    void Setup()
    {
        _holdemScene = (HoldemScene)Managers.Scene.CurrentScene;

        cardBuffer = new List<int>(FULL_CARD_LEN);
        cardSprites = new List<Sprite>(FULL_CARD_LEN);
        cardShape = new List<char>(FULL_CARD_LEN);
        cardNum = new List<int>(FULL_CARD_LEN);

        dealerCardPos = new Transform[DEALER_CARD_NUM];
        for(int i = 0; i < DEALER_CARD_NUM; i++)
        {
            dealerCardPos[i] = new GameObject($"Dealer Card Pos {i + 1}").transform;
            if (i == 0)
                dealerCardPos[i].position = GameObject.FindGameObjectWithTag("DealerCardPivot").transform.position;
            else
            {
                dealerCardPos[i].position = dealerCardPos[i - 1].position + new Vector3(DEALER_CARD_SPACE, 0, 0);
            }  
        }
        cardDeckPos = GameObject.FindGameObjectWithTag("Deck").transform;

        dealerCardList = new GameObject[DEALER_CARD_NUM];

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
            cardBuffer.Add(i);
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

    public void AddCardToPlayer(int playerIndex, bool isDealer = false)
    {
        GameObject cardGO = Managers.Resource.PhotonInstantiate("Game/Card", transform);
        cardGO.transform.position = cardDeckPos.position;
        int popedCard = PopCard();

        // 카드 앞면 처리
        cardGO.GetComponent<SpriteRenderer>().sprite = cardSprites[popedCard];

        if(isDealer == false)
        {
            _holdemScene.Players[playerIndex].AddCardToList(cardGO);

            CardMoveToPosPlayer(playerIndex);
        }
        else
        {
            int i;
            for(i = 0; i < DEALER_CARD_NUM; i++)
            {
                if (dealerCardList[i] == null)
                {
                    dealerCardList[i] = cardGO;
                    break;
                }
            }
            CardMoveToPosDealer(i);
        }
    }

    void CardMoveToPosDealer(int index)
    {
        GameObject targetCardGO = dealerCardList[index];
        Transform targetPos = dealerCardPos[index];

        targetCardGO.transform.DOMove(targetPos.position, 0.7f);
        targetCardGO.transform.DORotateQuaternion(Quaternion.identity, 0.7f);
        targetCardGO.transform.DOScale(Vector3.one * 5f, 0.7f);
    }


    void CardMoveToPosPlayer(int playerIndex)
    {
        GameObject targetCardGO = _holdemScene.Players[playerIndex].GetLastAddedCard();
        Vector3 destPos = CalCardPos(playerIndex);

        targetCardGO.transform.DOMove(destPos, 0.7f);
        targetCardGO.transform.DORotateQuaternion(Quaternion.identity, 0.7f);
        targetCardGO.transform.DOScale(Vector3.one * 3.5f, 0.7f);
    }

    Vector3 CalCardPos(int playerIndex)
    {
        UI_Holdem holdemUI = (UI_Holdem)Managers.UI.SceneUI;
        GameObject destGO = holdemUI.GetPlayerGameObjcet(playerIndex);

        RectTransform reference = destGO.GetComponent<RectTransform>();
        // 기준 RectTransform의 가로 길이
        float width = reference.rect.width;
        // 피벗 기준으로 오른쪽 끝 로컬 좌표 계산
        // (1 - pivot.x)을 곱하면 피벗 위치에서 오른쪽 끝까지 거리
        Vector3 localEdge = Vector3.zero;
        Vector3 worldPos = Vector3.zero;

        if (playerIndex % 2 == 0 && playerIndex != 0) 
        {
            localEdge = new Vector3(-reference.pivot.x * width, 0f, 0f);
            worldPos = reference.TransformPoint(localEdge);
            worldPos.x -= ICON_OFFSET;

            if (_holdemScene.Players[playerIndex].GetPosIndex() == 1)
                worldPos.x -= CARD_OFFSET;
        }
        else
        {
            localEdge = new Vector3((1f - reference.pivot.x) * width, 0f, 0f);
            worldPos = reference.TransformPoint(localEdge);
            worldPos.x += ICON_OFFSET;

            if (_holdemScene.Players[playerIndex].GetPosIndex() == 1)
                worldPos.x += CARD_OFFSET;
        }
        return worldPos;
    }

    Sprite GetRightCardImage(int cardIndex)
    {
        return cardSprites[cardIndex];
    }

}
