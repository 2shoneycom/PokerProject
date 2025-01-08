using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerHandEvaluator : MonoBehaviour
{
    public int[] idxs = new int[5];    // 이 코드를 호출 할 때, 여기에 카드 5장의 인덱스를 줄거임. 그래야함.
    [SerializeField]
    private char[] shapes = new char[5]; // idxs 배열을 Card System 쪽에 있는 무늬 판별기에 넣고 무늬를 얻어옴.
    [SerializeField]
    private int[] nums = new int[5]; // idx 배열을 Card System 쪽에 있는 숫자 판별기에 넣고 숫자를 얻어옴.
    
    void Start()
    {
        // Start에서 Card System 쪽에 있는 숫자를 보고 무늬, 숫자 판단하는 거 가져오기
        
    }

    private bool IsFlush ()
    {
        bool isflush = true;

        // 5 장을 순회하는 동안 한 번이라도 서로 다르면 false
        for (int i = 0; i < 4; i++) {
            if (shapes[i] != shapes[i+1]) {
                isflush = false;
            }
        }

        if (isflush) {
            Debug.Log("플러쉬입니다.");
        }
        else {
            Debug.Log("플러쉬가 아닙니다.");
        }

        return isflush;
    }
}
