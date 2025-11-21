using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;

public class PokerHandEvaluator
{
    PokerGameControl _control;

    public List<int> idxs;
    private List<Tuple<int, char>> numsAndShapes = new List<Tuple<int, char>>();
    private List<char> shapes = new List<char>();   // idxs 배열을 Card System 쪽에 있는 무늬 판별기에 넣고 무늬를 얻어옴.
    private List<int> nums = new List<int>();        // idxs 배열을 Card System 쪽에 있는 숫자 판별기에 넣고 숫자를 얻어옴.
    private int handRank;   // 이 5장의 족보 순위 ex) 0: highcard, 1: onepair ... 8: straight flush
    private float handScore; // 해당 족보안에서의 카드 점수 (동점 판별용)
    private bool is_flush;
    private bool is_straight;
    int cardlen;

    public PokerHandEvaluator(PokerGameControl control)
    {
        _control = control;
    }

    public Tuple<int, float> EvaluateHand()
    {
        cardlen = idxs.Count;
        Debug.Log($"cardlen(=idxs.Count)는 {cardlen}개");

        // Start에서 Card System 쪽에 있는 숫자를 보고 문양 숫자 판단하는 거 가져오는 코드
        numsAndShapes.Clear();
        for (int i = 0; i < cardlen; i++)
        {
            numsAndShapes.Add(Tuple.Create(_control.Card.GetCardNum(idxs[i]), _control.Card.GetCardShape(idxs[i])));
        }

        handRank = -1;
        handScore = 0;

        // 족보 판단을 두 번 행할꺼임 (A 때문에..)
        for (int i = 0; i < 2; i++)
        {
            if (i == 1)
            {
                for (int j = 0; j < cardlen; j++)
                {
                    // 두 번째 족보 체크에서는 A 카드를 14로 여김
                    if (numsAndShapes[j].Item1 == 1)
                    {
                        numsAndShapes[j] = Tuple.Create(14, numsAndShapes[j].Item2);
                    }
                }
            }

            // nums 내림차순 정렬
            numsAndShapes.Sort();
            numsAndShapes.Reverse();

            nums.Clear();
            shapes.Clear();
            for (int j = 0; j < cardlen; j++)
            {
                nums.Add(numsAndShapes[j].Item1);
                shapes.Add(numsAndShapes[j].Item2);
            }

            // 디버그
            for (int j = 0; j < cardlen; j++)
            {
                Debug.Log($"{j}번째 카드 -> {nums[j]}");
            }

            // 플러쉬, 스트레이트 여부는 미리 구해놓기
            if (cardlen >= 5)
            {
                SetFlush();
                SetStraight();
            }

            // 본격 족보 판단
            var (curRank, curScore) = GetRank();

            if (curRank > handRank)
            {
                handScore = curScore;
            }
            else if (curRank == handRank)
            {
                handScore = Math.Max(handScore, curScore);
            }
            handRank = Math.Max(handRank, curRank);

            // Debug.Log("handRank: " + handRank + ", handScore: " + handScore);
        }

        return Tuple.Create(handRank, handScore);
    }

    private void SetFlush()
    {
        is_flush = true;
        // 5 장을 순회하는 동안 한 번이라도 서로 다르면 false
        for (int i = 0; i < cardlen - 1; i++)
        {
            if (shapes[i] != shapes[i + 1])
            {
                is_flush = false;
            }
        }
    }

    private void SetStraight()
    {
        is_straight = false;
        // ex) 5,4,3,2,1
        if (nums[0] == nums[1] + 1 && nums[1] == nums[2] + 1
        && nums[2] == nums[3] + 1 && nums[3] == nums[4] + 1)
        {
            is_straight = true;
        }
    }

