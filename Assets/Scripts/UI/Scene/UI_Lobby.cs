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
        UI_Backspace,
    }

    UI_Popup _popup = null;

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));

        GetText((int)Texts.UI_Profile_Text).text = Managers.User.nickName;
        GetText((int)Texts.UI_Money_Text).text = Managers.User.seedMoney.ToString();

        BindEvent(GetImage((int)Images.UI_Backspace).gameObject, BackspaceClick);
        GetImage((int)Images.UI_Backspace).gameObject.SetActive(false);

        BindEvent(GetButton((int)Buttons.UI_ButtonHoldem).gameObject, HoldemClicked);
        BindEvent(GetButton((int)Buttons.UI_ButtonBlackJack).gameObject, JackClicked);
        BindEvent(GetButton((int)Buttons.UI_ButtonPoker).gameObject, PokerClicked);

        BindEvent(GetImage((int)Images.UI_Profile).gameObject, MoveToPlayerInfoScene);
        BindEvent(GetImage((int)Images.UI_IconFriend).gameObject, MoveToFriendScene);
        BindEvent(GetImage((int)Images.UI_IconSetting).gameObject, SettingClicked);
        BindEvent(GetImage((int)Images.UI_IconGift).gameObject, GiftClicked);
        //BindEvent(GetButton((int)Buttons.UI_ButtonBlackJack).gameObject, LoginManager.Instance.LogOut);
    }

    void HoldemClicked(PointerEventData data)
    {
        PopupSetting(true);

        _popup = Managers.UI.ShowPopupUI<UI_HoldemPopup>();

        GetText((int)Texts.UI_LobbyTitleText).text = "텍사스 홀덤";
    }

    void PokerClicked(PointerEventData data)
    {
        PopupSetting(true);

        _popup = Managers.UI.ShowPopupUI<UI_PokerPopup>();

        GetText((int)Texts.UI_LobbyTitleText).text = "세븐 포커";
    }

    void JackClicked(PointerEventData data)
    {
        PopupSetting(true);

        _popup = Managers.UI.ShowPopupUI<UI_JackPopup>();

        GetText((int)Texts.UI_LobbyTitleText).text = "블랙잭";
    }

    void BackspaceClick(PointerEventData data)
    {
        Managers.UI.ClosePopupUI(_popup);

        PopupSetting(false);

        GetText((int)Texts.UI_LobbyTitleText).text = "포커 하우스";
    }

    void PopupSetting(bool popupOn)
    {
        bool inScene = !popupOn;

        GetImage((int)Images.UI_Profile).gameObject.SetActive(inScene);
        GetImage((int)Images.UI_Money).gameObject.SetActive(inScene);
        GetImage((int)Images.UI_IconSetting).gameObject.SetActive(inScene);
        GetImage((int)Images.UI_IconFriend).gameObject.SetActive(inScene);
        GetImage((int)Images.UI_IconWeb).gameObject.SetActive(inScene);
        GetImage((int)Images.UI_IconGift).gameObject.SetActive(inScene);

        GetImage((int)Images.UI_Backspace).gameObject.SetActive(popupOn);
    }

    void GiftClicked(PointerEventData data)
    {
        Managers.UI.ShowPopupUI<UI_DailyCheck>();
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
}
