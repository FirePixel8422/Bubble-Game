using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Movement : NetworkBehaviour
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
        if (!IsOwner) return;

        transform.Translate(new Vector3(_input.x, 0, _input.y) * (speed * Time.deltaTime));

        SyncPlayerTransform_ServerRPC(transform.position, transform.GetChild(0).rotation);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncPlayerTransform_ServerRPC(Vector3 pos, Quaternion rot)
    {
        SyncPlayerTransform_ClientRPC(pos, rot);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SyncPlayerTransform_ClientRPC(Vector3 pos, Quaternion rot)
    {

    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        _input = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (_grounded)
        {
            //print("Jumped");
            _rb.AddForce(0, jump * speed, 0);
            _grounded = false;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;
        _grounded = true;
        // if (collision.collider.CompareTag("Ground"))
        // {
        //     
        // }
    }
}
