using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeMapCameraMovement : MonoBehaviour
{
    private bool dragPanMoveActive;
    private bool useEdgeScrolling;
    private Vector2 lastMousePosition;

    private void Update()
    {
        HandleCameraMovement();
        HandleDragPan();
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private void HandleCameraMovement()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);
        
        if (Input.GetKey(KeyCode.A)) inputDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) inputDir.x = +1f;
        
        Vector3 moveDir = transform.right * inputDir.x;
        
        float moveSpeed = 15f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void HandleDragPan()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);
        
        if (Input.GetMouseButtonDown(1))
        {
            dragPanMoveActive = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1))
        {
            dragPanMoveActive = false;
        }

        if (dragPanMoveActive)
        {
            Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - lastMousePosition;

            float dragPanSpeed = 0.15f;
            inputDir.x = -mouseMovementDelta.x * dragPanSpeed;
            
            lastMousePosition = Input.mousePosition;
        }
        
        float moveSpeed = 7f;
        transform.position += inputDir * moveSpeed * Time.deltaTime;
    }
}
