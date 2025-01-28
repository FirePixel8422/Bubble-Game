using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jump;
    private Vector2 _input;
    private Rigidbody _rb;
    private bool _grounded;
    private bool _isCoroutineRunning;
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        transform.Translate(new Vector3(_input.x, 0, _input.y) * (speed * Time.deltaTime));
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _input = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (_grounded)
        {
            print("Jumped");
            _rb.AddForce(0, jump * speed, 0);
            _grounded = false;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        _grounded = true;
        // if (collision.collider.CompareTag("Ground"))
        // {
        //     
        // }
    }
}
