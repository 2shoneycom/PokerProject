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
        UI_LoseBlock,
        UI_PushBlock,
    }

    bool isInit = false;

    public override void Init()
    {
        if (!isInit)
        {
            Bind<Image>(typeof(Images));
            Bind<GameObject>(typeof(GameObjects));

            UILoseBlockSwitch(false);
            UIPushBlockSwitch(false);
            isInit = true;
        }
    }

    public void SetCardImage(int cardDetail, JackGameControl jackControl)
    {
        Debug.Log("a");

        if (!isInit)
        {
            Init();
        }
        Image target = GetImage((int)Images.UI_Card_Image);
        Debug.Log(cardDetail);
        target.sprite = jackControl.Card.GetRightCardImage(cardDetail);
        Debug.Log("b");
    }

    public void UILoseBlockSwitch(bool isOn)
    {
        GetGameObject((int)GameObjects.UI_LoseBlock).SetActive(isOn);
    }

    public void UIPushBlockSwitch(bool isOn)
    {
        GetGameObject((int)GameObjects.UI_PushBlock).SetActive(isOn);
    }
}
