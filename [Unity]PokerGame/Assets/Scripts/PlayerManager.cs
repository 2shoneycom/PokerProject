using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Inst { get; private set; }

    [SerializeField] Button callButton;
    [SerializeField] Button doubleButton;
    [SerializeField] Button dieButton;
    [SerializeField] Button quarterButton;
    [SerializeField] Button halfButton;
    [SerializeField] Button allInButton;

    public List<Player> players;
    private int currentPlayerIndex = 2;
    public GameObject playerPrefab;
    public int totalMoney = 15000;
    public int canCallMoney = 10000;
    public int canBetMoney = 1000000;
    public int diePlayer = 0;
    public int roundCount = 0;
    private Coroutine turnTimerCoroutine;
    public int bigBlindIndex = 1;
    public int smallBlindIndex = 0;

    private void Awake()
    {
        Inst = this;
    }

    void Start()
    {
        SetupPlayers(7); // 7명의 플레이어 생성
        UpdateButtonStates(); // 첫 번째 플레이어의 버튼 활성화 상태 업데이트
        if (turnTimerCoroutine != null)
        {
            StopCoroutine(turnTimerCoroutine);
        }
        turnTimerCoroutine = StartCoroutine(AutoDieTimer(7f));
    }

    void SetupPlayers(int totalPlayers)
    {
        players = new List<Player>();
        for (int i = 1; i <= totalPlayers; i++)
        {
            GameObject playerObject = Instantiate(playerPrefab);
            Player player = playerObject.GetComponent<Player>();

            player.Initialize($"Player {i}");
            if (i == smallBlindIndex + 1)
            {
                player.CurrentBet = 5000;
                player.SeedMoney -= 5000;
            }
            else if (i == bigBlindIndex + 1)
            {
                player.CurrentBet = 10000;
                player.SeedMoney -= 10000;
            }
            players.Add(player);
        }
    }

    public void OnButtonClicked(string betType)
    {
        if (turnTimerCoroutine != null)
        {
            StopCoroutine(turnTimerCoroutine);
        }
        Player currentPlayer = players[currentPlayerIndex];

        if (!currentPlayer.IsActive)
        {
            Debug.Log("button clicked");
            return;
        }

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
        UpdateButtonStates();
        if (turnTimerCoroutine != null)
        {
            StopCoroutine(turnTimerCoroutine);
        }
        turnTimerCoroutine = StartCoroutine(AutoDieTimer(10f));
    }

    void UpdateButtonStates()
    {
        Player currentPlayer = players[currentPlayerIndex];

        if (diePlayer == players.Count - 1)
        {
            currentPlayer.SeedMoney += totalMoney;
            Debug.Log("No players are available! You win");
            InitializePlayer();
        }

        if (currentPlayer.CurrentBet == canCallMoney && currentPlayer.IsCall)
        {
            roundCount += 1;
            Debug.Log("Everyone finish bettings.");
            Debug.Log($"each player bets {currentPlayer.CurrentBet}");
            if (roundCount == 3)
            {
                Debug.Log("You need to figure out who wins the game");
                //승패 결정하고 이긴 사람이 돈 가지기 코드 필요
                InitializePlayer();
            }
            else
            {
                Debug.Log("Let's move to next round");
                Debug.Log($"total money is {totalMoney}");
                MakeIsCallFalse();
            }
        }

        // 비활성화된 플레이어면 턴 스킵
        if (!currentPlayer.IsActive)
        {
            Debug.Log($"Player {currentPlayerIndex + 1} is inactive. Skipping turn.");
            EndTurn();
            return;
        }

        // 버튼 활성화/비활성화 (현재 플레이어 상태에 따라)
        callButton.interactable = currentPlayer.IsActive;
        doubleButton.interactable = currentPlayer.IsActive && currentPlayer.IsCall == false;
        dieButton.interactable = true;
        quarterButton.interactable = currentPlayer.IsActive && currentPlayer.IsCall == false;
        halfButton.interactable = currentPlayer.IsActive && currentPlayer.IsCall == false;
        allInButton.interactable = currentPlayer.IsActive && currentPlayer.IsCall == false;
    }

    public int LeastMoney()
    {
        int minMoney = int.MaxValue;

        // 직접 최소값 계산 (LINQ 최소화)
        foreach (var player in players)
        {
            if (player.IsActive && player.SeedMoney < minMoney)
            {
                minMoney = player.SeedMoney;
            }
        }

        return minMoney;
    }
    void MakeIsCallFalse()
    {
        foreach (var player in players)
        {
            if (player.IsCall)
            {
                player.IsCall = false;
            }
        }
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
            player.IsActive = true;
            player.IsCall = false;
            player.CurrentBet = 0;
        }
        canBetMoney = LeastMoney();
        totalMoney = 15000;
        smallBlindIndex = (smallBlindIndex + 1) % players.Count;
        bigBlindIndex = (bigBlindIndex + 1) % players.Count;
        canCallMoney = 10000;
        diePlayer = 0;
        roundCount = 0;
    }
}
