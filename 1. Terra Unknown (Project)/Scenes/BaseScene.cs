using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
    Define.Scene sceneType = Define.Scene.Unknown;

    public Define.Scene SceneType { get; protected set; } = Define.Scene.Unknown;
    void Awake()
    {
        Init();
    }
    protected virtual void Init()
    {
        // frame 60
        Application.targetFrameRate = 60;
        
        Object obj = GameObject.FindObjectOfType(typeof(EventSystem));
        if (obj is null)
        {
            Managers.Resource.Instantiate("UI/EventSystem").name = "@EventSystem";
        }
    }

    public abstract void Clear();
}
