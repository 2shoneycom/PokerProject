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
    [SerializeField][Tooltip("디버깅용 가속 모드")] bool fastMode;

    [Header("Properties")]
    public bool isLoading;      // 카드 배분과 상대 플레이어 턴일때 클릭 방지용 // (승헌)이게 true이면 카드를 나눠주는 중인건가?
                                // (희준) 아직까지 사용을 안했어서 이번에 입력 방지용으로 사용됨.
    public bool myTurn;

    WaitForSeconds delay05 = new WaitForSeconds(0.5f);
    WaitForSeconds delay07 = new WaitForSeconds(0.7f);

    public int roundNum;
    public bool isFirst;
    public int playerBB;
    public int playerSB;
    public int playerD;

    public static Action<int> OnAddCard;

    void GameSetup()
    {
        if (fastMode) {
            delay05 = new WaitForSeconds(0.05f);
        }

        playerD = isFirst ? (playerD + 1) % GameManager.Inst.totalPlayer : Random.Range(0, GameManager.Inst.totalPlayer);
        playerSB = (playerD + 1) % GameManager.Inst.totalPlayer;
        playerBB = (playerSB + 1) % GameManager.Inst.totalPlayer;
        roundNum = 0;
        isFirst = true;     // 가장 처음의 게임은 딜러가 랜덤으로 정해지고, 이후는 이전 딜러의 다음 순서부터 딜러가 됨
    }

    public IEnumerator StartGameCo()
    {
        GameSetup();
        isLoading = true;

        // SB부터 시계방향으로 카드 2장씩 나눠주기
        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < GameManager.Inst.totalPlayer; i++)
            {
                int toPlayer = (playerSB + i) % GameManager.Inst.totalPlayer;

                yield return delay05;
                OnAddCard?.Invoke(toPlayer);    // == CardManager.AddCard(toPlayer)
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
                // 1번째 베팅 라운드
                PlayerManager.Inst.StartRound();
                break;
            case 2:
                // 2번째 베팅 라운드
                for (int i = 0; i < 3; i++)
                {
                    yield return delay07;
                    OnAddCard?.Invoke(GameManager.Inst.dealer);
                    yield return delay07;
                }
                PlayerManager.Inst.StartRound();
                break;
            case 5:
                // 베팅 끝, 승리 판단
                yield return delay07;
                StartCoroutine(EndTurn());
                break;
            default:
                // 3,4번째 베팅 라운드
                yield return delay07;
                OnAddCard?.Invoke(GameManager.Inst.dealer);
                yield return delay07;
                PlayerManager.Inst.StartRound();
                break;
        }

        if (roundNum != 5) isLoading = false;   // 베팅 종료시엔 계속 로딩
    }

    public IEnumerator EndTurn()
    {
        isLoading = true;
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
