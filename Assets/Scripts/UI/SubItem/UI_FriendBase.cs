using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UI_FriendBase : UI_Base
{
    protected string _name;
    protected Define.Status _status;
    protected Image _icon;

    public string Name
    {
        get { return _name; }
        set {  _name = value; }
    }

    public Image Icon
    {
        get { return _icon; }
        set { _icon = value; }
    }

    public Define.Status Status
    {
        get { return _status; }
        set { _status = value; }
    }

    public abstract void Setting();
}
