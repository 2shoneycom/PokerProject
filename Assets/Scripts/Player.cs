using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public string Name; // 각 플레이어 이름
    public int currentBet; // 각 플레이어의 현재 베팅액
    public int seedMoney; // 각 플레이어의 전체 자산
    public bool isActive; // 현재 플레이어의 상태가 Die면 false 나머지는 true
    public bool isCall; // 현재 플레이어의 상태가 Call이면 true 나머지는 false
    public bool isBet; // 현재 플레이어의 차례가 와서 베팅에 참여를 했으면 true 나머지는 false
    [SerializeField] public List<Card> myCards;
    [SerializeField] public int pIdx;
    public Transform myCardLeft;
    public Transform myCardRight;

    public void Initialize(string name)
    {
        Name = name;
        currentBet = 0;
        isActive = true;
        isCall = false;
        isBet = false;
        seedMoney = 1000000;
        myCards = new List<Card>();
    }

    public void PlaceBet(BetType betType)
    {
        if (!isActive) return;

        switch (betType)
        {
            case BetType.Call:
                if (PlayerManager.Inst.canCallMoney == currentBet) 
                {
                    // 체크
                    Debug.Log("It is check, not call");
                    isCall = true;
                    isBet = true;
                    break;
                }
                else 
                {
                    // 콜
                    seedMoney -= (PlayerManager.Inst.canCallMoney - currentBet);
                    currentBet = PlayerManager.Inst.canCallMoney;
                    isCall = true;
                    isBet = true;
                    break;
                }
            case BetType.Double:
                if (PlayerManager.Inst.canCallMoney == 0)
                {
                    if (PlayerManager.Inst.canBetMoney < 10000)
                    {
                        // 삥 못하는 상황 -> 삥 단위인 10000원이 베팅 최대 가능 금액을 초과하는 경우
                        seedMoney -= (PlayerManager.Inst.canBetMoney - currentBet);
                        currentBet = PlayerManager.Inst.canBetMoney;
                        PlayerManager.Inst.canCallMoney = currentBet;
                        isBet = true;
                        break;
                    }
                    else
                    {
                        // 삥
                        seedMoney -= 10000;
                        currentBet = 10000;
                        PlayerManager.Inst.canCallMoney = 10000;
                        isBet = true;
                        break;
                    }
                }
                else 
                {
                    if (PlayerManager.Inst.canBetMoney < 2 * (PlayerManager.Inst.canCallMoney - currentBet))
                    {
                        // 더블 못하는 상황 -> 플레이어의 더블 금액이 베팅 최대 가능 금액을 초과하는 경우
                        seedMoney -= (PlayerManager.Inst.canBetMoney - currentBet);
                        currentBet = PlayerManager.Inst.canBetMoney;
                        PlayerManager.Inst.canCallMoney = currentBet;
                        isBet = true;
                        break;
                    }
                    else 
                    {
                        // 더블
                        seedMoney -= 2 * (PlayerManager.Inst.canCallMoney - currentBet);
                        currentBet += 2 * (PlayerManager.Inst.canCallMoney - currentBet);
                        PlayerManager.Inst.canCallMoney = currentBet;
                        isBet = true;
                        break;
                    }
                }
            case BetType.Quarter:
                {
                    int totalPot = PlayerManager.Inst.canCallMoney - currentBet + PlayerManager.Inst.CalculateCurrentBets() + PlayerManager.Inst.totalMoney;
                    if (PlayerManager.Inst.canBetMoney < totalPot / 4 + PlayerManager.Inst.canCallMoney - currentBet)
                    {
                        // 쿼터 못하는 상황 -> 플레이어의 쿼터 금액이 베팅 최대 가능 금액을 초과하는 경우
                        seedMoney -= (PlayerManager.Inst.canBetMoney - currentBet);
                        currentBet = PlayerManager.Inst.canBetMoney;
                        PlayerManager.Inst.canCallMoney = currentBet;
                        isBet = true;
                        break;
                    }
                    else
                    {
                        // 쿼터
                        seedMoney -= (totalPot / 4 + PlayerManager.Inst.canCallMoney - currentBet);
                        currentBet += (totalPot / 4 + PlayerManager.Inst.canCallMoney - currentBet);
                        PlayerManager.Inst.canCallMoney = currentBet;
                        isBet = true;
                        break;
                    }
                }
            case BetType.Half:
                {
                    int totalPot = PlayerManager.Inst.canCallMoney - currentBet + PlayerManager.Inst.CalculateCurrentBets() + PlayerManager.Inst.totalMoney;
                    if (PlayerManager.Inst.canBetMoney < totalPot / 2 + PlayerManager.Inst.canCallMoney - currentBet)
                    {
                        // 하프 못하는 상황 -> 플레이어의 하프 금액이 베팅 최대 가능 금액을 초과하는 경우
                        seedMoney -= (PlayerManager.Inst.canBetMoney - currentBet);
                        currentBet = PlayerManager.Inst.canBetMoney;
                        PlayerManager.Inst.canCallMoney = currentBet;
                        isBet = true;
                        break;
                    }
                    else
                    {
                        // 하프
                        seedMoney -= (totalPot / 2 + PlayerManager.Inst.canCallMoney - currentBet);
                        currentBet += (totalPot / 2 + PlayerManager.Inst.canCallMoney - currentBet);
                        PlayerManager.Inst.canCallMoney = currentBet;
                        isBet = true;
                        break;
                    }
                }
            case BetType.AllIn:
                if (PlayerManager.Inst.canBetMoney < seedMoney)
                {
                    // 올인 못하는 상황 -> 플레이어의 올인 금액이 베팅 최대 가능 금액을 초과하는 경우
                    seedMoney -= (PlayerManager.Inst.canBetMoney - currentBet);
                    currentBet = PlayerManager.Inst.canBetMoney;
                    PlayerManager.Inst.canCallMoney = currentBet;
                    isBet = true;
                    break;
                }
                else
                {
                    // 올인
                    seedMoney -= PlayerManager.Inst.totalMoney;
                    currentBet += PlayerManager.Inst.totalMoney;
                    PlayerManager.Inst.canCallMoney = currentBet;
                    isBet = true;
                    break;
                }
            case BetType.Die:
                isActive = false;
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

// 만약 저번 게임에서 올인을 하는 등, SeedMoney가 최소 콜 금액인 10000원보다 아래가 되는 경우?
// 게임 시작하면 BB와 SB 고정인거 같은데?