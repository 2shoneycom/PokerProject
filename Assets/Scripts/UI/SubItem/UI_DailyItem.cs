using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_DailyItem : UI_Base
{
    enum GameObjects
    {
        UI_Block,
    }

    enum Images
    {
        UI_DailyItemImage,
    }

    enum Texts
    {
        UI_DailyItem_MoneyText,
        UI_DailyItem_DayText,
    }

    int[] rewardArray = { 100000, 100000, 300000, 500000, 1000000, 3000000, 5000000 };

    Sprite img_small = null;
    Sprite img_mid = null;
    Sprite img_big = null;

    string text;
    int order = 0;
    bool amIOn = false;

    public override void Init()
    {
        Bind<GameObject>(typeof(GameObjects));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        SpriteLoad();

        Setting();
        BlockSwitch(amIOn);
    }

    void SpriteLoad()
    {
        img_small = Managers.Resource.Load<Sprite>($"Art/BackGround/Icon/Reward_small");
        img_mid = Managers.Resource.Load<Sprite>($"Art/BackGround/Icon/Reward_mid");
        img_big = Managers.Resource.Load<Sprite>($"Art/BackGround/Icon/Reward_big");
    }

    public void SetOrder(int ord) { order = ord; }

    void Setting()
    {
        int reward = rewardArray[order];
        text = reward.ToString("N0");
        GetText((int)Texts.UI_DailyItem_MoneyText).text = text;

        GetText((int)Texts.UI_DailyItem_DayText).text = $"{order + 1}일차";

        Sprite mySprite = null;
        if (order < 2) mySprite = img_small;
        else if (order < 4) mySprite = img_mid;
        else mySprite = img_big;

        GetImage((int)Images.UI_DailyItemImage).sprite = mySprite;

        amIOn = order < User.NowUser.Getstreak();
    }

    public void BlockSwitch(bool isOn)
    {
        GetGameObject((int)GameObjects.UI_Block).SetActive(isOn);
    }
}
