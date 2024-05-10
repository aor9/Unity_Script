using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestSiteScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.RestSite;
        
        Debug.Log("RestSite Init");
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
