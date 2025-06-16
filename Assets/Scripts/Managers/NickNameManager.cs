using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum NickNameCheckResult
{
    Error,         // Firebase 조회 실패
    Duplicated,    // 중복됨
    Available      // 사용 가능
}

public class NickNameManager
{
    UI_EditNickName _editNickName;
    UI_PlayerInfo _playerInfo;
    public void EditNickName(string newNickName, UI_EditNickName ui)
    {
        _playerInfo = (UI_PlayerInfo)Managers.UI.SceneUI;
        _editNickName = ui;

        if (newNickName.Length < 2)
        {
            _editNickName.SetStatusInfoText("닉네임은 최소 2자입니다");
            return;
        }
        if (newNickName.Length > 8)
        {
            _editNickName.SetStatusInfoText("닉네임은 최대 8자입니다");
            return;
        }

        Managers.DB.IsNickNameChangeAvailable(isAvailable =>
        {
            if (isAvailable) {
                Managers.DB.IsOverlapped(newNickName, result =>
                {
                    switch (result)
                    {
                        case NickNameCheckResult.Duplicated:
                            Debug.Log("이미 사용 중인 닉네임입니다.");
                            break;

                        case NickNameCheckResult.Available:
                            Debug.Log("사용 가능한 닉네임입니다.");
                            Managers.DB.ChangeNickName(newNickName);
                            User.NowUser.SetNickName(newNickName);
                            _playerInfo.SetPlayerInfo();
                            _editNickName.ClosePopupUI();
                            break;

                        case NickNameCheckResult.Error:
                            Debug.LogError("닉네임 중복 검사 중 오류가 발생했습니다.");
                            break;
                    }
                });
            }
            else
            {
                _editNickName.SetStatusInfoText("닉네임을 변경한 지 30일이 지나지 않았습니다");
            }
        });
    }
}