using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Holdem : UI_Scene
{
    enum Buttons
    {
        UI_Buttons_Die,
        UI_Buttons_Call,
        UI_Buttons_Double,
        UI_Buttons_Quater,
        UI_Buttons_Half,
        UI_Buttons_AllIn,
        UI_RoomButton,
        UI_GameStartButton,
    }

    enum Texts
    {
        UI_Player1_NameText,
        UI_Player2_NameText,
        UI_Player3_NameText,
        UI_Player4_NameText,
        UI_Player5_NameText,
        UI_Player6_NameText,
        UI_Player7_NameText,
        UI_PotMoney_Text,
        UI_RoomButton_Text,
        UI_TmpWinnerShow_Text,
    }

    enum Images
    {
        UI_Player1_Icon,
        UI_Player2_Icon,
        UI_Player3_Icon,
        UI_Player4_Icon,
        UI_Player5_Icon,
        UI_Player6_Icon,
        UI_Player7_Icon,
        UI_PotMoney_Icon,
    }

    enum GameObjects
    {
        UI_Backspace,
        UI_IconFriend,
        UI_TmpWinnerShow,
    }

    bool isRoomOpened = false;

    public override void Init()
    {
//        base.Init();

        Managers.UI.SetWorldSpaceUI(gameObject);

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));

        SettingUIIconPos();

        SeatBind();
        BetButtonBind();
        UISwitch(false);

        GetGameObject((int)GameObjects.UI_TmpWinnerShow).SetActive(false);
        GetButton((int)Buttons.UI_GameStartButton).gameObject.SetActive(false);
        BindEvent(GetGameObject((int)GameObjects.UI_Backspace), Managers.Scene.MoveToLobbyScene);
        BindEvent(GetGameObject((int)GameObjects.UI_IconFriend), IconFriendClicked);

        SetRoomButton(isRoomOpened);
    }

    void SettingUIIconPos()
    {
        GameObject go = GetGameObject((int)GameObjects.UI_Backspace);
        // 0,0 은 왼쪽 아래, 1,1 은 오른쪽 위
        go.transform.position =
            Camera.main.ViewportToWorldPoint(new Vector3(0, 1, Camera.main.nearClipPlane));
        // Z 축이 -가 되서 클릭이 안되는 현상 발생.
        go.transform.position = SetZeroZ(go.transform);

        go = GetGameObject((int)GameObjects.UI_IconFriend);
        go.transform.position =
            Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
        go.transform.position = SetZeroZ(go.transform);

        RectTransform toRect = GetGameObject((int)GameObjects.UI_IconFriend).GetComponent<RectTransform>();
        RectTransform targetRect = GetButton((int)Buttons.UI_RoomButton).GetComponent<RectTransform>();

        // 기준 오브젝트의 왼쪽 중앙 위치를 구함
        Vector3 leftCenterLocal = new Vector3(-toRect.rect.width, -toRect.rect.height * 0.5f, 0);
        Vector3 leftCenterWorld = toRect.TransformPoint(leftCenterLocal);

        // A 오브젝트를 해당 위치로 이동
        targetRect.position = leftCenterWorld;
    }

    Vector3 SetZeroZ(Transform transform)
    {
        Vector3 zeroZ = transform.position;
        zeroZ.z = 0;
        return zeroZ;
    }

    public void UISwitch(bool isOn)
    {
        foreach (int idx in Enum.GetValues(typeof(Buttons)))
        {
            if (idx == (int)Buttons.UI_RoomButton || idx == (int)Buttons.UI_GameStartButton) 
                continue;

            GetButton(idx).interactable = false;
            GetButton(idx).gameObject.SetActive(isOn);
        }
        GetImage((int)Images.UI_PotMoney_Icon).gameObject.SetActive(isOn);
    }

    public void GameStartButtonOn()
    {
        Button bt = GetButton((int)Buttons.UI_GameStartButton);
        bt.gameObject.SetActive(true);

        bt.onClick.RemoveAllListeners();
        bt.onClick.AddListener(GameStartButtonClicked);
    }

    void GameStartButtonClicked()
    {
        GetButton((int)Buttons.UI_GameStartButton).gameObject.SetActive(false);

        // 게임 시작
        SyncSystem.Instacne.HoldemStartSync();
    }

    public void BetButtonInteractiveSwitch(string betType, bool isOn)
    {
        string type = $"UI_Buttons_{betType}";
        GetButton((int)Enum.Parse(typeof(Buttons), type)).interactable = isOn;
    }

    public void UpdatePlayerName(int index, string str)
    {
        GetText((int)Enum.Parse(typeof(Texts), $"UI_Player{index}_NameText")).text = str;
    }

    public void UpdatePotMoney()
    {
        GetText((int)Texts.UI_PotMoney_Text).text = $"{HoldemGameControl.Control.PotMoney}";
    }

    void SeatBind()
    {
        for(int i = 0; i < 7; i++)
        {
            string img = $"UI_Player{i + 1}_Icon";
            int num = i;
            GetImage((int)Enum.Parse(typeof(Images), img)).gameObject.BindEvent(PointerEventData =>
            {
                Managers.Seat.HaveSeat(User.NowUser.GetNickName(), num);
            });
        }
    }

    void SetRoomButton(bool isRoomOpened)
    {
        if (!isRoomOpened)
        {
            ColorUtility.TryParseHtmlString("#FF0000", out Color targetColor);
            GetButton((int)Buttons.UI_RoomButton).GetComponent<Image>().color = targetColor;

            GetText((int)Texts.UI_RoomButton_Text).text = "방 공개";
            BindEvent(GetButton((int)Buttons.UI_RoomButton).gameObject, OpenRoomClicked);
        }
        else
        {
            ColorUtility.TryParseHtmlString("#CFBFBF", out Color targetColor);
            GetButton((int)Buttons.UI_RoomButton).GetComponent<Image>().color = targetColor;

            GetText((int)Texts.UI_RoomButton_Text).text = "방 이동";
            BindEvent(GetButton((int)Buttons.UI_RoomButton).gameObject, MoveRoomClicked);
        }
    }

    void IconFriendClicked(PointerEventData data)
    {
        Managers.UI.ShowPopupUI<UI_InviteFriendPopup>();
    }

    void BetButtonBind()            // interactable 체크 귀찮아서 onclick 이벤트로 추가함
    {
        Button bt = null;

        bt = GetButton((int)Buttons.UI_Buttons_Call);
        bt.onClick.AddListener(() => RequestBet("Call"));

        bt = GetButton((int)Buttons.UI_Buttons_Die);
        bt.onClick.AddListener(() => RequestBet("Die"));

        bt = GetButton((int)Buttons.UI_Buttons_Double);
        bt.onClick.AddListener(() => RequestBet("Double"));

        bt = GetButton((int)Buttons.UI_Buttons_Quater);
        bt.onClick.AddListener(() => RequestBet("Quater"));

        bt = GetButton((int)Buttons.UI_Buttons_Half);
        bt.onClick.AddListener(() => RequestBet("Half"));

        bt = GetButton((int)Buttons.UI_Buttons_AllIn);
        bt.onClick.AddListener(() => RequestBet("AllIn"));
    }

    void RequestBet(string betType)
    {
        if (HoldemGameControl.Players.GetPlayerTurn(User.NowHoldemPlayer.GameIndex) == false)
        {
            if (betType == "Die")       // 자신의 턴이 아니면 die만 켜져있어서 die만 누를테지만 혹시 모르니
            {
                if (HoldemGameControl.Players.GetPlayerDieReserve(User.NowHoldemPlayer.GameIndex) == false)
                    SyncSystem.Instacne.SyncHoldemDieReserve(User.NowHoldemPlayer.GameIndex, true);
                else
                    SyncSystem.Instacne.SyncHoldemDieReserve(User.NowHoldemPlayer.GameIndex, false);
            }
            return;     // 자신의 턴이 아닐때 die가 아니면 모두 리턴
        }

        HoldemGameControl.Bet.PlayerBetSelected(betType);
    }

    void OpenRoomClicked(PointerEventData data)
    {

    }

    void MoveRoomClicked(PointerEventData data)
    {

    }

    public GameObject GetPlayerGameObjcet(int index)
    {
        return GetImage((int)index).gameObject;
    }

    public void SetWinnerPanel(bool isOn)
    {
        GetGameObject((int)GameObjects.UI_TmpWinnerShow).SetActive(isOn);

        if (!isOn)
            return;

        var panelText = GetText((int)Texts.UI_TmpWinnerShow_Text);

        panelText.text = "Winner : ";

        List<string> wList = HoldemGameControl.Players.GetWinnerList();
        int len = wList.Count;

        for(int i = 0; i < len; i++)
        {
            panelText.text += wList[i];

            if (i != len - 1)
            {
                panelText.text += ", ";
            }
        }

        if (isOn)
            StartCoroutine(WaitWinnerPanel(HoldemGameControl.RESULT_SHOW_TIME));
    }

    IEnumerator WaitWinnerPanel(float sec)
    {
        yield return new WaitForSeconds(sec);

        SetWinnerPanel(false);
        HoldemGameControl.Control.NextStage();
    }
}
