using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI : MonoBehaviour
{
    public TMP_Text userName;
    public TMP_Text userSeedMoney;

    private void Start()
    {
        userName.text = User.Instance.nickName;
        userSeedMoney.text = User.Instance.seedMoney.ToString();
    }
    void Update()
    {
        userName.text = User.Instance.nickName;
        userSeedMoney.text = User.Instance.seedMoney.ToString();
    }

    public void Logout()
    {
        LoginManager.Instance.LogOut();
    }

}
