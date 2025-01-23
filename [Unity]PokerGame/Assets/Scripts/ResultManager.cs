using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResultManager : MonoBehaviour
{
    public static ResultManager Inst { get; private set; }
    void Awake() => Inst = this;
    
    public List<Player> GetWinner ()
    {
        var evaluator = new PokerHandEvaluator();

        List<Player> winners = new List<Player>();  // 승자 리스트
        int maxRank = -1;           
        int maxScore = -1;          

        List<int> dealerCardIdx = CardManager.Inst.dealerCards.Select(card => card.myCardIndex).ToList();  // 딜러 카드 5장의 인덱스

        // 게임에 참가 중인(폴드하지 않은) 플레이어들을 파악하고
        foreach (var curplayerObject in PlayerManager.Inst.players)
        {
            var curplayer = curplayerObject.GetComponent<Player>();

            if (curplayer.IsActive) 
            {
                // 해당 플레이어의 카드는 딜러 카드 5장 + 본인 카드 2장, 총 7장
                List<int> cardIdx = new List<int>(dealerCardIdx);
                cardIdx.Add(curplayer.myCards[0].myCardIndex);
                cardIdx.Add(curplayer.myCards[1].myCardIndex);

                // 7C5의 조합을 얻어내기
                var combinations = GetCombinations(cardIdx, 5);

                foreach (var comb in combinations) 
                {
                    // 각 경우에서 점수를 얻고 현재 최대 점수 저장
                    evaluator.idxs = comb.ToArray();
                    var (curRank,curScore) = evaluator.EvaluateHand();

                    if (curRank > maxRank || (curRank == maxRank && curScore > maxScore))
                    {
                        maxRank = curRank;
                        maxScore = curScore;
                        winners.Clear();
                        winners.Add(curplayer);
                    }
                    else if (curRank == maxRank && curScore == maxScore)
                    {
                        // 동점자 발생
                        winners.Add(curplayer);
                    }
                }
            }
        }

        winners = winners.Distinct().ToList();

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

    private void DebugLog (int rank)
    {
        if (rank == 0) {
            Debug.Log("하이카드 입니다.");
        }
        else if (rank == 1) {
            Debug.Log("원페어 입니다.");
        }
        else if (rank == 2) {
            Debug.Log("투페어 입니다.");
        }
        else if (rank == 3) {
            Debug.Log("트리플 입니다.");
        }
        else if (rank == 4) {
            Debug.Log("스트레이트 입니다.");
        }
        else if (rank == 5) {
            Debug.Log("플러쉬 입니다.");
        }
        else if (rank == 6) {
            Debug.Log("풀하우스 입니다.");
        }
        else if (rank == 7) {
            Debug.Log("포카드 입니다.");
        }
        else if (rank == 8) {
            Debug.Log("스트레이트 플러쉬 입니다.");
        }
        else {
            Debug.Log("승패 판단 오류");
        }
    }
}
