using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] Transform _target;
    [SerializeField] float _lerpRate;

    private void LateUpdate()
    {
        Vector3 moveDistance = new(_target.position.x, 5f, _target.position.z);
        transform.position = Vector3.Lerp(transform.position, moveDistance, Time.deltaTime * _lerpRate);
    }
}
