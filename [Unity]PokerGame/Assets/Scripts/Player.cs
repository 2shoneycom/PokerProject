using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public string Name; // �� �÷��̾� �̸�
    public int CurrentBet; // �� �÷��̾��� ���� ���þ�
    public int SeedMoney; // �� �÷��̾��� ��ü �ڻ�
    public bool IsActive; // ���� �÷��̾��� ���°� Die�� false �������� true
    public bool IsCall; // ���� �÷��̾��� ���°� Call�̸� true �������� false
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

// player �ȿ� card ���� 2�� �߰� round�� �� 4���̰�
// playermanager�� placebet �Լ� �ű��
// playermanager���� start �ȿ� setupplayer�� round ���� �Լ� �ΰ� round �Լ� �ϳ� ���� �� �÷��̾ �ൿ�ϴ� �Լ� �����
// �׸��� round 4��°�� �����ų� ���� ���и� ���� Ÿ�ֿ̹� �����ٰ� return ���ֱ�
// rule �����ϱ� 