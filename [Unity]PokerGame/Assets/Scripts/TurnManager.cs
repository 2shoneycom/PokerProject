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
    public bool isLoading;      // 카드 배분과 상대 플레이어 턴일때 클릭 방지용
    public bool myTurn;

    WaitForSeconds delay05 = new WaitForSeconds(0.5f);
    WaitForSeconds delay07 = new WaitForSeconds(0.7f);

    public static Action<int> OnAddCard;

    void Start()
    {

    }

    void GameSetup()
    {
        if (fastMode)
            delay05 = new WaitForSeconds(0.05f);

        GameManager.Inst.playerD = Random.Range(0, GameManager.Inst.totalPlayer);
        GameManager.Inst.playerSB = (GameManager.Inst.playerD + 1) % GameManager.Inst.totalPlayer;
        GameManager.Inst.playerBB = (GameManager.Inst.playerSB + 1) % GameManager.Inst.totalPlayer;
        GameManager.Inst.currentPlayerIndex = (GameManager.Inst.playerBB + 1) % GameManager.Inst.totalPlayer;
        myTurn = GameManager.Inst.currentPlayerIndex == GameManager.Inst.mainPlayerIndex;
    }

    public IEnumerator StartGameCo()
    {
        GameSetup();
        isLoading = true;

        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < GameManager.Inst.totalPlayer; i++)
            {
                int toPlayer = (GameManager.Inst.playerSB + i) % GameManager.Inst.totalPlayer;

                yield return delay05;
                OnAddCard?.Invoke(toPlayer);
            }
        }
        StartCoroutine(StartTurnCo());
    }

    IEnumerator StartTurnCo()
    {
        // 턴 구현, 추후엔 딜 -> 3장 배분 -> 딜 -> 1장 배분 -> 딜 -> 1장 배분 -> 딜 -> 판단
        isLoading = true;

        yield return delay07;
        OnAddCard?.Invoke(GameManager.Inst.dealer);
        yield return delay07;

        isLoading = false;
    }

    public void EndTurn()
    {
        GameManager.Inst.currentPlayerIndex = (GameManager.Inst.currentPlayerIndex + 1) % GameManager.Inst.totalPlayer;
        myTurn = GameManager.Inst.currentPlayerIndex == GameManager.Inst.mainPlayerIndex;
        StartCoroutine(StartTurnCo());
    }
}
