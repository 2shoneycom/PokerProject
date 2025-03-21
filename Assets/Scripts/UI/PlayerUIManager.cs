using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    // 싱글톤
    public static PlayerUIManager Inst { get; private set; }
    void Awake() => Inst = this;

    public TextMeshProUGUI[] playerMoneyTexts;
    public TextMeshProUGUI potMoneyText;

    private bool isLinked;  // 플레이어 정보가 연결됐는 지

    void Start ()
    {
        isLinked = true;
    }

    void Update ()
    {
        // Debug.Log(isLinked);
        if (isLinked)
        {
            for (int i = 0; i < 7; i++) {
                Player curplayer = PlayerManager.Inst.players[i];
                playerMoneyTexts[i].text = $"P{i}: {curplayer.seedMoney} Bet: {curplayer.currentBet}";

                if (curplayer.isActive) 
                {
                    if (i == PlayerManager.Inst.currentPlayerIndex) 
                    {
                        // 살아있는 사람 중 현재 차례면 빨간색으로 표시
                        playerMoneyTexts[i].color = Color.red;
                    }
                    else 
                    {
                        // 그냥 살아있는 사람은 하얀색
                        playerMoneyTexts[i].color = Color.white;
                    }
                }
                else 
                {
                    // 죽은 사람은 회색
                    playerMoneyTexts[i].color = Color.gray;
                }
            }
            potMoneyText.text = $"{PlayerManager.Inst.totalMoney}";
        }
    }

    public void LinkPlayerUI ()
    {
        isLinked = true;
    }
}
