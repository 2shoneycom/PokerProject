using System.Collections;
using System.Collections.Generic;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Linq;
using UnityEngine;
using System;

public class HoldemResultManager
{
    HoldemGameControl _control;
    public HoldemResultManager(HoldemGameControl control)
    {
        _control = control;
    }

    public List<String> GetWinner()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemResultManager.cs 파일의 GetWinner 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        List<String> winners = new List<String>();  // 승자 리스트

        if (_control.Players.IsOneLeft)
        {
            for(int i = 0; i < HoldemGameControl.MAX_PLAYER_NUM; i++)
            {
                string pUID = _control.Players.GetPlayerUID(i);
                if (pUID == "" || _control.Players.GetPlayerState(i) == false)
                    continue;

                Debug.Log("one player left");
                winners.Add(pUID);
            }
            return winners;
        }

        HoldemHandEvaluator evaluator = new HoldemHandEvaluator(_control.Card);
        int maxRank = -1;
        int maxScore = -1;

        List<int> dealerCardIdx = _control.Card.GetDealerCardDetail().ToList();  // 딜러 카드 5장의 인덱스
    
        // 게임에 참가 중인(폴드하지 않은) 플레이어들을 파악하고
        for(int i = 0; i < HoldemGameControl.MAX_PLAYER_NUM; i++)
        {
            string pUID = _control.Players.GetPlayerUID(i);
            if (pUID == "")
                continue;

            // 족보 판단 디버그용!!!!!!
            int myRank = -1;
            int myScore = -1;

            if (_control.Players.GetPlayerState(i) == true)
            {
                // 해당 플레이어의 카드는 딜러 카드 5장 + 본인 카드 2장, 총 7장
                List<int> cardIdx = new List<int>(dealerCardIdx);
                cardIdx.Add(_control.Players.PlayerCards[i, 0]);
                cardIdx.Add(_control.Players.PlayerCards[i, 1]);

                // 7C5의 조합을 얻어내기
                var combinations = GetCombinations(cardIdx, 5);

                foreach (var comb in combinations)
                {
                    // 각 경우에서 점수를 얻고 현재 최대 점수 저장
                    evaluator.idxs = comb.ToArray();
                    var (curRank, curScore) = evaluator.EvaluateHand();

                    if (curRank > maxRank || (curRank == maxRank && curScore > maxScore))
                    {
                        maxRank = curRank;
                        maxScore = curScore;
                        winners.Clear();
                        winners.Add(pUID);
                    }
                    else if (curRank == maxRank && curScore == maxScore)
                    {
                        // 동점자 발생
                        winners.Add(pUID);
                    }

                    // 족보 판단 디버그용!!!!!!
                    if (curRank > myRank || (curRank == myRank && curScore > myScore))
                    {
                        myRank = curRank;
                        myScore = curScore;
                    }
                }

                // 족보 판단 디버그용!!!!!!!
                Debug.Log(i + "P의 족보");
                DebugLog(myRank);
                Debug.Log("Score: " + myScore);
            }
        }

        winners = winners.Distinct().ToList();

        Debug.Log("우승자의 족보는");
        DebugLog(maxRank);

        return winners;
    }

    public static IEnumerable<IEnumerable<T>> GetCombinations<T>(List<T> list, int choose)
    {
        if (choose == 0) return new List<List<T>> { new List<T>() };
        if (list.Count == 0) return new List<List<T>>();
        var head = list[0];
        var tail = list.Skip(1).ToList();

        // Include the current element
        var include = GetCombinations(tail, choose - 1)
                      .Select(comb => new List<T>(comb) { head });

        // Exclude the current element
        var exclude = GetCombinations(tail, choose);

        return include.Concat(exclude);
    }

    // 디버그용
    private void DebugLog(int rank)
    {
        if (rank == 0)
        {
            Debug.Log("하이카드 입니다.");
        }
        else if (rank == 1)
        {
            Debug.Log("원페어 입니다.");
        }
        else if (rank == 2)
        {
            Debug.Log("투페어 입니다.");
        }
        else if (rank == 3)
        {
            Debug.Log("트리플 입니다.");
        }
        else if (rank == 4)
        {
            Debug.Log("스트레이트 입니다.");
        }
        else if (rank == 5)
        {
            Debug.Log("플러쉬 입니다.");
        }
        else if (rank == 6)
        {
            Debug.Log("풀하우스 입니다.");
        }
        else if (rank == 7)
        {
            Debug.Log("포카드 입니다.");
        }
        else if (rank == 8)
        {
            Debug.Log("스트레이트 플러쉬 입니다.");
        }
        else
        {
            Debug.Log("승패 판단 오류");
        }
    }
}
