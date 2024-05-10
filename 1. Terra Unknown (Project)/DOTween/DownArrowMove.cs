using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

//DOTWEEN을 사용한 현재 턴인 플레이어 위에 화살표를 표시하고 움직이게하는 클래스
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
