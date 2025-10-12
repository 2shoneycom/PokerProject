using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Setting : UI_Popup
{
    enum Images
    {
        UI_BGM_Icon,
        UI_SFX_Icon,
    }

    enum GameObjects
    {
        UI_PopupClose,
    }

    Sprite volumeOn;
    Sprite volumeOff;

    public override void Init()
    {
        base.Init();

        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));

        volumeOff = Managers.Resource.Load<Sprite>("Art/BackGround/Icon/volumeOff");
        volumeOn = Managers.Resource.Load<Sprite>("Art/BackGround/Icon/volumeOn");

        SetImage();
        BindEvent(GetGameObject((int)GameObjects.UI_PopupClose), (PointerEventData) => { ClosePopupUI(); });
        BindEvent(GetImage((int)Images.UI_BGM_Icon).gameObject, BGMControl);
        BindEvent(GetImage((int)Images.UI_SFX_Icon).gameObject, SFXControl);
    }

    void SetImage()
    {
        float volume = Managers.Audio.GetBGMVolume();
        SwitchImage(volume, Images.UI_BGM_Icon);

        volume = Managers.Audio.GetSFXVolume();
        SwitchImage(volume, Images.UI_SFX_Icon);
    }

    void SwitchImage(float volume, Images image)
    {
        if (volume > 0) GetImage((int)image).sprite = volumeOn;
        else GetImage((int)image).sprite = volumeOff;
    }

    void BGMControl(PointerEventData data)
    {
        float volume = Managers.Audio.BGMSoundOnOff();
        SwitchImage(volume, Images.UI_BGM_Icon);
    }

    void SFXControl(PointerEventData data)
    {
        float volume = Managers.Audio.SFXSoudOnOff();
        SwitchImage(volume, Images.UI_SFX_Icon);
    }
}
