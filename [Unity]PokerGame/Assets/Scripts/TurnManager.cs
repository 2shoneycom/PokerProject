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
        // �� ����, ���Ŀ� �� -> 3�� ��� -> �� -> 1�� ��� -> �� -> 1�� ��� -> �� -> �Ǵ�
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
