using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Inst {  get; private set; }
    void Awake() => Inst = this;

    public int totalPlayer = 1;     // 0���� ���� �÷��̾�� ����
    public int mainPlayer = 0;
    public int dealerPlayer = 99;
    public int currentPlayer;

    // Start is called before the first frame update
    void Start()
    {
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
