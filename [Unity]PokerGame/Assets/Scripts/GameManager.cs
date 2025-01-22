using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Inst {  get; private set; }
    void Awake() => Inst = this;

    [SerializeField] Transform[] playerPos;
    [SerializeField] Transform mainplayerPos;
    [SerializeField] GameObject playerPrefabs;        // ������ ī�� ������

    GameObject mainPlayer;
    public List<GameObject> players;

    public int totalPlayer;     // �߾��� ���� �÷��̾�� ����
    public int mainPlayerIndex = 0;
    public int dealer = 99;

    // Start is called before the first frame update
    void Start()
    {
        SetupPlayer(totalPlayer);
        StartGame();        // ���� ��ư �Է����ε� ����
    }

    void SetupPlayer(int totalPlayer)
    {
        players = new List<GameObject>();

        mainPlayerIndex = 0;
        mainPlayer = Instantiate(playerPrefabs, mainplayerPos.position, Quaternion.identity);
        players.Add(mainPlayer);

        for(int i = 0; i < totalPlayer - 1; i++)
        {
            players.Add(Instantiate(playerPrefabs, playerPos[i].position, Quaternion.identity));
        }
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
    }

    public void StartGame()
    {
        StartCoroutine(TurnManager.Inst.StartGameCo());
    }

    public void SetupNewGame()
    {
        CardManager.Inst.RemoveDealerCard();
        for(int i=0; i<players.Count; i++)
        {
            players[i].GetComponent<Player>()?.RemoveCard();
        }
        CardManager.Inst.ShuffleCard();
        StartCoroutine(TurnManager.Inst.StartGameCo());
    }
}
