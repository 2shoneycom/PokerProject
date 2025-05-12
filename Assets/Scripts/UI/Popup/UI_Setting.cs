using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Setting : UI_Popup
{
    enum Images
    {
        UI_BGM_Icon,
        UI_SFX_Icon,
    }

    enum GameObjects
    {
        UI_PopupClose,
    }

    public override void Init()
    {
        base.Init();

        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));

        BindEvent(GetGameObject((int)GameObjects.UI_PopupClose), (PointerEventData) => { ClosePopupUI(); });
        BindEvent(GetImage((int)Images.UI_BGM_Icon).gameObject, BGMControl);
        BindEvent(GetImage((int)Images.UI_SFX_Icon).gameObject, SFXControl);
    }

    void BGMControl(PointerEventData data)
    {

    }

    void SFXControl(PointerEventData data)
    {

    }
}
