using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerHandEvaluator : MonoBehaviour
{
    public int[] idxs = new int[5];         // 이 코드를 호출 할 때, 여기에 카드 5장의 인덱스를 줄거임. 그래야함.
    [SerializeField]
    private char[] shapes = new char[5];    // idxs 배열을 Card System 쪽에 있는 무늬 판별기에 넣고 무늬를 얻어옴.
    [SerializeField]
    private int[] nums = new int[5];        // idx 배열을 Card System 쪽에 있는 숫자 판별기에 넣고 숫자를 얻어옴.
    private int handRank;   // 이 5장의 족보 순위 ex) 0: highcard, 1: onepair ... 8: straight flush
    private int handScore; // 해당 족보안에서의 카드 점수 (동점 판별용)
    private bool is_flush;
    private bool is_straight;
    
    public Tuple<int,int> EvalueateHand ()
    {
        /*
        Start에서 Card System 쪽에 있는 숫자를 보고 무늬, 숫자 판단하는 거 가져오는 코드
        */

        handRank = -1;
        handScore = 0;

        // 족보 판단을 두 번 행할꺼임 (A 때문에..)
        for (int i = 0; i < 2; i++) {
            if (i == 1) {
                for (int j = 0; j < 5; j++) {
                    // 두 번째 족보 체크에서는 A 카드를 14로 여김
                    if (nums[j]==1) {
                        nums[j]=14;
                    }
                }
            }

            // nums 내림차순 정렬
            Array.Sort(nums);
            Array.Reverse(nums);

            // 플러쉬, 스트레이트 여부는 미리 구해놓기
            SetFlush();
            SetStraight();

            // 본격 족보 판단
            var (curRank, curScore) = GetRank();

            if (curRank > handRank) {
                handScore = curScore;
            }
            else if (curRank == handRank) {
                handScore = Math.Max(handScore, curScore);
            }
            handRank = Math.Max(handRank, curRank);

            Debug.Log("handRank: " + handRank + ", handScore: " + handScore);
        }

        return Tuple.Create(handRank, handScore);
    }

    private void SetFlush ()
    {
        is_flush = true;
        // 5 장을 순회하는 동안 한 번이라도 서로 다르면 false
        for (int i = 0; i < 4; i++) {
            if (shapes[i] != shapes[i+1]) {
                is_flush = false;
            }
        }
    }

    private void SetStraight ()
    {
        is_straight = false;
        // ex) 5,4,3,2,1
        if (nums[0]==nums[1]+1 && nums[1]==nums[2]+1
        && nums[2]==nums[3]+1 && nums[3]==nums[4]+1) {
            is_straight = true;
        }
    }

    private Tuple<int,int> GetRank ()
    {
        int scr = -1;

        scr = IsStraightFlush();
        if (scr > 0) {
            Debug.Log("스트레이트 플러쉬입니다.");
            return Tuple.Create(8,scr);
        }
        
        scr = IsFourCard();
        if (scr > 0) {
            Debug.Log("포카드입니다.");
            return Tuple.Create(7,scr);
        }

        scr = IsFullHouse();
        if (scr > 0) {
            Debug.Log("풀하우스입니다.");
            return Tuple.Create(6,scr);
        }

        scr = IsFlush();
        if (scr > 0) {
            Debug.Log("플러쉬입니다.");
            return Tuple.Create(5,scr);
        }

        scr = IsStraight();
        if (scr > 0) {
            Debug.Log("스트레이트입니다.");
            return Tuple.Create(4,scr);
        }

        scr = IsTriple();
        if (scr > 0) {
            Debug.Log("트리플입니다.");
            return Tuple.Create(3,scr);
        }

        scr = IsTwoPair();
        if (scr > 0) {
            Debug.Log("투페어입니다.");
            return Tuple.Create(2,scr);
        }

        scr = IsOnePair();
        if (scr > 0) {
            Debug.Log("원페어입니다.");
            return Tuple.Create(1,scr);
        }

        // 하이 카드
        Debug.Log("하이카드입니다.");
        scr = 0;
        for (int i = 0; i < 5; i++) {
            scr *= 100;
            scr += nums[i];
        }
        return Tuple.Create(0,scr);
    }

    private int IsStraightFlush ()
    {
        int scr = -1;
        
        if (is_flush && is_straight) {
            scr = nums[0];    // 스트레이트 계열은 제일 높은 한 장만 보면 됨
        }

        return scr;
    }

    private int IsFourCard ()
    {
        int scr = -1;

        if (nums[0]==nums[1] && nums[1]==nums[2] && nums[2]==nums[3]) {
            // 앞의 4장이 같은 경우
            scr = nums[0];    // 포카드는 포카드를 이루는 한 장을 비교하면 됨
        }
        else if (nums[1]==nums[2] && nums[2]==nums[3] && nums[3]==nums[4]) {
            // 뒤의 4장이 같은 경우
            scr = nums[4];    // 포카드는 포카드를 이루는 한 장을 비교하면 됨
        }

        return scr;
    }

    private int IsFullHouse ()
    {
        int scr = -1;

        if (nums[0]==nums[1] && nums[1]==nums[2] && nums[3]==nums[4]) {
            // 앞의 3장이 같고 뒤의 2장이 같은 경우
            scr = nums[0];    // 풀하우스는 트리플을 이루는 한 장을 비교하면 됨
        }
        else if (nums[0]==nums[1] && nums[2]==nums[3] && nums[3]==nums[4]) {
            // 앞의 2장이 같고 뒤의 3장이 같은 경우
            scr = nums[4];    // 풀하우스는 트리플을 이루는 한 장을 비교하면 됨
        }

        return scr;
    }

    private int IsFlush ()
    {
        int scr = -1;
        if (is_flush) {
            for (int i = 0; i < 5; i++) {
                // 플러쉬끼리는 하이카드 방식으로 비교해야함
                scr *= 100;
                scr += nums[i];
            }
        }
        return scr;
    }

    private int IsStraight ()
    {
        int scr = -1;
        if (is_straight) {
            scr = nums[0];    // 스트레이트는 제일 높은 한 장만 비교하면 됨
        }
        return scr;
    }

    private int IsTriple ()
    {
        int scr = -1;

        if (nums[0]==nums[1] && nums[1]==nums[2]) {
            // 앞의 3장이 같은 경우
            scr = nums[0];    // 트리플은 트리플을 이루는 카드 한 장만 비교하면 됨
        }
        else if (nums[1]==nums[2] && nums[2]==nums[3]) {
            // 중간 3장이 같은 경우
            scr = nums[1];    // 트리플은 트리플을 이루는 카드 한 장만 비교하면 됨
        }
        else if (nums[2]==nums[3] && nums[3]==nums[4]) {
            // 뒤의 3장이 같은 경우
            scr = nums[2];    // 트리플은 트리플을 이루는 카드 한 장만 비교하면 됨
        }

        return scr;
    }

    private int IsTwoPair ()
    {
        int scr = -1;

        if (nums[0]==nums[1] && nums[2]==nums[3]) {
            // 앞의 두 쌍이 같은 경우 ex) 7,7,4,4,1
            scr = 10000*nums[0]+100*nums[2]+nums[4];
        }
        else if (nums[0]==nums[1] && nums[3]==nums[4]) {
            // 앞의 한 쌍, 뒤의 한 쌍이 같은 경우 ex) 7,7,4,1,1
            scr = 10000*nums[0]+100*nums[3]+nums[2];
        }
        else if (nums[1]==nums[2] && nums[3]==nums[4]) {
            // 뒤의 두 쌍이 같은 경우 ex) 7,4,4,1,1
            scr = 10000*nums[1]+100*nums[3]+nums[0];
        }

        return scr;
    }

    private int IsOnePair ()
    {
        int scr = -1;

        if (nums[0]==nums[1]) {
            // ex) 7,7,5,3,1
            scr = 1000000*nums[0]+10000*nums[2]+100*nums[3]+nums[4];
        }
        else if (nums[1]==nums[2]) {
            // ex) 7,5,5,3,1
            scr = 1000000*nums[1]+10000*nums[0]+100*nums[3]+nums[4];
        }
        else if (nums[2]==nums[3]) {
            // ex) 7,5,3,3,1
            scr = 1000000*nums[2]+10000*nums[0]+100*nums[1]+nums[4];
        }
        else if (nums[3]==nums[4]) {
            // ex) 7,5,3,1,1
            scr = 100000000*nums[3]+10000*nums[0]+100*nums[1]+nums[2];
        }

        return scr;
    }
}
