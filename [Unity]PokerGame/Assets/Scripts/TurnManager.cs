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
        roundNum = 1;
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
        switch (roundNum)
        {
            case 1:
                roundNum++;
                PlayerManager.Inst.StartRound();
                break;
            case 2:
                roundNum++;
                for (int i = 0; i < 3; i++)
                {
                    yield return delay07;
                    OnAddCard?.Invoke(GameManager.Inst.dealer);
                    yield return delay07;
                }
                PlayerManager.Inst.StartRound();
                break;
            case 5:
                StartCoroutine(EndTurn());
                break;
            default:
                roundNum++;
                yield return delay07;
                OnAddCard?.Invoke(GameManager.Inst.dealer);
                yield return delay07;
                PlayerManager.Inst.StartRound();
                break;
        }
        // �� ����, ���Ŀ� �� -> 3�� ��� -> �� -> 1�� ��� -> �� -> 1�� ��� -> �� -> �Ǵ�
    }

    public IEnumerator EndTurn()
    {
        // 우승자 리스트 가져오기
        List<Player> winnerList = ResultManager.Inst.GetWinner();
        for (int i = 0; i < winnerList.Count; i++)
        {
            foreach (var winner in winnerList) {
                Debug.Log("우승자: " + winner.pIdx);
            }
        }

        // 3초 대기
        yield return new WaitForSeconds(3f);

        // 새로운 게임 설정
        GameManager.Inst.SetupNewGame();
    }
}
