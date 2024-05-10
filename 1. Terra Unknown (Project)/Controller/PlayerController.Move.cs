using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.ExampleScripts;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using AnimationState = Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts.AnimationState;

public partial class PlayerController
{
    //TODO : Move 관련 메소드 클래스에서 분리하기
    
    public void GetInRangeTiles(CharacterInfo character)
    {
        HideCurrentTiles();

        if (battleSystem.actionState == ActionState.MOVE)
        {
            inRangeTiles = rangeFinder.GetTilesInRange(character.standingOnTile, character.move);
        } 
        else if (battleSystem.actionState == ActionState.ATTACK)
        {
            inRangeTiles = rangeFinder.GetTilesInRange(character.standingOnTile, character.move);
        }
        
        foreach (var item in inRangeTiles)
        {
            if (character.standingOnTile == item)
            {
                continue;
            }
            
            item.ShowTile(battleSystem.actionState);
        }
    }

    public bool CheckCharacterOverlapping(CharacterInfo character, OverlayTile clickedTile)
    {
        foreach (var unit in battleSystem.TurnOrderList)
        {
            if (unit.standingOnTile == clickedTile && clickedTile.isBlocked is true)
            {
                Debug.Log("해당 타일로 이동할 수 없습니다.");
                isPaused = false;
                return true;
            }
        }

        character.standingOnTile.isUnitOn = false;
        return false;
    }
    
    public void MoveAlongPath(CharacterInfo character, OverlayTile clickedTile)
    {
        var step = Speed * Time.deltaTime;
        
        float zIndex = path[0].transform.position.z;
        Vector3 pathPosition = new Vector3(path[0].transform.position.x, path[0].transform.position.y + 0.15f, path[0].transform.position.z + 5);
        
        if (playingFootStep == false)
        {
            audioManager.PlayBgm(true,AudioManager.Bgm.FootStep);
            playingFootStep = true;
        }
        
        character.transform.position = Vector3.MoveTowards(character.transform.position, pathPosition, step);
        CharacterControl characterControl = character.gameObject.GetOrAddComponent<CharacterControl>();
        characterControl.Character.SetState(AnimationState.Running);
        characterControl.MoveDust.Play();
        
        // 캐릭터 바라보는 방향 설정
        if (character.transform.position.x < pathPosition.x)
        {
            if (character.transform.localScale.x < 0.0f)
            {
                var scale = character.transform.localScale;
                var velocityModule = characterControl.MoveDust.GetComponent<ParticleSystem>().velocityOverLifetime;
                velocityModule.x = new ParticleSystem.MinMaxCurve(-2.0f);
                scale.x = -1 * scale.x;
                character.transform.localScale = scale;
            }
        }
        else if(character.transform.position.x > pathPosition.x)
        {
            if (character.transform.localScale.x > 0.0f)
            {
                var scale = character.transform.localScale;
                var velocityModule = characterControl.MoveDust.GetComponent<ParticleSystem>().velocityOverLifetime;
                velocityModule.x = new ParticleSystem.MinMaxCurve(2.0f);
                scale.x = Mathf.Sign(-1) * scale.x;
                character.transform.localScale = scale;
            }
        }

        if(Vector2.Distance(character.transform.position, pathPosition) < 0.00001f)
        {
            PositionCharacterOnTile(path[0], character);
            path.RemoveAt(0);
        }

        if (path.Count == 0)
        {
            ActionCost++;
            character.standingOnTile.isUnitOn = true;
            isMove = false;
            isConfirmed = false;

            // 캐릭터가 스포너 위에 있는지 확인 후 있다면 5칸 도발
            if (SpawningPoolManager.Instance.IsPlayerOnSpawner(character))
            {
                TauntEnemy(character);
            }
            
            // 캐릭터가 두 번 행동했을 때
            if (ActionCost == 2)
            {
                isMove = false;

                HideCurrentTiles();
                
                Debug.Log("player가 두 번의 행동을 끝냈습니다.");
                //BlockButton();

                ActionCost = 0;
            }
            
            // 이동이 끝나면 화살표 표시
            characterControl.Character.SetState(AnimationState.Idle);
            characterControl.MoveDust.Stop();
            battleSystem.downArrowMove.ShowDownArrow();
            battleSystem.actionState = ActionState.DEFAULT;
            isPaused = false;
            if (playingFootStep == true)
            {
                audioManager.PlayBgm(false,AudioManager.Bgm.FootStep);
                playingFootStep = false; 
            }
        }
    }
    
}
