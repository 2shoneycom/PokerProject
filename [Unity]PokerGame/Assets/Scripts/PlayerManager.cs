using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UI;

public enum BetType
{
    Call,
    Double,
    Die,
    Quarter,
    Half,
    AllIn
}
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Inst { get; private set; }

    [SerializeField] Transform[] playerPos;
    [SerializeField] Transform mainplayerPos;
    [SerializeField] GameObject playerPrefabs;

    public List<Player> players;
    public int currentPlayerIndex = 0;
    public int totalMoney = 0;
    public int canCallMoney = 0;    // 콜을 하기위해서 내야하는 돈
    public int canBetMoney = 0;     // 최대로 베팅할 수 있는 돈
    public int diePlayer = 0;
    public bool oneLeft = false;    // 한명만 남으면 true 나머지 false
    private Coroutine turnTimerCoroutine;

    [SerializeField] public Button callButton;
    [SerializeField] public Button doubleButton;
    [SerializeField] public Button dieButton;
    [SerializeField] public Button quarterButton;
    [SerializeField] public Button halfButton;
    [SerializeField] public Button allInButton;

    private void Awake()
    {
        Inst = this;
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

    public void SetupPlayers(int totalPlayers)
    {
        players = new List<Player>();

        GameManager.Inst.mainPlayerIndex = 0;
        GameManager.Inst.mainPlayer = Instantiate(playerPrefabs, mainplayerPos.position, Quaternion.identity);
        players.Add(GameManager.Inst.mainPlayer.GetComponent<Player>());
        players[0].pIdx = 0;
        players[0].Initialize("Player 0");

        for (int i = 0; i < totalPlayers - 1; i++)
        {
            var curPlayerInst = Instantiate(playerPrefabs, playerPos[i].position, Quaternion.identity);
            Player curPlayer = curPlayerInst.GetComponent<Player>();
            curPlayer.pIdx = i+1;
            players.Add(curPlayer);
            curPlayer.Initialize($"Player {i + 1}");
        }
    }

    public void StartRound()
    {
        if (TurnManager.Inst.roundNum == 1) 
        {
            currentPlayerIndex = (TurnManager.Inst.playerSB + 2) % players.Count;
            InitializePlayer();
            Player SBPlayer = players[TurnManager.Inst.playerSB];
            SBPlayer.currentBet = 5000;
            SBPlayer.seedMoney -= 5000;
            Player BBPlayer = players[TurnManager.Inst.playerBB];
            BBPlayer.currentBet = 10000;
            BBPlayer.seedMoney -= 10000;
            canCallMoney = 10000;
        }
        else 
        {
            currentPlayerIndex = TurnManager.Inst.playerSB;
            canCallMoney = 0;
            OptionsChange();
        }
        StartBetting(currentPlayerIndex);
    }

    void StartBetting(int currentPlayerIndex) {
        if(currentPlayerIndex != GameManager.Inst.mainPlayerIndex)
        {
            DisableAllButtons();
        }
        if (currentPlayerIndex == 0)
        {
            UpdateButtonStates();
        }
        Player currentPlayer = players[currentPlayerIndex];

        if (!currentPlayer.isActive)
        {
            // 비활성화된 플레이어면 턴 스킵
            Debug.Log($"Player {currentPlayerIndex} is inactive. Skipping turn.");
            EndTurn();
            return;
        }
        if (diePlayer == players.Count - 1)
        {
            // 1명만 남았을 때
            oneLeft = true;
            StartCoroutine(TurnManager.Inst.EndTurn());
            return;
        }
        if (currentPlayer.currentBet == canCallMoney && currentPlayer.isBet)
        {
            // 정상적으로 한 바퀴 돌았을 때
            UpdateTotalMoney();
            StartCoroutine(TurnManager.Inst.StartTurnCo());
            return;
        }
        if (turnTimerCoroutine != null)
            StopCoroutine(turnTimerCoroutine);
        turnTimerCoroutine = StartCoroutine(AutoDieTimer(7f));
    }

    public void OnButtonClicked(string betType)
    {
        if (turnTimerCoroutine != null)
        {
            StopCoroutine(turnTimerCoroutine);
        }
        Player currentPlayer = players[currentPlayerIndex];

        switch (betType)
        {
            case "Call":
                currentPlayer.PlaceBet(BetType.Call);
                break;
            case "Double":
                currentPlayer.PlaceBet(BetType.Double);
                break;
            case "Die":
                currentPlayer.PlaceBet(BetType.Die);
                break;
            case "Quarter":
                currentPlayer.PlaceBet(BetType.Quarter);
                break;
            case "Half":
                currentPlayer.PlaceBet(BetType.Half);
                break;
            case "AllIn":
                currentPlayer.PlaceBet(BetType.AllIn);
                break;
        }

        Debug.Log($"{currentPlayer.Name} selected {betType}");
        EndTurn();
    }

    void EndTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count; // 다음 플레이어로 이동
        StartBetting(currentPlayerIndex);
    }

    void UpdateButtonStates()
    {
        Player currentPlayer = players[currentPlayerIndex];
        // 버튼 활성화/비활성화 (현재 플레이어 상태에 따라)
        callButton.interactable = true;
        doubleButton.interactable = currentPlayer.isCall == false;
        dieButton.interactable = true;
        quarterButton.interactable = currentPlayer.isCall == false;
        halfButton.interactable = currentPlayer.isCall == false;
        allInButton.interactable = currentPlayer.isCall == false;
    }

    public int LeastMoney()
    {
        int minMoney = int.MaxValue;

        // 직접 최소값 계산 (LINQ 최소화)
        foreach (var player in players)
        {
            // (승헌) 살아있는 플레이어들 중 보유금액의 최솟값 구하기
            if (player.isActive && (player.seedMoney + player.currentBet) < minMoney)
            {
                minMoney = player.seedMoney + player.currentBet;
            }
        }

        return minMoney;
    }

    void OptionsChange() // 한 라운드가 끝나면 IsCall false로 초기화 해주기
    {
        foreach (var player in players)
        {
            if (player.isActive)
            {
                player.isCall = false;
                player.isBet = false;
            }
        }
    }
    void UpdateTotalMoney() // 한 라운드가 끝나면 totalMoney 업데이트 해주기
    { 
        foreach (var player in players)
        {
            totalMoney += player.currentBet;
            player.currentBet = 0;
        }
    }
    public int CalculateCurrentBets() // 쿼터, 하프 계산을 위한 현재 플레이어가 베팅한 금액들 합
    {
        int currentBets = 0;
        foreach (var player in players) 
        {
            currentBets += player.currentBet;
        }
        return currentBets;
    }

    IEnumerator AutoDieTimer(float duration)
    {
        yield return new WaitForSeconds(duration);

        // 현재 플레이어가 7초 동안 버튼을 누르지 않았을 경우 Die 처리
        Debug.Log($"Player {players[currentPlayerIndex].Name} didn't respond. Automatically choosing Die.");
        OnButtonClicked("Die");
    }

    void InitializePlayer()
    {
        foreach (var player in players)
        {
            player.isActive = true;
            player.isCall = false;
            player.isBet = false;
            player.currentBet = 0;
        }
        canBetMoney = LeastMoney();
        totalMoney = players.Count * 5000; // notion 사진 분석 결과 SB와 BB의 기본 베팅 말고 처음 팟에 (플레이어수) * (SB가 내는 돈) 만큼 존재하던데?
        canCallMoney = 0;
        diePlayer = 0;
        oneLeft = false;
    }
}