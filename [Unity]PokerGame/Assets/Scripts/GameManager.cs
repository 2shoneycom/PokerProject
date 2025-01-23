using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Inst {  get; private set; }
    void Awake() => Inst = this;

    public GameObject mainPlayer;
    public int totalPlayer;     // �߾��� ���� �÷��̾�� ����
    public int mainPlayerIndex = 0;
    public int dealer = 99;

    void Start()
    {
        PlayerManager.Inst.SetupPlayers(totalPlayer);
        StartGame();        // ���� ��ư �Է����ε� ����
    }

    void Update()
    {
#if UNITY_EDITOR            // ����Ƽ �����Ϳ����� ġƮ ����
        if (!TurnManager.Inst.isLoading) InputCheatKey();   // (희준) Loading 아닐 경우만 키 입력 가능
#endif
    }

    void InputCheatKey()    // �׽�Ʈ�� ġƮ
    {                       // 1�� �ִ� 2��, 2�� �ִ� 5���� ������.
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log($"{PlayerManager.Inst.currentPlayerIndex} Call");
            PlayerManager.Inst.OnButtonClicked("Call");
        }
        if (Input.GetKeyDown(KeyCode.W))
            PlayerManager.Inst.OnButtonClicked("Double");
        if (Input.GetKeyDown(KeyCode.E))
            PlayerManager.Inst.OnButtonClicked("Die");
        if (Input.GetKeyDown(KeyCode.R))
            PlayerManager.Inst.OnButtonClicked("Quarter");
        if (Input.GetKeyDown(KeyCode.T))
            PlayerManager.Inst.OnButtonClicked("Half");
        if (Input.GetKeyDown(KeyCode.Y))
            PlayerManager.Inst.OnButtonClicked("AllIn");
    }

    public void StartGame()
    {
        StartCoroutine(TurnManager.Inst.StartGameCo());
    }

    public void SetupNewGame()
    {
        CardManager.Inst.RemoveDealerCard();
        for (int i = 0; i < PlayerManager.Inst.players.Count; i++) 
        {
            PlayerManager.Inst.players[i]?.RemoveCard();
        }
        CardManager.Inst.ShuffleCard();
        StartCoroutine(TurnManager.Inst.StartGameCo());
    }
}
