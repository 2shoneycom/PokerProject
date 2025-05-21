using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldemPlayer
{
    int seatedIndex = -1;
    public int SeatIndex { get { return seatedIndex; }
        set { 
            value = seatedIndex;
        } 
    }

    int cardLen = 0;
    GameObject[] myCardList;

    public HoldemPlayer()
    {
        Init();
    }

    void Init()
    {
        myCardList = new GameObject[2];
    }

    public void AddCardToList(GameObject card)
    {
        if (cardLen > 2)
            return;

        myCardList[cardLen++] = card;
    }

    public GameObject GetLastAddedCard()
    {
        return myCardList[cardLen - 1];
    }

    public int GetPosIndex()
    {
        return cardLen - 1;
    }
}
