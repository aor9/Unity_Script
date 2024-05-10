using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum NodeType
{
    Basic,
    Special,
    Event,
    RestSite,
    Shop,
    Boss
}

public enum DungeonType
{
    None,
    Sweep,
    Survival,
    Brawl,
}

public enum PenaltyType
{
    None,
    Rain,
    EnemyBuff,
    SpawnerBuff
}

public class NodeGenerator : MonoBehaviour
{
    public int x;
    public int y;
    public NodeMapScene.NodeMapType nodeMapType;
    
    [SerializeField] private float spacingX;
    [SerializeField] private float spacingY;
    [SerializeField] private Sprite basicSprite;
    [SerializeField] private Sprite specialSprite;
    [SerializeField] private Sprite eventSprite;
    [SerializeField] private Sprite restSiteSprite;
    [SerializeField] private Sprite shopSprite;
    [SerializeField] private Sprite bossSprite;
    
    private Dictionary<Vector2Int, NodeInfo> nodeDictionary;
    private List<NodeInfo> uselessNodes;
    private List<NodeInfo> nodePath;
    private int currentPosition = 0;
    private Transform nodeMapRoot;
    private Transform nodeRoot;
    private Transform lineRoot;
    private Color whiteColor = new Color(1, 1, 1, 1);
    private Color grayColor = new Color(0.2f, 0.2f, 0.2f, 1);
    
