using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Photon.Pun.Demo.PunBasics;
using Photon.Pun;
using Photon.Realtime;


public class PokerCardManager
{
    const int FULL_CARD_LEN = 52;

    const float CARD_ANIMATION_TIME = 0.4f;

    public const int PLAYER_CARD_NUM = 8;
    const float ICON_OFFSET = 3f;
    const float CARD_OFFSET = 3.7f;

    WaitForSeconds cardMoveDelay = new WaitForSeconds(0.2f);

    List<int> cardBuffer;
    List<char> cardShape;       // 0-12 : Clover , 13-25 : Diamond, 26-38 : Heart, 39-51 : Spade
    List<Sprite> cardSprites;   // same with above
    List<int> cardNum;          // 0-12 : Clover's 1 to 13, 13-25 : Diamond's 1 to 13 ,,,,

    List<GameObject> leavePlayerCard;

    Transform cardDeckPos;

    Vector3[,] playerCardPos;
    //public int CardLen { get { return PokerGameControl.Control.StageCount == 5 ? 0 : 1; } }

    int tmp = 0;
    int TMPLEN
    {
        get { return tmp++; }
    }

    UI_Poker _pokerUI;

    public static Action<string> OnAddCard;

    bool isInited = false;

    public void Init()
    {
        if (isInited)
            return;

        isInited = true;
        Setup();

        OnAddCard -= AddCardOrPopCard;
        OnAddCard += AddCardOrPopCard;
    }

    void Setup()
    {
        _pokerUI = (UI_Poker)Managers.UI.SceneUI;

        cardDeckPos = GameObject.FindGameObjectWithTag("Deck").transform;

        playerCardPos = new Vector3[PokerGameControl.MAX_PLAYER_NUM, PLAYER_CARD_NUM];
        SetupPlayerCardPos();

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
        for (int i = 0; i < PokerGameControl.MAX_PLAYER_NUM; i++)
        {
            int seatedIndex = HoldemGameControl.Control.ConvertGameToUI(i);
            GameObject destGO = _pokerUI.GetPlayerGameObjcet(seatedIndex);
            RectTransform reference = destGO.GetComponent<RectTransform>();
            // 기준 RectTransform의 가로 길이
            float width = reference.rect.width;
            // 피벗 기준으로 오른쪽 끝 로컬 좌표 계산
            // (1 - pivot.x)을 곱하면 피벗 위치에서 오른쪽 끝까지 거리
            Vector3 localEdge = Vector3.zero;
            Vector3 worldPos = Vector3.zero;

            if (seatedIndex % 2 == 0 && seatedIndex != 0)   // 카드가 왼쪽에서 정렬
            {
                localEdge = new Vector3(-reference.pivot.x * width, 0f, 0f);
                worldPos = reference.TransformPoint(localEdge);
                worldPos.x -= ICON_OFFSET;
                playerCardPos[i, 0] = worldPos;

                for (int j = 1; j < PLAYER_CARD_NUM; j++)
                {
                    playerCardPos[i, j] = playerCardPos[i, j - 1];
                    if (j != 4)
                        playerCardPos[i, j].x -= CARD_OFFSET;
                }

                for (int j = 0; j < PLAYER_CARD_NUM / 2; j++)
                {
                    int oppositeIndex = PLAYER_CARD_NUM - j - 1;
                    var tmp = playerCardPos[i, j];
                    playerCardPos[i, j] = playerCardPos[i, oppositeIndex];
                    playerCardPos[i, oppositeIndex] = tmp;
                }
            }
            else        // 카드가 오른쪽에서 정렬
            {
                localEdge = new Vector3((1f - reference.pivot.x) * width, 0f, 0f);
                worldPos = reference.TransformPoint(localEdge);
                worldPos.x += ICON_OFFSET;
                playerCardPos[i, 0] = worldPos;

                for (int j = 1; j < PLAYER_CARD_NUM; j++)
                {
                    playerCardPos[i, j] = playerCardPos[i, j - 1];
                    if (j != 4)
                        playerCardPos[i, j].x += CARD_OFFSET;
                }
            }
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

    public IEnumerator DealingCard(int toPlayer = -1)
    {
        yield return cardMoveDelay;
        OnAddCard?.Invoke("");
        //string pUID = PokerGameControl.Players.GetPlayerUID(toPlayer);

        //SyncSystem.Sync.HoldemAddCard(pUID);
    }

    public void AddCardToPlayerStarter(string playerUID = "")
    {
        OnAddCard?.Invoke(playerUID);
    }

    private void AddCardOrPopCard(string playerUID = "")
    {
        int popedCard = PopCard();

        if (!PhotonNetwork.IsMasterClient)
            return;

        AddCardToPlayer(popedCard, playerUID);
        //SyncSystem.Sync.HoldemNextStage_V2(1);
    }

    private void AddCardToPlayer(int popedCard, string pUID)
    {
        if (cardDeckPos == null)
            cardDeckPos = GameObject.FindGameObjectWithTag("Deck").transform;

        GameObject cardGO = Managers.Resource.PhotonInstantiate("Game/Card", cardDeckPos);
        cardGO.GetComponent<PhotonView>().OwnershipTransfer = OwnershipOption.Takeover;

        //SyncSystem.Sync.SyncHoldemPlayerCard(pUID, cardGO, popedCard);

        CardMoveToPosPlayer(cardGO, pUID);
    }

    void CardMoveToPosPlayer(GameObject cardGO, string pUID)
    {
        Vector3 destPos = playerCardPos[0, TMPLEN];

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
        cardBuffer.Clear();

        for (int i = 0; i < FULL_CARD_LEN; i++)
        {
            cardBuffer.Add(cardDeck[i]);
        }

        HoldemGameControl.Control.NextStage();
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

    public void ClearCard()
    {
        leavePlayerCard.Clear();
    }

    public void GiveHoldemCardManagerSyncData(Player newPlayer)
    {

    }
}
