using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldemPlayer
{
    const float ICON_OFFSET = 3f;
    const float CARD_OFFSET = 4f;
    const int MAX_CARD_NUM = 2;

    int seatedIndex = -1;
    public int SeatIndex {  get { return seatedIndex; } }

    int cardLen = 0;
    GameObject[] myCardList;
    int[] myCardDetail;
    Vector3[] myCardPos;

    int betMoney;
    public int BetMoney { get { return betMoney; } }

    public HoldemPlayer()
    {
        Init();
    }

    void Init()
    {
        myCardList = new GameObject[MAX_CARD_NUM];
        myCardPos = new Vector3[MAX_CARD_NUM];
        myCardDetail = new int[MAX_CARD_NUM];
    }

    public void AddCardToList(GameObject card, int cardDetail)
    {
        if (cardLen > MAX_CARD_NUM)
            return;

        myCardDetail[cardLen] = cardDetail;
        myCardList[cardLen++] = card;
    }

    public void SetSeatIndex(int idx)
    {
        seatedIndex = idx;
        SetCardPos();
    }

    void SetCardPos()
    {
        UI_Holdem holdemUI = (UI_Holdem)Managers.UI.SceneUI;
        GameObject destGO = holdemUI.GetPlayerGameObjcet(seatedIndex);

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
            myCardPos[0] = worldPos;

            worldPos.x -= CARD_OFFSET;
            myCardPos[1] = worldPos;
        }
        else
        {
            localEdge = new Vector3((1f - reference.pivot.x) * width, 0f, 0f);
            worldPos = reference.TransformPoint(localEdge);
            worldPos.x += ICON_OFFSET;
            myCardPos[0] = worldPos;

            worldPos.x += CARD_OFFSET;
            myCardPos[1] = worldPos;
        }
    }

    public GameObject GetLastAddedCard()
    {
        return myCardList[cardLen - 1];
    }

    public Vector3 GetCardPos()
    {
        return myCardPos[cardLen - 1];
    }

    public int GetCardLen()
    {
        return cardLen;
    }

    public void SetBetMoney(int amount)
    {
        betMoney = amount;

        SyncSystem.Instacne.SyncMyBetting(HoldemGameControl.Control.ConvertUItoGame(SeatIndex), amount);
    }
}