    private Tuple<int, float> GetRank()
    {
        float scr = -1;

        scr = cardlen >= 5 ? IsStraightFlush() : scr;
        if (scr > 0)
        {
            // Debug.Log("스트레이트 플러쉬입니다.");
            return Tuple.Create(8, scr);
        }

        scr = cardlen >= 4 ? IsFourCard() : scr;
        if (scr > 0)
        {
            // Debug.Log("포카드입니다.");
            return Tuple.Create(7, scr);
        }
        Debug.Log("IsFourCard 무사 실행 완료");

        scr = cardlen >= 5 ? IsFullHouse() : scr;
        if (scr > 0)
        {
            // Debug.Log("풀하우스입니다.");
            return Tuple.Create(6, scr);
        }

        scr = cardlen >= 5 ? IsFlush() : scr;
        if (scr > 0)
        {
            // Debug.Log("플러쉬입니다.");
            return Tuple.Create(5, scr);
        }

        scr = cardlen >= 5 ? IsStraight() : scr;
        if (scr > 0)
        {
            // Debug.Log("스트레이트입니다.");
            return Tuple.Create(4, scr);
        }

        scr = cardlen >= 3 ? IsTriple() : scr;
        if (scr > 0)
        {
            // Debug.Log("트리플입니다.");
            return Tuple.Create(3, scr);
        }
        Debug.Log("IsTriple 무사 실행 완료");

        scr = cardlen >= 4 ? IsTwoPair() : scr;
        if (scr > 0)
        {
            // Debug.Log("투페어입니다.");
            return Tuple.Create(2, scr);
        }
        Debug.Log("IsTwoPair 무사 실행 완료");

        scr = cardlen >= 2 ? IsOnePair() : scr;
        if (scr > 0)
        {
            // Debug.Log("원페어입니다.");
            return Tuple.Create(1, scr);
        }
        Debug.Log("IsOnePair 무사 실행 완료");

        // 하이 카드
        scr = 0;
        for (int i = 0; i < cardlen; i++)
        {
            scr *= 100;
            scr += nums[i];
        }
        scr += (float)(0.1 * ShapesToInt(shapes[0]));
        return Tuple.Create(0, scr);
    }

    // 5장 이상일 때 들어올 수 있음
    private float IsStraightFlush()
    {
        float scr = -1;

        if (is_flush && is_straight)
        {
            scr = nums[0];    // 스트레이트 계열은 제일 높은 한 장만 보면 됨
            scr += (float)(0.1 * ShapesToInt(shapes[0]));    // 플러쉬 계열은 한 장의 문양만 보면 됨
        }

        return scr;
    }

    // 4장 이상일 때 들어올 수 있음
    private float IsFourCard()
    {
        float scr = -1;

        if (nums[0] == nums[1] && nums[1] == nums[2] && nums[2] == nums[3])
        {
            // 앞의 4장이 같은 경우
            scr = nums[0];    // 포카드는 포카드를 이루는 한 장을 비교하면 됨
        }
        else if (cardlen >= 5 && nums[1] == nums[2] && nums[2] == nums[3] && nums[3] == nums[4])
        {
            // 뒤의 4장이 같은 경우
            scr = nums[4];    // 포카드는 포카드를 이루는 한 장을 비교하면 됨
        }

        // 포카드는 동점이 나올 수 없음! (문양 판별 필요없음)
        return scr;
    }

    // 5장 이상일 때 들어올 수 있음
    private float IsFullHouse()
    {
        float scr = -1;

        if (nums[0] == nums[1] && nums[1] == nums[2] && nums[3] == nums[4])
        {
            // 앞의 3장이 같고 뒤의 2장이 같은 경우
            scr = nums[0];    // 풀하우스는 트리플을 이루는 한 장을 비교하면 됨
        }
        else if (nums[0] == nums[1] && nums[2] == nums[3] && nums[3] == nums[4])
        {
            // 앞의 2장이 같고 뒤의 3장이 같은 경우
            scr = nums[4];    // 풀하우스는 트리플을 이루는 한 장을 비교하면 됨
        }

        // 풀하우스는 동점이 나올 수 없음! (문양 판별 필요없음)
        return scr;
    }

    // 5장 이상일 때 들어올 수 있음
    private float IsFlush()
    {
        float scr = -1;
        if (is_flush)
        {
            scr = 0;
            for (int i = 0; i < 5; i++)
            {
                // 플러쉬끼리는 하이카드 방식으로 비교해야함
                scr *= 100;
                scr += nums[i];
            }
        }
        scr += (float)(0.1 * ShapesToInt(shapes[0]));    // 플러쉬 계열은 한 장의 문양만 보면 됨

        return scr;
    }

    // 5장 이상일 때 들어올 수 있음
    private float IsStraight()
    {
        float scr = -1;
        if (is_straight)
        {
            scr = nums[0];    // 스트레이트는 제일 높은 한 장만 비교하면 됨
        }
        scr += (float)(0.1 * ShapesToInt(shapes[0]));    // 제일 높은 한 장의 문양만 보면 됨 (아마?)
        return scr;
    }

