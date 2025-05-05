using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Setting : UI_Popup
{
    enum GameObjects
    {
        UI_Block,
    }

    public override void Init()
    {
        base.Init();

        Bind<GameObject>(typeof(GameObjects));
        GetGameObject((int)GameObjects.UI_Block).BindEvent((PointerEventData) => { ClosePopupUI(); });
    }

}
