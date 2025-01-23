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

    // Start is called before the first frame update
    void Start()
    {
        PlayerManager.Inst.SetupPlayers(totalPlayer);
        StartGame();        // ���� ��ư �Է����ε� ����
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR            // ����Ƽ �����Ϳ����� ġƮ ����
        InputCheatKey();
#endif
    }

    void InputCheatKey()    // �׽�Ʈ�� ġƮ
    {                       // 1�� �ִ� 2��, 2�� �ִ� 5���� ������.
        if (Input.GetKeyDown(KeyCode.Keypad1))
            TurnManager.OnAddCard?.Invoke(mainPlayerIndex);
        if (Input.GetKeyDown(KeyCode.UpArrow))
            TurnManager.OnAddCard?.Invoke(dealer);
        if (Input.GetKeyDown(KeyCode.DownArrow))
            TurnManager.Inst.EndTurn();
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