    // 3장 이상일 때 들어올 수 있음
    private float IsTriple()
    {
        float scr = -1;

        if (nums[0] == nums[1] && nums[1] == nums[2])
        {
            // 앞의 3장이 같은 경우
            scr = nums[0];    // 트리플은 트리플을 이루는 카드 한 장만 비교하면 됨
        }
        else if (cardlen >= 4 && nums[1] == nums[2] && nums[2] == nums[3])
        {
            // 중간 3장이 같은 경우
            scr = nums[1];    // 트리플은 트리플을 이루는 카드 한 장만 비교하면 됨
        }
        else if (cardlen >= 5 && nums[2] == nums[3] && nums[3] == nums[4])
        {
            // 뒤의 3장이 같은 경우
            scr = nums[2];    // 트리플은 트리플을 이루는 카드 한 장만 비교하면 됨
        }

        // 트리플은 동점이 나올 수 없음! (문양 판별 필요없음)
        return scr;
    }

    // 4장 이상일 때 들어올 수 있음
    private float IsTwoPair()
    {
        float scr = -1;

        if (nums[0] == nums[1] && nums[2] == nums[3])
        {
            // 앞의 두 쌍이 같은 경우 ex) 7,7,4,4,1
            scr = 10000 * nums[0] + 100 * nums[2];
            scr += cardlen >= 5 ? nums[4] : 0;  // 5장인 경우에만 마지막 카드 점수 더하기

            scr += (float)(0.1 * Math.Max(ShapesToInt(shapes[0]), ShapesToInt(shapes[1])));  // 높은 쌍(7,7)에서 높은 문양을 점수로 사용
        }
        else if (cardlen >= 5 && nums[0] == nums[1] && nums[3] == nums[4])
        {
            // 앞의 한 쌍, 뒤의 한 쌍이 같은 경우 ex) 7,7,4,1,1
            scr = 10000 * nums[0] + 100 * nums[3] + nums[2];
            scr += (float)(0.1 * Math.Max(ShapesToInt(shapes[0]), ShapesToInt(shapes[1])));  // 높은 쌍(7,7)에서 높은 문양을 점수로 사용
        }
        else if (cardlen >= 5 && nums[1] == nums[2] && nums[3] == nums[4])
        {
            // 뒤의 두 쌍이 같은 경우 ex) 7,4,4,1,1
            scr = 10000 * nums[1] + 100 * nums[3] + nums[0];
            scr += (float)(0.1 * Math.Max(ShapesToInt(shapes[1]), ShapesToInt(shapes[2])));  // 높은 쌍(4,4)에서 높은 문양을 점수로 사용
        }

        return scr;
    }

    // 2장 이상일 때 들어올 수 있음
    private float IsOnePair()
    {
        float scr = -1;

        if (nums[0] == nums[1])
        {
            // ex) 7,7,5,3,1
            scr = 1000000 * nums[0];
            scr += cardlen >= 3 ? 10000 * nums[2] : 0;
            scr += cardlen >= 4 ? 100 * nums[3] : 0;
            scr += cardlen >= 5 ? nums[4] : 0;

            scr += (float)(0.1 * Math.Max(ShapesToInt(shapes[0]), ShapesToInt(shapes[1])));  // 페어에서 높은 문양을 점수로 사용
        }
        else if (cardlen >= 3 && nums[1] == nums[2])
        {
            // ex) 7,5,5,3,1
            scr = 1000000 * nums[1] + 10000 * nums[0];
            scr += cardlen >= 4 ? 100 * nums[3] : 0;
            scr += cardlen >= 5 ? nums[4] : 0;

            scr += (float)(0.1 * Math.Max(ShapesToInt(shapes[1]), ShapesToInt(shapes[2])));  // 페어에서 높은 문양을 점수로 사용
        }
        else if (cardlen >= 4 && nums[2] == nums[3])
        {
            // ex) 7,5,3,3,1
            scr = 1000000 * nums[2] + 10000 * nums[0] + 100 * nums[1];
            scr += cardlen >= 5 ? nums[4] : 0;

            scr += (float)(0.1 * Math.Max(ShapesToInt(shapes[2]), ShapesToInt(shapes[3])));  // 페어에서 높은 문양을 점수로 사용
        }
        else if (cardlen >= 5 && nums[3] == nums[4])
        {
            // ex) 13,10,9,5,5
            scr = 1000000 * nums[3] + 10000 * nums[0] + 100 * nums[1] + nums[2];
            scr += (float)(0.1 * Math.Max(ShapesToInt(shapes[3]), ShapesToInt(shapes[4])));  // 페어에서 높은 문양을 점수로 사용
        }

        return scr;
    }

    public int ShapesToInt(char shape)
    {
        int retVal = -1;

        switch (shape)
        {
            case 'S':
                retVal = 4;
                break;
            case 'D':
                retVal = 3;
                break;
            case 'H':
                retVal = 2;
                break;
            case 'C':
                retVal = 1;
                break;
        }

        return retVal;
    }
}