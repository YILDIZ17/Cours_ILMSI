using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float Speed = 10f;

    [SerializeField]
    private float SpeedDecrease = 0.9f;

    [SerializeField]
    private Rigidbody Body;

    private Vector2 _movement;

    [SerializeField]
    private int HP = 10;

    [SerializeField]
    private Slider HPSlider;

    [SerializeField]
    private float JumpForce = 7f;

    [SerializeField]
    private LayerMask GroundLayers;

    [SerializeField]
    private float GroundCheckDistance = 0.6f;

    [SerializeField]
    private GameObject GameOverScreen;

    void OnMove(InputValue value)
    {
        _movement = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        if (_movement.magnitude > 0)
        {
            Body.linearVelocity = new Vector3(_movement.x * Speed, Body.linearVelocity.y, _movement.y * Speed);
        }
        else
        {
            Body.linearVelocity = new Vector3(
                Body.linearVelocity.x * SpeedDecrease,
                Body.linearVelocity.y,
                Body.linearVelocity.z * SpeedDecrease
            );
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && IsGrounded())
        {
            Body.linearVelocity = new Vector3(Body.linearVelocity.x, JumpForce, Body.linearVelocity.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Obstacle o = other.GetComponent<Obstacle>();
        if(o != null)
        {
            int damages = o.Explode();
            HP -= damages;
            HPSlider.value = HP;
            if (HP <= 0)
            {
                enabled = false;
                GameOverScreen.SetActive(true);
            }
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, GroundCheckDistance, GroundLayers);
    }
}
