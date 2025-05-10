using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_DailyItem : UI_Base
{
    enum Texts
    {
        UI_DailyItem_Text,
    }

    string text;

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        GetText((int)Texts.UI_DailyItem_Text).text = text;
    }

    public void SetInfo(string txt) { text = txt; }
}
