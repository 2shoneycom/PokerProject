using System;
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
    private int handRank = -1;   // 이 5장의 족보 순위 ex) 0: highcard, 1: onepair ... 8: straight flush
    private int handScore = 0;  // 해당 족보안에서의 카드 점수 (동점 판별용)
    private bool is_flush = true;
    private bool is_straight = false;
    
    void Start()
    {
        // Start에서 Card System 쪽에 있는 숫자를 보고 무늬, 숫자 판단하는 거 가져오기
        
        // nums 내림차순 정렬
        Array.Sort(nums);
        Array.Reverse(nums);

        // 플러쉬, 스트레이트 여부는 미리 구해놓기
        CheckFlush();
        CheckStraight();

        // 본격 족보 판단
        EvaluteHand();

        Debug.Log("handRank: " + handRank + ", handScore: " + handScore);
    }

    private void EvaluteHand ()
    {
        if (IsStraightFlush()) {
            Debug.Log("스트레이트 플러쉬입니다.");
            return;
        }
        else if (IsFourCard()) {
            Debug.Log("포카드입니다.");
            return;
        }
        else if (IsFullHouse()) {
            Debug.Log("풀하우스입니다.");
            return;
        }
        else if (IsFlush()) {
            Debug.Log("플러쉬입니다.");
            return;
        }
        else if (IsStraight()) {
            Debug.Log("스트레이트입니다.");
            return;
        }
        else if (IsTriple()) {
            Debug.Log("트리플입니다.");
            return;
        }
        else if (IsTwoPair()) {
            Debug.Log("투페어입니다.");
            return;
        }
        else if (IsOnePair()) {
            Debug.Log("원페어입니다.");
            return;
        }
        else {
            // 하이 카드
            Debug.Log("하이카드입니다.");
            handRank = 0;
            for (int i = 0; i < 5; i++) {
                handScore *= 10;
                handScore += nums[i];
            }
        }
    }

    private void CheckFlush ()
    {
        is_flush = true;
        // 5 장을 순회하는 동안 한 번이라도 서로 다르면 false
        for (int i = 0; i < 4; i++) {
            if (shapes[i] != shapes[i+1]) {
                is_flush = false;
            }
        }
    }

    private void CheckStraight ()
    {
        if (nums[0]==nums[1]+1 && nums[1]==nums[2]+1
        && nums[2]==nums[3]+1 && nums[3]==nums[4]+1) {
            is_straight = true;
        }
    }

    private bool IsStraightFlush ()
    {
        bool tf = false;
        
        if (is_flush && is_straight) {
            tf = true;
            handRank = 8;
            handScore = nums[0];    // 스트레이트 계열은 제일 높은 한 장만 보면 됨
        }

        return tf;
    }

    private bool IsFourCard ()
    {
        bool tf = false;

        if (nums[0]==nums[1] && nums[1]==nums[2] && nums[2]==nums[3]) {
            // 앞의 4장이 같은 경우
            tf = true;
            handRank = 7;
            handScore = nums[0];    // 포카드는 포카드를 이루는 한 장을 비교하면 됨
        }
        else if (nums[1]==nums[2] && nums[2]==nums[3] && nums[3]==nums[4]) {
            // 뒤의 4장이 같은 경우
            tf = true;
            handRank = 7;
            handScore = nums[4];    // 포카드는 포카드를 이루는 한 장을 비교하면 됨
        }

        return tf;
    }

    private bool IsFullHouse ()
    {
        bool tf = false;

        if (nums[0]==nums[1] && nums[1]==nums[2] && nums[3]==nums[4]) {
            // 앞의 3장이 같고 뒤의 2장이 같은 경우
            tf = true;
            handRank = 6;
            handScore = nums[0];    // 풀하우스는 트리플을 이루는 한 장을 비교하면 됨
        }
        else if (nums[0]==nums[1] && nums[2]==nums[3] && nums[3]==nums[4]) {
            // 앞의 2장이 같고 뒤의 3장이 같은 경우
            tf = true;
            handRank = 6;
            handScore = nums[4];    // 풀하우스는 트리플을 이루는 한 장을 비교하면 됨
        }

        return tf;
    }

    private bool IsFlush ()
    {
        bool tf = false;

        if (is_flush) {
            tf = true;
            handRank = 5;
            for (int i = 0; i < 5; i++) {
                // 플러쉬끼리는 하이카드 방식으로 비교해야함
                handScore *= 10;
                handScore += nums[i];
            }
        }

        return tf;
    }

    private bool IsStraight ()
    {
        bool tf = false;

        if (is_straight) {
            tf = true;
            handRank = 4;
            handScore = nums[0];    // 스트레이트는 제일 높은 한 장만 비교하면 됨
        }

        return tf;
    }

    private bool IsTriple ()
    {
        bool tf = false;

        if (nums[0]==nums[1] && nums[1]==nums[2]) {
            // 앞의 3장이 같은 경우
            tf = true;
            handRank = 3;
            handScore = nums[0];    // 트리플은 트리플을 이루는 카드 한 장만 비교하면 됨
        }
        else if (nums[1]==nums[2] && nums[2]==nums[3]) {
            // 중간 3장이 같은 경우
            tf = true;
            handRank = 3;
            handScore = nums[1];    // 트리플은 트리플을 이루는 카드 한 장만 비교하면 됨
        }
        else if (nums[2]==nums[3] && nums[3]==nums[4]) {
            // 뒤의 3장이 같은 경우
            tf = true;
            handRank = 3;
            handScore = nums[2];    // 트리플은 트리플을 이루는 카드 한 장만 비교하면 됨
        }

        return tf;
    }

    private bool IsTwoPair ()
    {
        bool tf = false;

        if (nums[0]==nums[1] && nums[2]==nums[3]) {
            // 앞의 두 쌍이 같은 경우 ex) 7,7,4,4,1
            tf = true;
            handRank = 2;
            handScore = 100*nums[0]+10*nums[2]+nums[4];
        }
        else if (nums[0]==nums[1] && nums[3]==nums[4]) {
            // 앞의 한 쌍, 뒤의 한 쌍이 같은 경우 ex) 7,7,4,1,1
            tf = true;
            handRank = 2;
            handScore = 100*nums[0]+10*nums[3]+nums[2];
        }
        else if (nums[1]==nums[2] && nums[3]==nums[4]) {
            // 뒤의 두 쌍이 같은 경우 ex) 7,4,4,1,1
            tf = true;
            handRank = 2;
            handScore = 100*nums[1]+10*nums[3]+nums[0];
        }

        return tf;
    }

    private bool IsOnePair ()
    {
        bool tf = false;

        if (nums[0]==nums[1]) {
            // ex) 7,7,5,3,1
            tf = true;
            handRank = 1;
            handScore = 1000*nums[0]+100*nums[2]+10*nums[3]+nums[4];
        }
        else if (nums[1]==nums[2]) {
            // ex) 7,5,5,3,1
            tf = true;
            handRank = 1;
            handScore = 1000*nums[1]+100*nums[0]+10*nums[3]+nums[4];
        }
        else if (nums[2]==nums[3]) {
            // ex) 7,5,3,3,1
            tf = true;
            handRank = 1;
            handScore = 1000*nums[2]+100*nums[0]+10*nums[1]+nums[4];
        }
        else if (nums[3]==nums[4]) {
            // ex) 7,5,3,1,1
            tf = true;
            handRank = 1;
            handScore = 1000*nums[3]+100*nums[0]+10*nums[1]+nums[2];
        }

        return tf;
    }
}
