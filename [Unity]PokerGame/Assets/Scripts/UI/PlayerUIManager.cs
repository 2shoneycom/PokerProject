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
                playerMoneyTexts[i].text = $"P{i}: {PlayerManager.Inst.players[i].SeedMoney} Bet: {PlayerManager.Inst.players[i].CurrentBet}";
            }
            potMoneyText.text = $"{PlayerManager.Inst.totalMoney}";
        }
    }

    public void LinkPlayerUI ()
    {
        isLinked = true;
    }
}
