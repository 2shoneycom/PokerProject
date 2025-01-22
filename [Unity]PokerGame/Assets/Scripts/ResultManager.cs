using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultManager : MonoBehaviour
{
    void Start()
    {
        
    }

    public void CheckWinner ()
    {
        /*
        게임에 참가 중인(폴드하지 않은) 플레이어들을 파악하고
        */

        /*
        한 명씩 돌아가면서 딜러 카드와 플레이어 카드 총 7장을 보고 족보를 판단
        // 한 명씩 돌아가면서
            // 딜러 카드와 플레이어 카드, 총 7장의 인덱스를 얻어온다.
            // 이 7장 중 5장을 고른다. 7C5
                // 각 경우에서 PokerHandEvaluator.cs 내의 함수를 이용해 handRank, handScore를 얻는다.
            
            // 자신이 만들 수 있는 가장 높은 족보의 점수를 저장해놓는다.
        */

        /*
        가장 점수가 높은 사람을 승리자로 뽑아서 반환
        */
    }

    private void ChooseFive ()
    {
        
    }
}
