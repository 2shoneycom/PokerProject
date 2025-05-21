using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_EditNickName : UI_Popup
{
    enum Texts
    {
        UI_StatusText,
    }

    enum Images
    {
        UI_ClosePopUp,
    }

    enum Buttons
    {
        UI_EditButton,
    }

    public override void Init()
    {
        base.Init();

        Bind<Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        BindEvent(GetImage((int)Images.UI_ClosePopUp).gameObject, (PointerEventData) => { ClosePopupUI(); });
    }
}
