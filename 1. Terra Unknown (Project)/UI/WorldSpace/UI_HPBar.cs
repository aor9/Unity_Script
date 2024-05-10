using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

public class UI_HPBar : UI_Base
{
    enum GameObjects
    {
        HPBar
    }

    private CharacterInfo characterInfo;
    
    public override void Init()
    {
        Bind<GameObject>(typeof(GameObjects));
        characterInfo = transform.parent.GetComponent<CharacterInfo>();
    }

    private void Update()
    {
        Transform parent = transform.parent;
        transform.position = parent.position + Vector3.up * -0.01f;

        float ratio = (float)characterInfo.hp / characterInfo.maxHp;
        SetHpRatio(ratio);
    }

    public void SetHpRatio(float ratio)
    {
        GetObject((int)GameObjects.HPBar).GetComponent<Slider>().value = ratio;
    }
}
