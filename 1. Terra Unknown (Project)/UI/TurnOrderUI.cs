using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class TurnOrderUI : MonoBehaviour
{
    public GameObject battleSystemObject;
    
    private List<Sprite> portraits;
    private BattleSystem battleSystem;
    private int portraitCnt = 0;
    private void Start()
    {
        battleSystem = battleSystemObject.GetComponent<BattleSystem>();
    }

    public IEnumerator GetPortrait()
    {
        yield return new WaitForSeconds(0.1f);
        portraitCnt = 0;
        
        Sprite sprite;
        portraits = new List<Sprite>();
        int i = 0, cnt = 0;
        int index = battleSystem.TurnOrderIdx;
        
        int turnOrderCnt = 5;
        if (battleSystem.TurnOrderList.Count < 5)
        {
            turnOrderCnt = battleSystem.TurnOrderList.Count;
        }

        while (cnt < turnOrderCnt)
        {
            if (index >= battleSystem.TurnOrderList.Count || index == -1)
            {
                index = 0;
                continue;
            }
            
            string name = battleSystem.TurnOrderList[index].transform.name;
            if (battleSystem.TurnOrderList[index].type == "Player")
            {
                sprite = Resources.Load<Sprite>($"Img/Mercenary/{name}");
                portraits.Add(sprite);
            }
            else if(battleSystem.TurnOrderList[index].type == "Enemy")
            {
                sprite = Resources.Load<Sprite>($"Img/Enemy/{name}");
                portraits.Add(sprite);
            }
            else
            {
                Debug.Log("Sprite not found !!");
            }

            index++;
            cnt++;
        }
        
        BindTurnOrderImg();
    }

    private void BindTurnOrderImg()
    {
        int i = 0;
        foreach (var _portrait in portraits)
        {
            if (i == 0)
            {
                transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = _portrait;
                transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = battleSystem.TurnOrderList[battleSystem.TurnOrderIdx].name;
                portraitCnt++;
            }
            else
            {
                transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = _portrait;
            }
            i++;
        }
    }

    IEnumerator DestroyObj(GameObject destroyObj)
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(destroyObj);
    }

}
