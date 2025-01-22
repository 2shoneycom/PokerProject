using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BetType
{
    Call,
    Double,
    Die,
    Quarter,
    Half,
    AllIn
}
public class Player : MonoBehaviour
{
    public string Name { get; private set; } // 각 플레이어 이름
    public int CurrentBet { get; set; } // 각 플레이어의 현재 베팅액
    public int SeedMoney { get; set; } // 각 플레이어의 전체 자산
    public bool IsActive { get; set; } // 현재 플레이어의 상태가 Die면 false 나머지는 true
    public bool IsCall { get; set; } // 현재 플레이어의 상태가 Call이면 true 나머지는 false

    public void Initialize(string name)
    {
        Name = name;
        CurrentBet = 0;
        IsActive = true;
        IsCall = false;
        SeedMoney = 1000000;
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
}
