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
    public List<int> cards;

    [SerializeField] public Button callButton;
    [SerializeField] public Button doubleButton;
    [SerializeField] public Button dieButton;
    [SerializeField] public Button quarterButton;
    [SerializeField] public Button halfButton;
    [SerializeField] public Button allInButton;

    public void Initialize(string name)
    {
        Name = name;
        CurrentBet = 0;
        IsActive = true;
        IsCall = false;
        SeedMoney = 1000000;
        cards = new List<int>();
        DisableAllButtons();
    }
    public void DisableAllButtons()
    {
        callButton.interactable = false;
        doubleButton.interactable = false;
        dieButton.interactable = false;
        quarterButton.interactable = false;
        halfButton.interactable = false;
        allInButton.interactable = false;
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

// player 안에 card 변수 2개 추가 round는 총 4번이고
// playermanager에 placebet 함수 옮기기
// playermanager에서 start 안에 setupplayer랑 round 시작 함수 두고 round 함수 하나 만들어서 각 플레이어가 행동하는 함수 만들기
// 그리고 round 4번째가 끝나거나 게임 승패를 가릴 타이밍에 끝났다고 return 해주기
// rule 수정하기 