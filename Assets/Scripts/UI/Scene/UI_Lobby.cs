using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Lobby : UI_Scene
{
    enum Buttons
    {
        UI_ButtonHoldem,
        UI_ButtonPoker,
        UI_ButtonBlackJack,
    }

    enum Texts
    {
        UI_LobbyTitleText,
        UI_ButtonHoldem_Text,
        UI_ButtonPoker_Text,
        UI_ButtonBlackJack_Text,
        UI_Profile_Text,
        UI_Money_Text,
    }

    enum Images
    {
        UI_IconFriend,
        UI_IconGift,
        UI_IconWeb,
        UI_IconSetting,
        UI_Profile,
        UI_Profile_Icon,
        UI_Money,
    }

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));

        GetText((int)Texts.UI_Profile_Text).text = Managers.User.nickName;
        GetText((int)Texts.UI_Money_Text).text = Managers.User.seedMoney.ToString();

        BindEvent(GetButton((int)Buttons.UI_ButtonHoldem).gameObject, HoldemClicked);
        BindEvent(GetImage((int)Images.UI_Profile).gameObject, MoveToPlayerInfoScene);
        BindEvent(GetImage((int)Images.UI_IconFriend).gameObject, MoveToFriendScene);
        BindEvent(GetImage((int)Images.UI_IconSetting).gameObject, SettingClicked);
        BindEvent(GetButton((int)Buttons.UI_ButtonBlackJack).gameObject, LoginManager.Instance.LogOut);
    }

    void HoldemClicked(PointerEventData data)
    {
        if (!GetButton((int)Buttons.UI_ButtonHoldem).interactable)
            return;

        GetButton((int)Buttons.UI_ButtonHoldem).interactable = false;
        GetButton((int)Buttons.UI_ButtonBlackJack).gameObject.SetActive(false);
        GetButton((int)Buttons.UI_ButtonPoker).gameObject.SetActive(false);

        Managers.Photon.JoinHoldem();
    }

    void SettingClicked(PointerEventData data)
    {
        Managers.UI.ShowPopupUI<UI_Setting>();
    }

    void MoveToPlayerInfoScene(PointerEventData data)
    {
        Managers.Scene.LoadScene(Define.Scene.PlayerInfo);
    }

    void MoveToFriendScene(PointerEventData data)
    {
        Managers.Scene.LoadScene(Define.Scene.Friend);
    }


    // Update is called once per frame
    void Update()
    {
//        GetText((int)Texts.UI_Profile_Text).text = Managers.User.nickName;
//        GetText((int)Texts.UI_Money_Text).text = Managers.User.seedMoney.ToString();
    }
}
