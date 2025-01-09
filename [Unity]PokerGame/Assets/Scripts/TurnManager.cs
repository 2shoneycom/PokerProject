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
    [SerializeField][Tooltip("���� �� ��带 ����")] ETurnMode eTurnMode;
    [SerializeField][Tooltip("������ ���� ���")] bool fastMode;

    [Header("Properties")]
    public bool isLoading;      // ī�� ��а� ��� �÷��̾� ���϶� Ŭ�� ������
    public bool myTurn;

    enum ETurnMode { Random, MyTurn, OthersTurn }   //n���� �� �ʿ�������?
    WaitForSeconds delay05 = new WaitForSeconds(0.5f);
    WaitForSeconds delay07 = new WaitForSeconds(0.7f);
    GameManager gameManager;

    public static Action<int> OnAddCard;

    void Start()
    {
        gameManager = GameManager.Inst;
    }

    void GameSetup()
    {
        if (fastMode)
            delay05 = new WaitForSeconds(0.05f);

        switch (eTurnMode)
        {
            case ETurnMode.Random:
                gameManager.currentPlayer = Random.Range(0, gameManager.totalPlayer);
                myTurn = gameManager.currentPlayer == gameManager.mainPlayer;
                break;
            case ETurnMode.MyTurn:
                gameManager.currentPlayer = gameManager.mainPlayer;
                myTurn = true;
                break;
            case ETurnMode.OthersTurn:
                gameManager.currentPlayer = 0; // Random.Range(1, gameManager.totalPlayer);
                myTurn = true;  //false;
                break;
        }
    }

    public IEnumerator StartGameCo()
    {
        GameSetup();
        isLoading = true;

        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < gameManager.totalPlayer; i++)
            {
                int toPlayer = (gameManager.currentPlayer + i) % gameManager.totalPlayer;

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
        OnAddCard?.Invoke(gameManager.dealerPlayer);
        yield return delay07;

        isLoading = false;
    }

    public void EndTurn()
    {
        gameManager.currentPlayer = (gameManager.currentPlayer + 1) % gameManager.totalPlayer;
        myTurn = gameManager.currentPlayer == gameManager.mainPlayer;
        StartCoroutine(StartTurnCo());
    }
}
