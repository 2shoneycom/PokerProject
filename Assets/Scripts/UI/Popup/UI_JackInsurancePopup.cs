using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_JackInsurancePopup : UI_Popup
{
    enum Texts
    {
        UI_BackText,
    }

    enum Buttons
    {
        UI_ButtonYes,
        UI_ButtonNo,
    }

    enum GameObjects
    {
        UI_Block,
    }

    bool isEvenMoney = false;

    JackGameControl _control;

    public override void Init()
    {
        base.Init();

        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(GameObjects));

        SetText();
        BindButton();
    }

    public void SetBool(bool value)
    {
        isEvenMoney = value;
    }

    public void SetControl(JackGameControl control)
    {
        _control = control;
    }

    void SetText()
    {
        if (isEvenMoney)
            GetText((int)Texts.UI_BackText).text = "이븐머니?";
    }

    void BindButton()
    {
        GetButton((int)Buttons.UI_ButtonYes).gameObject.BindEvent(BindButtonYes);
        GetButton((int)Buttons.UI_ButtonNo).gameObject.BindEvent(BindButtonNo);
    }

    void BindButtonYes(PointerEventData data)
    {
        _control.Sync.SyncJackIsInsurance(User.NowGamePlayer.GameIndex, 1);

        ClosePopupUI();
    }

    void BindButtonNo(PointerEventData data)
    {
        _control.Sync.SyncJackIsInsurance(User.NowGamePlayer.GameIndex, -1);

        ClosePopupUI();
    }
}
