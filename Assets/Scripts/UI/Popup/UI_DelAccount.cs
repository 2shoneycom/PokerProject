using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_DelAccount : UI_Popup
{
    enum Buttons
    {
        UI_YesButton,
        UI_NoButton,
    }

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));

        BindEvent(GetButton((int)Buttons.UI_NoButton).gameObject, PopUpClose);
    }

    void PopUpClose(PointerEventData data)
    {
        ClosePopupUI();
    }
}
