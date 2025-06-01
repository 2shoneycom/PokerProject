using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User
{
    private string uid;
    private string nickName;
    private long seedMoney;

    public void SetUid(string value) => uid = value;
    public void SetNickName(string value) => nickName = value;
    public void SetSeedMoney(long value) => seedMoney = value;

    // 값 읽기용 getter도 필요하다면 추가
    public string GetUid() => uid;
    public string GetNickName() => nickName;
    public long GetSeedMoney() => seedMoney;

    public void UpdateMoney(long value)
    {
        seedMoney += value;
    }
}
