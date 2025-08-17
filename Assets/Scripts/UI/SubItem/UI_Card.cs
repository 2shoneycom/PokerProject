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
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));
    }

    public void SetCardImage(int cardDetail)
    {
        Debug.Log("a");
        Image target = GetImage((int)Images.UI_Card_Image);
        if(target == null)
        {
            Debug.Log("null");

            Bind<Image>(typeof(Images));
            target = GetImage((int)Images.UI_Card_Image);
        }

        Debug.Log(cardDetail);
        target.sprite = JackGameControl.Card.GetRightCardImage(cardDetail);
        Debug.Log("b");

    }

}
