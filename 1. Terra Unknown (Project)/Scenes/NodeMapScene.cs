using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NodeMapScene : BaseScene
{
    public enum NodeMapType
    {
        Normal,
        Boss
    }

    public NodeMapType nodeMapType;
    
    protected override void Init()
    {
        base.Init();
        StartCoroutine(BgmStart());
        
        //TODO: boss, normal nodemap 구분
        SceneType = Define.Scene.BossNodeMap;
        
        // NodeMap 불러오거나 활성화
        if (GameObject.Find("@NodeMap_Root") is null)
        {
            GameObject nodeGenerator = Managers.Resource.Instantiate("NodeGenerator");
            NodeGenerator nodeGen = nodeGenerator.GetComponent<NodeGenerator>();
            
            nodeGen.nodeMapType = nodeMapType;
            if (nodeMapType == NodeMapType.Normal)
            {
                nodeGen.x = 5;
                nodeGen.y = 4;
            } 
            else if (nodeMapType == NodeMapType.Boss)
            {
                nodeGen.x = 9;
                nodeGen.y = 5;
            }
            
            nodeGen.Init();
        }
        else
        {
            Destroy(GameObject.Find("NodeMap"));
            GameObject nodeMapRoot = GameObject.Find("@NodeMap_Root");
            nodeMapRoot.transform.Find("NodeGenerator").gameObject.SetActive(true);
            nodeMapRoot.transform.Find("@Node_Root").gameObject.SetActive(true);
            nodeMapRoot.transform.Find("@Line_Root").gameObject.SetActive(true);
        }
        
        Debug.Log("NodeMap Init");
    }
    public override void Clear()
    {
        Debug.Log("Campaign Scene Clear!");   
    }

    IEnumerator BgmStart()
    {
        yield return new WaitForSeconds(0.3f);
        AudioManager.instance.PlayBgm(true, AudioManager.Bgm.Main);
    }
    
}
