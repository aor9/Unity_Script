using System;
using System.Collections;
using System.Collections.Generic;
using KoreanTyper;
using UnityEngine;
using UnityEngine.UI;

public class Typer : MonoBehaviour
{
    private string originText;
    private Text myText;

    private void Awake()
    {
        myText = GetComponent<Text>();
    }

    void Start()
    {
        originText = myText.text;
        myText.text = "";

        StartCoroutine(TypingRoutine());
    }

    IEnumerator TypingRoutine()
    {
        int typingLength = originText.GetTypingLength();

        for (int i = 0; i <= typingLength; i++)
        {
            myText.text = originText.Typing(i);
            yield return new WaitForSeconds(0.05f);
        }
    }
}
