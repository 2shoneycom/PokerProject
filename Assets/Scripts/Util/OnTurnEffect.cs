using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnTurnEffect : MonoBehaviour
{
    public float speed = 1f;  // 색 변화 속도
    private Image rend;

    void Start()
    {
        rend = GetComponent<Image>();
    }

    void Update()
    {
        // 시간에 따라 Hue 값 변경 (0~1 범위를 순환)
        float h = Mathf.PingPong(Time.time * speed, 1f);
        Color rainbow = Color.HSVToRGB(h, 1f, 1f);

        rend.material.SetColor("_SolidOutline", rainbow);
    }
}
