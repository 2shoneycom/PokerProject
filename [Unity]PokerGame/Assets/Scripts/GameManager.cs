using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Inst {  get; private set; }
    void Awake() => Inst = this;

    [SerializeField] Transform playerPosLU;
    [SerializeField] Transform playerPosLD;
    [SerializeField] Transform playerPosRU;
    [SerializeField] Transform playerPosRD;
    [SerializeField] Transform mainplayerPos;
    [SerializeField] GameObject playerPrefabs;        // 생성할 카드 프리펩

    List<GameObject> leftPlayers;
    List<GameObject> rightPlayers;
    GameObject mainPlayer;
    public List<GameObject> players;

    public int totalPlayer;     // 중앙을 기준 플레이어로 하자
    public int mainPlayerIndex = 0;
    public int dealerPlayerIndex = 99;
    public int currentPlayerIndex;

    // Start is called before the first frame update
    void Start()
    {
        SetupPlayer(totalPlayer);
        StartGame();        // 추후 버튼 입력으로도 변동
    }

    void SetupPlayer(int totalPlayer)
    {
        leftPlayers = new List<GameObject>();
        rightPlayers = new List<GameObject>();
        players = new List<GameObject>();

        if(totalPlayer == 1)
        {
            mainPlayerIndex = 0;
        }
        else if(totalPlayer == 2)
        {
            rightPlayers.Add(Instantiate(playerPrefabs, (playerPosRD.position + playerPosRU.position) / 2, Quaternion.identity));
            mainPlayerIndex = 1;
        }
        else
        {
            int rC = (totalPlayer - 1) / 2;
            float newY = CalPlayerYDis(rC);
            for(int i = 0; i < rC; i++)
            {
                Vector3 newPos = new Vector3(playerPosRU.position.x, playerPosRU.position.y - newY * i, playerPosRU.position.z);
                rightPlayers.Add(Instantiate(playerPrefabs, newPos, Quaternion.identity));
            }
            mainPlayerIndex = rC;
            int lC = totalPlayer - 1 - rC;
            newY = CalPlayerYDis(lC);
            for (int i = 0; i < lC; i++)
            {
                Vector3 newPos = new Vector3(playerPosLU.position.x, playerPosLU.position.y - newY * i, playerPosLU.position.z);
                leftPlayers.Add(Instantiate(playerPrefabs, newPos, Quaternion.identity));
            }

        }
        mainPlayer = Instantiate(playerPrefabs, mainplayerPos.position, Quaternion.identity);

        for (int i = 0; i < rightPlayers.Count; i++)
            players.Add(rightPlayers[i]);
        players.Add(mainPlayer);
        for (int i = leftPlayers.Count - 1; i >= 0; i--)
            players.Add(leftPlayers[i]);
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR            // 유니티 에디터에서만 치트 적용
        InputCheatKey();
#endif
    }

    float CalPlayerYDis(int count)
    {
        float upside = playerPosRU.position.y - playerPosRD.position.y;
        if (count == 1) return 0;
        else return upside / (count - 1);
    }

    void InputCheatKey()    // 테스트용 치트
    {                       // 1은 최대 2번, 2는 최대 5번만 누를것.
        if (Input.GetKeyDown(KeyCode.Keypad1))
            TurnManager.OnAddCard?.Invoke(mainPlayerIndex);
        if (Input.GetKeyDown(KeyCode.Keypad2))
            TurnManager.OnAddCard?.Invoke(dealerPlayerIndex);
        if (Input.GetKeyDown(KeyCode.Keypad3))
            TurnManager.Inst.EndTurn();
    }

    public void StartGame()
    {
        StartCoroutine(TurnManager.Inst.StartGameCo());
    }
}
