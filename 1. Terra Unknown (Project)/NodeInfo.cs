using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeInfo : MonoBehaviour
{
    public NodeType type;
    public DungeonType dungeonType = DungeonType.None;
    public PenaltyType penaltyType = PenaltyType.None;
    public bool visited = false;
    public bool isValidPathNode = false;
    public Vector2Int nodeCoord;
    public List<NodeInfo> destinationNodes;
}
