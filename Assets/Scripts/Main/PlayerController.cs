using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] Transform _spawnPos;
    [SerializeField] TMP_Text posText;
    private Vector2 _moveDirection;
    private Rigidbody _rb;


    public void OnMove(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>();
    }

    private void Awake()
    {
        
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        transform.position = _spawnPos.position;
        StartCoroutine(SpawnIgnoreCollision());
    }

    void FixedUpdate()
    {
        Move(_moveDirection);
        posText.text = DisplayCoordinates(transform.position);
    }

    private void Move(Vector2 direction)
    {
        var velocity  = new Vector3(_moveDirection.x, 0f, _moveDirection.y) * _moveSpeed;
        velocity.y = _rb.velocity.y;
        var worldVelocity = transform.TransformVector(velocity);
        _rb.velocity = worldVelocity;
    }

    IEnumerator SpawnIgnoreCollision()
    {
        yield return new WaitForSeconds(5);
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    string DisplayCoordinates(Vector3 pos)
    {
        return $"x: {pos.x}\ny: {pos.z}";
    }

}
