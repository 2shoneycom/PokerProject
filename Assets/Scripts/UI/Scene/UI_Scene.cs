using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Scene : UI_Base     // ¸ðµç ¾À UIÀÇ super class
{
    public override void Init()
    {
        Managers.UI.SetCanvas(gameObject, false);
    }
    
}
