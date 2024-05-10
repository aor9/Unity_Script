using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DownArrowMove : MonoBehaviour
{
    public void ShowDownArrow()
    {
        Vector3 position = transform.parent.position;
        gameObject.transform.position = new Vector3(position.x, position.y + 0.85f, position.z);
        position.y += 0.75f;
        transform.DOMove(position, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        
        SpriteRenderer downArrowRenderer;
        transform.gameObject.TryGetComponent<SpriteRenderer>(out downArrowRenderer);
        downArrowRenderer.color = new Color(0, 0.8f, 0.8f, 1);
    }

    public void HideDownArrow()
    {
        DOTween.Kill(transform);
        SpriteRenderer downArrowRenderer;
        transform.gameObject.TryGetComponent<SpriteRenderer>(out downArrowRenderer);
        downArrowRenderer.color = new Color(1, 1, 1, 0);
    }
    
}
