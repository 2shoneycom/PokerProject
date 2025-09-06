using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Card : UI_Base
{
    enum Images
    {
        UI_Card_Image,
    }

    enum GameObjects
    {
        UI_Card,
        UI_Block,
    }

    bool isInit = false;

    public override void Init()
    {
        if (!isInit)
        {
            Bind<Image>(typeof(Images));
            Bind<GameObject>(typeof(GameObjects));

            UIBlockSwitch(false);
            isInit = true;
        }
    }

    public void SetCardImage(int cardDetail)
    {
        Debug.Log("a");

        if (!isInit)
        {
            Init();
        }
        Image target = GetImage((int)Images.UI_Card_Image);
        Debug.Log(cardDetail);
        target.sprite = JackGameControl.Card.GetRightCardImage(cardDetail);
        Debug.Log("b");
    }

    public void UIBlockSwitch(bool isOn)
    {
        GetGameObject((int)GameObjects.UI_Block).SetActive(isOn);
    }
}
