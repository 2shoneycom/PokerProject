using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Scene : UI_Base     // 모든 씬 UI의 super class
{
    public override void Init()
    {
        Managers.UI.SetCanvas(gameObject, false);
    }
    
}
