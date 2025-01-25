using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public string Name; // 각 플레이어 이름
    public int CurrentBet; // 각 플레이어의 현재 베팅액
    public int SeedMoney; // 각 플레이어의 전체 자산
    public bool IsActive; // 현재 플레이어의 상태가 Die면 false 나머지는 true
    public bool IsCall; // 현재 플레이어의 상태가 Call이면 true 나머지는 false
    [SerializeField] public List<Card> myCards;
    [SerializeField] public int pIdx;
    public Transform myCardLeft;
    public Transform myCardRight;

    public void Initialize(string name)
    {
        Name = name;
        CurrentBet = 0;
        IsActive = true;
        IsCall = false;
        SeedMoney = 1000000;
        myCards = new List<Card>();
    }

    public void PlaceBet(BetType betType)
    {
        if (!IsActive) return;

        switch (betType)
        {
            case BetType.Call:
                SeedMoney -= (PlayerManager.Inst.canCallMoney - CurrentBet);
                PlayerManager.Inst.totalMoney += (PlayerManager.Inst.canCallMoney - CurrentBet);
                CurrentBet = PlayerManager.Inst.canCallMoney;
                IsCall = true;
                break;
            case BetType.Double:
                if (PlayerManager.Inst.canBetMoney < 2 * PlayerManager.Inst.canCallMoney - CurrentBet)
                {
                    // 더블을 하면 최대 베팅 가능 금액을 초과할 때
                    SeedMoney -= (PlayerManager.Inst.canBetMoney - CurrentBet);
                    PlayerManager.Inst.totalMoney += (PlayerManager.Inst.canBetMoney - CurrentBet);
                    CurrentBet = PlayerManager.Inst.canBetMoney;
                    PlayerManager.Inst.canCallMoney = CurrentBet;
                    break;
                }
                else
                {
                    SeedMoney -= (2 * PlayerManager.Inst.canCallMoney - CurrentBet);
                    PlayerManager.Inst.totalMoney += (2 * PlayerManager.Inst.canCallMoney - CurrentBet);
                    CurrentBet = 2 * PlayerManager.Inst.canCallMoney;
                    PlayerManager.Inst.canCallMoney = CurrentBet;
                    break;
                }
            case BetType.Quarter:
                if (PlayerManager.Inst.canBetMoney < PlayerManager.Inst.totalMoney / 4)
                {
                    // 쿼터를 하면 최대 베팅 가능 금액을 초과할 때
                    SeedMoney -= (PlayerManager.Inst.canBetMoney - CurrentBet);
                    PlayerManager.Inst.totalMoney += (PlayerManager.Inst.canBetMoney - CurrentBet);
                    CurrentBet = PlayerManager.Inst.canBetMoney;
                    PlayerManager.Inst.canCallMoney = CurrentBet;
                    break;
                }
                else
                {
                    SeedMoney -= PlayerManager.Inst.totalMoney / 4;
                    PlayerManager.Inst.totalMoney += PlayerManager.Inst.totalMoney / 4;
                    CurrentBet += PlayerManager.Inst.totalMoney / 4;
                    PlayerManager.Inst.canCallMoney = CurrentBet;
                    break;
                }
            case BetType.Half:
                if (PlayerManager.Inst.canBetMoney < PlayerManager.Inst.totalMoney / 2)
                {
                    // 하프를 하면 최대 베팅 가능 금액을 초과할 때
                    SeedMoney -= (PlayerManager.Inst.canBetMoney - CurrentBet);
                    PlayerManager.Inst.totalMoney += (PlayerManager.Inst.canBetMoney - CurrentBet);
                    CurrentBet = PlayerManager.Inst.canBetMoney;
                    PlayerManager.Inst.canCallMoney = CurrentBet;
                    break;
                }
                else
                {
                    SeedMoney -= PlayerManager.Inst.totalMoney / 2;
                    PlayerManager.Inst.totalMoney += PlayerManager.Inst.totalMoney / 2;
                    CurrentBet += PlayerManager.Inst.totalMoney / 2;
                    PlayerManager.Inst.canCallMoney = CurrentBet;
                    break;
                }
            case BetType.AllIn:
                if (PlayerManager.Inst.canBetMoney < PlayerManager.Inst.totalMoney)
                {
                    // 올인을 하면 베팅 최대 가능 금액을 초과할 때
                    SeedMoney -= (PlayerManager.Inst.canBetMoney - CurrentBet);
                    PlayerManager.Inst.totalMoney += (PlayerManager.Inst.canBetMoney - CurrentBet);
                    CurrentBet = PlayerManager.Inst.canBetMoney;
                    PlayerManager.Inst.canCallMoney = CurrentBet;
                    break;
                }
                else
                {
                    SeedMoney -= PlayerManager.Inst.totalMoney;
                    PlayerManager.Inst.totalMoney += PlayerManager.Inst.totalMoney;
                    CurrentBet += PlayerManager.Inst.totalMoney;
                    PlayerManager.Inst.canCallMoney = CurrentBet;
                    break;
                }
            case BetType.Die:
                IsActive = false;
                PlayerManager.Inst.diePlayer += 1;
                PlayerManager.Inst.canBetMoney = Mathf.Min(PlayerManager.Inst.canBetMoney, PlayerManager.Inst.LeastMoney());
                break;
        }
    }

    public void RemoveCard()
    {
        for (int i = 0; i < myCards.Count; i++)
        {
            var card = myCards[i];
            var cardObject = card.transform.gameObject;
            Destroy(cardObject);
        }
        myCards.Clear();
    }
}
