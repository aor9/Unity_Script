using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampaignScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Campaign;
        // TODO: player controller 같은 곳에서 초기화 하고 있던 친구들을 (캠페인 씬에 관련된 ) 그런 애들을 여기에 넣어주면 된다.
        // 여기에
        Debug.Log("Campaign Init");
    }
    
    public override void Clear()
    {
        Debug.Log("Campaign Scene Clear!");   
    }
    
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
