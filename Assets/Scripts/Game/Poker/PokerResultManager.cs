using System.Collections;
using System.Collections.Generic;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Linq;
using UnityEngine;
using System;

public class PokerResultManager
{
    public string GetWinner(int cardLen)
    {
        List<String> winners = new List<String>();  // 승자 리스트

        if (PokerGameControl.Players.IsOneLeft)
        {
            for (int i = 0; i < PokerGameControl.MAX_PLAYER_NUM; i++)
            {
                string pUID = PokerGameControl.Players.GetPlayerUID(i);
                if (pUID == "" || PokerGameControl.Players.GetPlayerState(i) == false)
                    continue;

                Debug.Log("one player left");
                winners.Add(pUID);
            }
            return winners[0];
        }

        PokerHandEvaluator evaluator = new PokerHandEvaluator();
        int maxRank = -1;
        int maxScore = -1;

        // 게임에 참가 중인(폴드하지 않은) 플레이어들을 파악하고
        for (int i = 0; i < PokerGameControl.MAX_PLAYER_NUM; i++)
        {
            string pUID = PokerGameControl.Players.GetPlayerUID(i);
            if (pUID == "" || PokerGameControl.Players.GetPlayerState(i) == false)
                continue;

            // 족보 판단 디버그용!!!!!!
            int myRank = -1;
            int myScore = -1;

            List<int> cardIdx = new List<int>();

            if (cardLen == PokerGameControl.MAX_PLAYER_NUM)
            {
                for (int j = 0; j < PokerGameControl.MAX_PLAYER_NUM; j++)
                {
                    if (j == 3) continue;

                    cardIdx.Add(PokerGameControl.Players.GetPlayerCardDetail(i, j));
                }
            }
            else
            {
                for (int j = 0; j < cardLen; j++)
                {
                    if (j == 0 || j == 1 || j == 3) continue;

                    cardIdx.Add(PokerGameControl.Players.GetPlayerCardDetail(i, j));
                }
            }

            if (cardLen < PokerGameControl.MAX_PLAYER_NUM)
            {
                evaluator.idxs = cardIdx;
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
            else
            {
                // 7C5의 조합을 얻어내기
                var combinations = GetCombinations(cardIdx, 5);

                foreach (var comb in combinations)
                {
                    // 각 경우에서 점수를 얻고 현재 최대 점수 저장
                    evaluator.idxs = comb.ToList<int>();
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
            }

            // 족보 판단 디버그용!!!!!!!
            Debug.Log(i + "P의 족보");
            DebugLog(myRank);
            Debug.Log("Score: " + myScore);
        }

        winners = winners.Distinct().ToList();

        // 세븐에서는 동점자가 있을 시 문양 판단으로 단 1명의 승자를 가려야함
        return RealWinnerDecider(winners);

        Debug.Log("우승자의 족보는");
        DebugLog(maxRank);

        return winners[0];
    }


    // 문양 판단으로 단 1명의 승자 가리는 함수
    public string RealWinnerDecider(List<String> winners)
    {
        return null;
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