    public void Init()
    {
        nodeDictionary = new Dictionary<Vector2Int, NodeInfo>();
        uselessNodes = new List<NodeInfo>();
        nodePath = new List<NodeInfo>();
        
        nodeMapRoot = new GameObject { name = "@NodeMap_Root" }.transform;
        nodeRoot = new GameObject { name = "@Node_Root" }.transform;
        lineRoot = new GameObject { name = "@Line_Root" }.transform;

        this.transform.parent = nodeMapRoot;
        nodeRoot.parent = nodeMapRoot;
        lineRoot.parent = nodeMapRoot;  
        
        // 노드 생성
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                GameObject nodeObj = Managers.Resource.Instantiate("node");
                nodeObj.name = i.ToString() + j.ToString();
                nodeObj.transform.parent = nodeRoot;
                
                float randomX = Random.Range(-0.3f, 0.3f);
                float randomY = Random.Range(-0.3f, 0.3f);
                
                nodeObj.transform.position = new Vector3(i * spacingX - 10 + randomX, j * spacingY - 4 + randomY, 0f);

                nodeObj.TryGetComponent<NodeInfo>(out var node);
                node.nodeCoord = new Vector2Int(i, j);
                nodeDictionary.Add(node.nodeCoord, node);
            }
        }

        NodeDFS();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var node = GetNode();
            if (node.HasValue)
            {
                node.Value.collider.gameObject.TryGetComponent<NodeInfo>(out var clickedNode);
                if (clickedNode.nodeCoord.x == 0)
                {
                    PressDownNode(clickedNode);
                }
                if (clickedNode.nodeCoord.x == currentPosition && clickedNode.isValidPathNode == true)
                {
                    PressDownNode(clickedNode);
                }
            }
        }
    }
    
    // Raycast로 node를 받아오는 메서드
    public RaycastHit2D? GetNode()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2d = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2d, Vector2.zero);

        if(hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }

        return null;
    }

    // 노드 클릭 시 메소드
    private void PressDownNode(NodeInfo clickedNode)
    {
        // audio test
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        
        nodePath.Add(clickedNode);

        EnterDungeon(clickedNode);
        
        HidePreviousNode();
        
        foreach (var node in clickedNode.destinationNodes)
        {
            node.isValidPathNode = true;
        }
        
        currentPosition++;
        
        DisplaySelectableNode();
    }
    
    // TODO:노드를 선택했을 때 해당 노드의 타입에 맞는 던전으로 들어가게함.
    private void EnterDungeon(NodeInfo clickedNode)
    {
        DontDestroyOnLoad(nodeMapRoot);
        
        this.gameObject.SetActive(false);
        nodeRoot.gameObject.SetActive(false);
        lineRoot.gameObject.SetActive(false);

        switch (clickedNode.type)
        {
            case NodeType.Basic:
                LoadingSceneController.Instance.LoadScene("test");
                break;
            case NodeType.Special:
                LoadingSceneController.Instance.LoadScene("test");
                break;
            case NodeType.Event:
                Managers.Scene.LoadScene(Define.Scene.Event);
                break;
            case NodeType.RestSite:
                Managers.Scene.LoadScene(Define.Scene.RestSite);
                break;
            case NodeType.Shop:
                Managers.Scene.LoadScene(Define.Scene.Shop);
                break;
                
        }
    }
    
    // 이전 선택 가능했던 노드 강조 표시 해제
    private void HidePreviousNode()
    {
        for (int i = 0; i < y; i++)
        {
            NodeInfo node;
            if (nodeDictionary.TryGetValue(new Vector2Int(currentPosition, i), out node))
            {
                bool isContained = nodePath.Contains(node);
                if (isContained is false)
                {
                    node.TryGetComponent<SpriteRenderer>(out var nodeSpriteRenderer);
                    nodeSpriteRenderer.color = grayColor;
                    node.isValidPathNode = false;
                }
            }
        }
    }

    // 선택가능한 노드 강조 표시
    private void DisplaySelectableNode()
    {
        for (int i = 0; i < y; i++)
        {
            NodeInfo node;
            if (nodeDictionary.TryGetValue(new Vector2Int(currentPosition, i), out node))
            {
                if (currentPosition == 0 || node.isValidPathNode is true)
                {
                    node.TryGetComponent<SpriteRenderer>(out var nodeSpriteRenderer);
                    nodeSpriteRenderer.color = whiteColor;
                }
            }
        }
    }

    // DFS를 y - 1번 수행하여 y - 1개의 길을 만드는 메서드.
    private void NodeDFS()
    {
        List<int> startingPoints = new List<int>();
        
        for (int i = 0; i < y - 1; i++)
        {
            int start = Random.Range(0, y);
            if (i < 3)
            {
                start = Random.Range(0, y);
                startingPoints.Add(start);
            }
            else
            {
                start = Random.Range(0, y);
                while (startingPoints.Contains(start))
                {
                    start = Random.Range(0, y);
                }
                startingPoints.Add(start);
            }
            
            NodeInfo currentNode = nodeDictionary[new Vector2Int(0, start)];
            currentNode.visited = true;
            
            int nextX = 1;
            bool rest = false;
                
            while (nextX < x)
            {
                if (nextX == x - 1 && nodeMapType == NodeMapScene.NodeMapType.Boss)
                {
                    Vector2Int lastNodePosition = new Vector2Int(nextX, 2);
                    nodeDictionary.TryGetValue(lastNodePosition, out NodeInfo lastNode);
                    currentNode.destinationNodes.Add(lastNode);
                    lastNode.visited = true;
                    break;
                }
                
                int nextY = Random.Range(-1, 2);
                int rand = Random.Range(0, 101);
                Vector2Int nextNodePosition = new Vector2Int(nextX, currentNode.nodeCoord.y + nextY);
                
                if (nodeDictionary.TryGetValue(nextNodePosition, out NodeInfo nextNode))
                {
                    currentNode.destinationNodes.Add(nextNode);
                    nextNode.visited = true;
                    currentNode = nextNode;
                    nextX++;
                    if (rand < 5 && !rest)
                    {
                        currentNode.type = NodeType.RestSite;
                        rest = true;
                    }
                }
                else
                {
                    if (currentNode.nodeCoord.y == 0)
                    {
                        int randomY = Random.Range(0, 2);
                        nextNode = nodeDictionary[new Vector2Int(nextX, currentNode.nodeCoord.y + randomY)];
                    }
                    else
                    {
                        int randomY = Random.Range(-1, 1);
                        nextNode = nodeDictionary[new Vector2Int(nextX, currentNode.nodeCoord.y + randomY)];
                    }
                    
                    currentNode.destinationNodes.Add(nextNode);
                    nextNode.visited = true;
                    currentNode = nextNode;
                    nextX++;
                }
            }
        }
        SetNodeType();
        DrawLine();
    }

    // 생성된 Node의 Type을 생성하고 visited가 false인 node들은 삭제
    private void SetNodeType()
    {
        foreach (var node in nodeDictionary.Values)
        {
            if (node.visited == false)
            {
                uselessNodes.Add(node);
            }
            else
            {
                int randomNode = Random.Range(0, 93);
                node.TryGetComponent<SpriteRenderer>(out var nodeSpriteRenderer);
                nodeSpriteRenderer.color = grayColor;      
                
                if (node.nodeCoord.x == 0)
                {
                    SetNodeProperties(NodeType.Basic, GenerateDungeonType());
                    continue;
                }

                if (node.nodeCoord.x == x - 1 && nodeMapType == NodeMapScene.NodeMapType.Boss)
                {
                    SetNodeProperties(NodeType.Boss, GenerateDungeonType());
                    continue;
                }
                
                if (node.nodeCoord.x == x - 2 && nodeMapType == NodeMapScene.NodeMapType.Boss)
                {
                    SetNodeProperties(NodeType.RestSite, GenerateDungeonType());
                    continue;
                }
                
                if (randomNode <= 45)
                {
                    SetNodeProperties(NodeType.Basic, GenerateDungeonType());
                }
                else if (randomNode <= 67)
                {
                    SetNodeProperties(NodeType.Special, GenerateDungeonType(), GeneratePenaltyType());
                }
                else if (randomNode <= 83)
                {
                    SetNodeProperties(NodeType.Event);
                }
                else if(randomNode <= 88)
                {
                    if (nodeMapType == NodeMapScene.NodeMapType.Normal)
                    {
                        SetNodeProperties(NodeType.Event);
                    }
                    else
                    {
                        SetNodeProperties(NodeType.Shop);
                    }
                }
                else if (randomNode <= 92 && node.nodeCoord.x != x - 3)
                {
                    SetNodeProperties(NodeType.RestSite);
                }
                
                void SetNodeProperties(NodeType type, DungeonType dungeonType = DungeonType.None, PenaltyType penaltyType = PenaltyType.None)
                {
                    switch (type)
                    {
                        case NodeType.Basic:
                            nodeSpriteRenderer.sprite = basicSprite;
                            break;
                        case NodeType.Special:
                            nodeSpriteRenderer.sprite = specialSprite;
                            break;
                        case NodeType.Event:
                            nodeSpriteRenderer.sprite = eventSprite;
                            break;
                        case NodeType.RestSite:
                            nodeSpriteRenderer.sprite = restSiteSprite;
                            break;
                        case NodeType.Shop:
                            nodeSpriteRenderer.sprite = shopSprite;
                            break;
                        case NodeType.Boss:
                            nodeSpriteRenderer.sprite = bossSprite;
                            break;
                        default:
                            throw new ArgumentException(nameof(type));
                    }

                    node.type = type;
                    node.dungeonType = dungeonType;
                    node.penaltyType = penaltyType;
                }

                DungeonType GenerateDungeonType()
                {
                    int randomDungeon = Random.Range(0, 101);
                    if (randomDungeon <= 60)
                    {
                        return DungeonType.Sweep;
                    }
                    else if (randomDungeon <= 90)
                    {
                        return DungeonType.Brawl;
                    }
                    else if (randomDungeon <= 100)
                    {
                        return DungeonType.Survival;
                    }
                    else
                    {
                        Debug.Log("확률 값이 벗어남");
                        return DungeonType.None;
                    }
                }

                PenaltyType GeneratePenaltyType()
                {
                    int randomPenalty = Random.Range(0, 101);
                    if (randomPenalty <= 33)
                    {
                        return PenaltyType.Rain;
                    }
                    else if (randomPenalty <= 66)
                    {
                        return PenaltyType.EnemyBuff;
                    }
                    else if (randomPenalty <= 100)
                    {
                        return PenaltyType.SpawnerBuff;
                    }
                    else
                    {
                        Debug.Log("확률 값이 벗어남");
                        return PenaltyType.None;
                    }
                }
            }
        }

        foreach (var node in uselessNodes)
        {
            Destroy(node.gameObject);
            nodeDictionary.Remove(node.nodeCoord);
        }
    }

    // LineRender로 node를 연결하는 line을 그리는 메서드
    private void DrawLine()
    {
        foreach (var node in nodeDictionary.Values)
        {
            foreach (var destinationNode in node.destinationNodes)
            {
                GameObject lineObj = Managers.Resource.Instantiate("Line");
                lineObj.transform.parent = lineRoot;
                
                lineObj.TryGetComponent<LineRenderer>(out var line);
                line.positionCount = 2;
                line.enabled = true;
                line.SetPosition(0, node.transform.position);
                line.SetPosition(1, destinationNode.transform.position);
            }
        }
        
        DisplaySelectableNode();
    }
}
