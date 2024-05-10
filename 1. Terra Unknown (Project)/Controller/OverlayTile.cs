using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// OverlayTile 관련 클래스
public class OverlayTile : MonoBehaviour
{
    public int G;
    public int H;
    public int F { get { return G + H; } }
    
    public bool isBlocked = false;

    public bool isUnitOn = false;

    public bool isSpawnerOn = false;

    public OverlayTile previous;

    public Vector3Int gridLocation;
    public Vector2Int grid2DLocation { get { return new Vector2Int(gridLocation.x, gridLocation.y); } }

    public List<Sprite> paths;

    public void ShowTile(ActionState actionState)
    {
        if (isBlocked is false)
        {
            if (actionState == ActionState.MOVE)
            {
                gameObject.GetComponent<SpriteRenderer>().color = new Color(0, 1, 0, 0.8f);          
            } 
            else if (actionState == ActionState.ATTACK)
            {
                gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.8f);
            }
            else if (actionState == ActionState.DEFAULT)
            {
                gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 0, 0.8f);
            }
        }
        
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 2;
    }

    public void ShowSkillTile()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(0.6f, 0.6f, 0, 0.8f);
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 2;
    }
    
    public void HideTile()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        SetPathSprite(PathTranslator.PathDirection.None);
    }
    
    public void SetPathSprite(PathTranslator.PathDirection d)
    {
        if (d == PathTranslator.PathDirection.None)
            GetComponentsInChildren<SpriteRenderer>()[1].color = new Color(1, 1, 1, 0);
        else
        {
            GetComponentsInChildren<SpriteRenderer>()[1].color = new Color(1, 1, 1, 1);
            GetComponentsInChildren<SpriteRenderer>()[1].sprite = paths[(int)d];
            GetComponentsInChildren<SpriteRenderer>()[1].sortingOrder = gameObject.GetComponent<SpriteRenderer>().sortingOrder;
        }
    }
}
