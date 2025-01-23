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
    public int myCardIndex;                        // ���� ī���� �ε���, CardManager�� CardShape, CardNum
                                            // ����Ʈ�� �����Ͽ� � ī������ �ľ� ����

    public void Setup(Item item, int playerIndex)
    {
        this.item = item;
        this.isFront = playerIndex == GameManager.Inst.mainPlayerIndex || playerIndex == GameManager.Inst.dealer;
        myCardIndex = this.item.cardIndex;
        this.isFront = true;    // (희준) 디버깅용 앞면 보이기, 추후엔 삭제
        if (this.isFront) card.sprite = this.item.sprite;
        else card.sprite = cardBack;
    }

}
