using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera MainCam;
    [SerializeField] private GameObject ObjectToFollow;

    [Header("Camera Settings")]
    [SerializeField] private float ScrollSensitivity = 1f;
    [SerializeField] private float MinScroll = 0f;
    [SerializeField] private float MaxScroll = 20f;
    [SerializeField] private Vector3 PositionFromFollow = new Vector3(0f, 0f, -10f);

    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0f ) // forward
        {
            MainCam.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * ScrollSensitivity;
            if (MainCam.orthographicSize < MinScroll) MainCam.orthographicSize = MinScroll;
            if (MainCam.orthographicSize > MaxScroll) MainCam.orthographicSize = MaxScroll;
        }

        if (ObjectToFollow != null) 
        {
            transform.position = ObjectToFollow.transform.position + PositionFromFollow;
        }
    }
}
