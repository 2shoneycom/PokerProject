using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Inst {  get; private set; }
    void Awake() => Inst = this;

    public int totalPlayer = 1;     // 0번을 기준 플레이어로 하자
    public int mainPlayer = 0;
    public int dealerPlayer = 99;
    public int currentPlayer;

    // Start is called before the first frame update
    void Start()
    {
        StartGame();        // 추후 버튼 입력으로도 변동
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR            // 유니티 에디터에서만 치트 적용
        InputCheatKey();
#endif
    }

    void InputCheatKey()    // 테스트용 치트
    {                       // 1은 최대 2번, 2는 최대 5번만 누를것.
        if (Input.GetKeyDown(KeyCode.Keypad1))
            TurnManager.OnAddCard?.Invoke(mainPlayer);
        if (Input.GetKeyDown(KeyCode.Keypad2))
            TurnManager.OnAddCard?.Invoke(dealerPlayer);
        if (Input.GetKeyDown(KeyCode.Keypad3))
            TurnManager.Inst.EndTurn();
    }

    public void StartGame()
    {
        StartCoroutine(TurnManager.Inst.StartGameCo());
    }
}
