using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampaignScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Campaign;
        Debug.Log("Campaign Init");
    }
    
    public override void Clear()
    {
        Debug.Log("Campaign Scene Clear!");   
    }
    
    //Scene 변경
    public void Lobby()
    {
        GameObject map = GameObject.Find("Map");
        GameObject overlayContainer = GameObject.Find("OverlayContainer");
        Managers.Resource.Destroy(map);
        Managers.Resource.Destroy(overlayContainer);
        
        Managers.Scene.LoadScene(Define.Scene.BossNodeMap);
    }
    
    public void Characteristic()
    {
        Managers.Scene.LoadScene(Define.Scene.Trait);
    }
}
