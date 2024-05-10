using System;
using Cinemachine;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
public class CinemachineSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera freeCam;
    public CinemachineVirtualCamera battleCam1;
    public CinemachineVirtualCamera battleCam2;
    

    // priority 값을 변경하여 가상 카메라를 변경하는 메소드
    public void SwitchPriorityToFreeCam()
    {
        if (battleCam1.Priority == 2)
        {
            battleCam1.Priority = 0;
            battleCam2.Priority = 1;
            freeCam.Priority = 2;
        }
        else if (battleCam2.Priority == 2)
        {
            battleCam2.Priority = 0;
            battleCam1.Priority = 1;
            freeCam.Priority = 2;
        }
    }
    public void SwitchPriorityToBattleCam()
    {
        if (freeCam.Priority == 2)
        {
            if (battleCam1.Priority == 1)
            {
                freeCam.Priority = 0;
                battleCam1.Priority = 2;
                battleCam2.Priority = 1;
            }
            else if (battleCam2.Priority == 1)
            {
                freeCam.Priority = 0;
                battleCam2.Priority = 2;
                battleCam1.Priority = 1;
            }
        }
        else
        {
            if (battleCam1.Priority == 2)
            {
                freeCam.Priority = 0;
                battleCam1.Priority = 1;
                battleCam2.Priority = 2;
            }
            else if (battleCam2.Priority == 2)
            {
                freeCam.Priority = 0;
                battleCam2.Priority = 1;
                battleCam1.Priority = 2;
            }
        }
    }
}
