using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Card : MonoBehaviour
{
    [SerializeField] SpriteRenderer card;
    [SerializeField] Sprite cardBack;       // ��� �÷��̾�� ������ ī��� ī�� �޸� ��������

    public Item item;
    bool isFront;                           // �޸� �������ϴ���
    int myCardIndex;                        // ���� ī���� �ε���, CardManager�� CardShape, CardNum
                                            // ����Ʈ�� �����Ͽ� � ī������ �ľ� ����

    public void Setup(Item item, int playerIndex)
    {
        this.item = item;
        this.isFront = playerIndex == GameManager.Inst.mainPlayerIndex || playerIndex == GameManager.Inst.dealer;
        myCardIndex = this.item.cardIndex;

        if (this.isFront) card.sprite = this.item.sprite;
        else card.sprite = cardBack;
    }

}
