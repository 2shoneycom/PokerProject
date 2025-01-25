using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Card : MonoBehaviour
{
    [SerializeField] SpriteRenderer card;
    [SerializeField] Sprite cardBack;       // 상대 플레이어와 딜러의 카드는 카드 뒷면 보여야함

    public Item item;
    bool isFront;                           // 뒷면 보여야하는지
    public int myCardIndex;                        // 현재 카드의 인덱스, CardManager의 CardShape, CardNum
                                            // 리스트에 접근하여 어떤 카드인지 파악 가능

    public void Setup(Item item, int playerIndex)
    {
        this.item = item;
        this.isFront = playerIndex == GameManager.Inst.mainPlayerIndex || playerIndex == GameManager.Inst.dealer;
        myCardIndex = this.item.cardIndex;
        this.isFront = true;
        if (this.isFront) card.sprite = this.item.sprite;
        else card.sprite = cardBack;
    }

}
