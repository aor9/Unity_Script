using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Event;
        
        Debug.Log("Event Init");
    }
    
    public override void Clear()
    {
        Debug.Log("Campaign Scene Clear!");   
    }
    public void PreviousScene()
    {
        Managers.Scene.LoadScene(Define.Scene.BossNodeMap);
    }
}
