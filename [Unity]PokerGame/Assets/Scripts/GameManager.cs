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
    public int totalPlayer;     // 중앙을 기준 플레이어로 하자
    public int mainPlayerIndex = 0;
    public int dealer = 99;

    void Start()
    {
        PlayerManager.Inst.SetupPlayers(totalPlayer);
        PlayerUIManager.Inst.LinkPlayerUI();
        StartGame();        // 추후 버튼 입력으로도 변동
    }

    void Update()
    {
#if UNITY_EDITOR            // 유니티 에디터에서만 치트 적용
        if (!TurnManager.Inst.isLoading) InputCheatKey();   // Loading 아닐 경우만 키 입력 가능
#endif
    }

    // 테스트용 치트
    void InputCheatKey()    
    {                       
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
