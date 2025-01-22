using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Inst { get; private set; }
    void Awake() => Inst = this;

    [Header("Develop")]
    [SerializeField][Tooltip("������ ���� ���")] bool fastMode;

    [Header("Properties")]
    public bool isLoading;      // ī�� ��а� ��� �÷��̾� ���϶� Ŭ�� ������
    public bool myTurn;

    WaitForSeconds delay05 = new WaitForSeconds(0.5f);
    WaitForSeconds delay07 = new WaitForSeconds(0.7f);

    public int roundNum;
    public bool isFirst;
    public int playerBB;
    public int playerSB;
    public int playerD;

    public static Action<int> OnAddCard;

    void Start()
    {

    }

    void GameSetup()
    {
        if (fastMode)
            delay05 = new WaitForSeconds(0.05f);

        playerD = isFirst ? (playerD + 1) % GameManager.Inst.totalPlayer : Random.Range(0, GameManager.Inst.totalPlayer);
        playerSB = (playerD + 1) % GameManager.Inst.totalPlayer;
        playerBB = (playerSB + 1) % GameManager.Inst.totalPlayer;
        roundNum = 0;
        isFirst = true;
    }

    public IEnumerator StartGameCo()
    {
        GameSetup();
        isLoading = true;

        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < GameManager.Inst.totalPlayer; i++)
            {
                int toPlayer = (playerSB + i) % GameManager.Inst.totalPlayer;

                yield return delay05;
                OnAddCard?.Invoke(toPlayer);
            }
        }
        StartCoroutine(StartTurnCo());
    }

    public IEnumerator StartTurnCo()
    {
        isLoading = true;
        roundNum++;

        switch (roundNum)
        {
            case 1:
                PlayerManager.Inst.StartRound();
                break;
            case 2:
                for (int i = 0; i < 3; i++)
                {
                    yield return delay07;
                    OnAddCard?.Invoke(GameManager.Inst.dealer);
                    yield return delay07;
                }
                PlayerManager.Inst.StartRound();
                break;
            case 5:
                yield return delay07;
                EndTurn();
                break;
            default:
                yield return delay07;
                OnAddCard?.Invoke(GameManager.Inst.dealer);
                yield return delay07;
                PlayerManager.Inst.StartRound();
                break;
        }
    }

    public void EndTurn()
    {
        List<Player> winnerList = ResultManager.Inst.GetWinner();
        for (int i = 0; i < winnerList.Count; i++)
        {
            Debug.Log($"Player {winnerList[i]}");
        }
        // ���� ȭ�� ����
        GameManager.Inst.SetupNewGame();
    }
}
