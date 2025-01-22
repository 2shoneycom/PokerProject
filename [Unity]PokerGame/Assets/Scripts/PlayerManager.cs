using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening.Core.Easing;
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

    public List<Player> players;
    private int currentPlayerIndex = 0;
    public GameObject playerPrefab;
    public int totalMoney = 0;
    public int canCallMoney = 0;
    public int canBetMoney = 0;
    public int diePlayer = 0;
    public int roundCount = 0;
    private Coroutine turnTimerCoroutine;

    private void Awake()
    {
        Inst = this;
    }

    void Start()
    {
        SetupPlayers(7); // 7���� �÷��̾� ����
    }
    void SetupPlayers(int totalPlayers)
    {
        players = new List<Player>();
        for (int i = 1; i <= totalPlayers; i++)
        {
            GameObject playerObject = Instantiate(playerPrefab);
            Player player = playerObject.GetComponent<Player>();

            player.Initialize($"Player {i}");
            players.Add(player);
        }
    }
    void StartRound()
    {
        if (TurnManager.Inst.roundNum == 1) {
            currentPlayerIndex = (GameManager.Inst.playerSB + 2) % players.Count;
            InitializePlayer();
        }
        else {
            currentPlayerIndex = GameManager.Inst.playerSB;
            MakeIsCallFalse();
        }
        StartBet(currentPlayerIndex);
    }
    void StartBet(int currentPlayerIndex) {
        Player currentPlayer = players[currentPlayerIndex];

        if (diePlayer == players.Count - 1)
        {
            return StartCoroutine(TurnManager.Inst.StartTurnCo());
        }
        if (currentPlayer.CurrentBet == canCallMoney && currentPlayer.IsCall)
        {
            return StartCoroutine(TurnManager.Inst.StartTurnCo());
        }
        // ��Ȱ��ȭ�� �÷��̾�� �� ��ŵ
        if (!currentPlayer.IsActive)
        {
            Debug.Log($"Player {currentPlayerIndex + 1} is inactive. Skipping turn.");
            EndTurn();
            return;
        }
        if (currentPlayerIndex == 0) {
            UpdateButtonStates();
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
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count; // ���� �÷��̾�� �̵�
        StartBet(currentPlayerIndex);
    }

    void UpdateButtonStates()
    {
        Player currentPlayer = players[currentPlayerIndex];

        // ��ư Ȱ��ȭ/��Ȱ��ȭ (���� �÷��̾� ���¿� ����)
        currentPlayer.callButton.interactable = currentPlayer.IsActive;
        currentPlayer.doubleButton.interactable = currentPlayer.IsActive && currentPlayer.IsCall == false;
        currentPlayer.dieButton.interactable = true;
        currentPlayer.quarterButton.interactable = currentPlayer.IsActive && currentPlayer.IsCall == false;
        currentPlayer.halfButton.interactable = currentPlayer.IsActive && currentPlayer.IsCall == false;
        currentPlayer.allInButton.interactable = currentPlayer.IsActive && currentPlayer.IsCall == false;
    }

    public int LeastMoney()
    {
        int minMoney = int.MaxValue;

        // ���� �ּҰ� ��� (LINQ �ּ�ȭ)
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

        // ���� �÷��̾ 7�� ���� ��ư�� ������ �ʾ��� ��� Die ó��
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
        totalMoney = 0;
        canCallMoney = 0;
        diePlayer = 0;
        roundCount = 0;
    }
}