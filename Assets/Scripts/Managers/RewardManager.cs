using System;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class RewardResult
{
    public bool committed;      // 커밋됨(=이번 클릭으로 수령 성공)
    public string today;        // yyyy-MM-dd
    public string weekKey;      // yyyy-MM-dd(월요일)
    public int streak;          // 이번주 누적
    public long seedMoney;      // 최신 시드머니
    public long reward;         // 이번에 받은 금액(중복이면 0)
    public string message;      // 로그/표시용
}

public class RewardManager
{
    // 팝업 열기 전에 UI 초기상태(버튼 활성/체크개수 등) 계산
    public DBManager.ClaimView PrepareDailyState()
    {
        return Managers.DB.ComputeDailyStateFromCache();
    }

    // 보상 수령(원자적)
    public void DailyGift(System.Action<DBManager.ClaimResult> onDone)
    {
        Managers.DB.RunDailyClaimTransaction(onDone);
    }
}
