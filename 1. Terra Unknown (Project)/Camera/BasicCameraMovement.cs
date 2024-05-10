using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class BasicCameraMovement : MonoBehaviour
{
    

    public GameObject cineManager;
    public bool updateFreeCamPos = false;
    public bool isActionStart = false;
    public bool isActionEnd = false;
    
    [SerializeField] private CinemachineVirtualCamera freeCam;
    [SerializeField] private CinemachineVirtualCamera battleCam1;
    [SerializeField] private CinemachineVirtualCamera battleCam2;
    [SerializeField] private float orthoSizeMax = 4.5f;
    [SerializeField] private float orthoSizeMin = 2.5f;
    private CinemachineSwitcher cineSwitcher;

    private bool dragPanMoveActive;
    private Vector2 lastMousePosition;
    private float targetOrthoSize = 3.5f;
    
    private Bounds cameraBounds;
    private Vector3 targetPosition;

    private void Start()
    {
        cineSwitcher = cineManager.GetComponent<CinemachineSwitcher>();
    }

    private void Update()
    {
        HandleCameraMovement();
        HandleCameraMovementDragPan();
        HandleCameraZoom();
        UpdateFreeCamPosition();
        ActionZoom();
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private void UpdateFreeCamPosition()
    {
        if (updateFreeCamPos == true)
        {
            if (battleCam1.Priority == 2)
            {
                gameObject.transform.position = battleCam1.transform.position;
            } 
            else if (battleCam2.Priority == 2)
            {
                gameObject.transform.position = battleCam2.transform.position;
            }
        }
    }
    
    // wasd 로 카메라 이동
    private void HandleCameraMovement()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W))
        {
            updateFreeCamPos = false;
            cineSwitcher.SwitchPriorityToFreeCam();
            freeCam.Follow = gameObject.transform;
            freeCam.LookAt = gameObject.transform;
            inputDir.y = +1f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            updateFreeCamPos = false;
            cineSwitcher.SwitchPriorityToFreeCam();
            freeCam.Follow = gameObject.transform;
            freeCam.LookAt = gameObject.transform;
            inputDir.y = -1f;
        }

        if (Input.GetKey(KeyCode.A))
        {
            updateFreeCamPos = false;
            cineSwitcher.SwitchPriorityToFreeCam();
            freeCam.Follow = gameObject.transform;
            freeCam.LookAt = gameObject.transform;
            inputDir.x = -1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            updateFreeCamPos = false;
            cineSwitcher.SwitchPriorityToFreeCam();
            freeCam.Follow = gameObject.transform;
            freeCam.LookAt = gameObject.transform;
            inputDir.x = +1f;
        }
        
        float moveSpeed = 10f;
        transform.position += inputDir * moveSpeed * Time.deltaTime;
    }
    
    

    // 우클릭으로 카메라 이동 
    private void HandleCameraMovementDragPan()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);
        
        if (Input.GetMouseButtonDown(1))
        {
            updateFreeCamPos = false;
            cineSwitcher.SwitchPriorityToFreeCam();
            dragPanMoveActive = true;
            freeCam.Follow = gameObject.transform;
            freeCam.LookAt = gameObject.transform;
            
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(1))
        {
            dragPanMoveActive = false;
        }

        if (dragPanMoveActive)
        {
            Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - lastMousePosition;

            float dragPanSpped = 0.15f;
            inputDir.x = -mouseMovementDelta.x * dragPanSpped;
            inputDir.y = -mouseMovementDelta.y * dragPanSpped;
            
            lastMousePosition = Input.mousePosition;
        }

        float moveSpeed = 7f;
        transform.position += inputDir * moveSpeed * Time.deltaTime;
        
    }

    // 마우스 휠로 카메라 줌인 줌아웃
    private void HandleCameraZoom()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            targetOrthoSize -= 0.3f;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            targetOrthoSize += 0.3f;
        }

        targetOrthoSize = Mathf.Clamp(targetOrthoSize, orthoSizeMin, orthoSizeMax);

        float zoomSpeed = 10f;
        battleCam1.m_Lens.OrthographicSize = Mathf.Lerp(battleCam1.m_Lens.OrthographicSize, targetOrthoSize, Time.deltaTime * zoomSpeed);
        battleCam2.m_Lens.OrthographicSize = Mathf.Lerp(battleCam1.m_Lens.OrthographicSize, targetOrthoSize, Time.deltaTime * zoomSpeed);
        freeCam.m_Lens.OrthographicSize = Mathf.Lerp(battleCam1.m_Lens.OrthographicSize, targetOrthoSize, Time.deltaTime * zoomSpeed);
    }

    public void ActionZoom()
    {
        float zoomSpeed = 15f;

        if (isActionStart)
        {
            targetOrthoSize -= 0.7f;
            isActionStart = false;
        } 
        else if (isActionEnd)
        {
            targetOrthoSize += 0.7f;
            isActionEnd = false;
        }

        targetOrthoSize = Mathf.Clamp(targetOrthoSize, orthoSizeMin, orthoSizeMax);
        
        battleCam1.m_Lens.OrthographicSize = Mathf.Lerp(battleCam1.m_Lens.OrthographicSize, targetOrthoSize, Time.deltaTime * zoomSpeed);
        battleCam2.m_Lens.OrthographicSize = Mathf.Lerp(battleCam1.m_Lens.OrthographicSize, targetOrthoSize, Time.deltaTime * zoomSpeed);
        freeCam.m_Lens.OrthographicSize = Mathf.Lerp(battleCam1.m_Lens.OrthographicSize, targetOrthoSize, Time.deltaTime * zoomSpeed);
    }

    // 전투시 카메라 전환을 위한 cinemachine vcam 설정 변경
    public void ChangeVcamFollow(GameObject obj, int camIdx)
    {
        if (camIdx % 2 == 1)
        {
            battleCam1.Follow = obj.transform;
            battleCam1.LookAt = obj.transform;
        }
        else
        {
            battleCam2.Follow = obj.transform;
            battleCam2.LookAt = obj.transform;
        }
        
        cineSwitcher.SwitchPriorityToBattleCam();
    }

}
