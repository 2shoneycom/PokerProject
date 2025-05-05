using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerInfo : UI_Scene
{

    enum Buttons
    {
        UI_ChangePwdButton,
        UI_LogoutButton,
        UI_DelAccountButton,
    }

    enum Texts
    {
        UI_MainInfo_UserName,
        UI_MainInfo_Money_Text,
    }

    enum Images
    {
        UI_Backspace,
        UI_MainInfo_UserImage,
        UI_MainInfo_UserEdit,
    }

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));

        BindEvent(GetImage((int)Images.UI_Backspace).gameObject, Managers.Scene.MoveToLobbyScene);
    }
}
