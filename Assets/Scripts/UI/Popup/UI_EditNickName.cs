using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

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
    enum InputFields
    {
        UI_InputNickName,
    }

    public override void Init()
    {
        base.Init();

        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<TMP_InputField>(typeof(InputFields));

        BindEvent(GetImage((int)Images.UI_ClosePopUp).gameObject, (PointerEventData) => { ClosePopupUI(); });
        BindEvent(GetButton((int)Buttons.UI_EditButton).gameObject, EditClicked);
    }

    void EditClicked(PointerEventData data)
    {
        string nickName = Get<TMP_InputField>((int)InputFields.UI_InputNickName).text;
        if (string.IsNullOrEmpty(nickName))
        {
            return;
        }
        Managers.NickName.EditNickName(nickName, this);
    }

    public void SetStatusInfoText(string info)
    {
        GetText((int)Texts.UI_StatusText).text = info;
    }
}
