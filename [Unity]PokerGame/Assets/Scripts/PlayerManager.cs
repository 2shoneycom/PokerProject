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
    public int canCallMoney = 0;
    public int canBetMoney = 0;
    public int diePlayer = 0;
    public int roundCount = 0;
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

    void Start()
    {
       
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
        players[0].Initialize("Player 0");

        for (int i = 0; i < totalPlayers - 1; i++)
        {
            var curPlayerInst = Instantiate(playerPrefabs, playerPos[i].position, Quaternion.identity);
            Player curPlayer = curPlayerInst.GetComponent<Player>();
            players.Add(curPlayer);
            curPlayer.Initialize($"Player {i + 1}");
        }
    }
    public void StartRound()
    {
        if (TurnManager.Inst.roundNum == 1) {
            currentPlayerIndex = (TurnManager.Inst.playerSB + 2) % players.Count;
            InitializePlayer();
        }
        else {
            currentPlayerIndex = TurnManager.Inst.playerSB;
            MakeIsCallFalse();
        }
        StartBetting(currentPlayerIndex);
    }
    void StartBetting(int currentPlayerIndex) {
        if(currentPlayerIndex != GameManager.Inst.mainPlayerIndex)
        {
            DisableAllButtons();
        }

        Player currentPlayer = players[currentPlayerIndex];

        if (diePlayer == players.Count - 1)
        {
            StartCoroutine(TurnManager.Inst.StartTurnCo());
            return;
        }
        if (currentPlayer.CurrentBet == canCallMoney && currentPlayer.IsCall)
        {
            StartCoroutine(TurnManager.Inst.StartTurnCo());
            return;
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
        StartBetting(currentPlayerIndex);
    }

    void UpdateButtonStates()
    {
        Player currentPlayer = players[currentPlayerIndex];

        // ��ư Ȱ��ȭ/��Ȱ��ȭ (���� �÷��̾� ���¿� ����)
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