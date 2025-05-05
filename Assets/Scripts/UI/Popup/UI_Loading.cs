using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;

public class UI_Loading : UI_Popup
{
    enum Texts
    {
        UI_ConnectingInfoText,
    }

    public override void Init()
    {
        base.Init();

        Bind<TextMeshProUGUI>(typeof(Texts));
        GetText((int)Texts.UI_ConnectingInfoText).text = "Loading...";
    }


    public void SetConnectionInfoText(string info)
    {
        GetText((int)Texts.UI_ConnectingInfoText).text = info;
    }
}
