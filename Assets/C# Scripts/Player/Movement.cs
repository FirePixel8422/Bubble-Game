using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Movement : NetworkBehaviour
{
    [SerializeField] private float speed;

    [SerializeField] private Transform rotTransform;

    private Vector2 _input;
    private Rigidbody _rb;



    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        _input = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!IsOwner || !ctx.performed) return;

        if (hasExtraJump || OnGround)
        {
            jumping = true;

            _rb.velocity = new Vector3(0, jumpStrength, 0);


            if (OnGround)
            {
                hasExtraJump = true;
            }
            else
            {
                hasExtraJump = false;
            }
        }
    }



    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!IsOwner) return;


        Vector3 dir = transform.TransformDirection(new Vector3(_input.x, 0, _input.y));

        _rb.velocity = new Vector3(dir.x * speed, _rb.velocity.y, dir.z * speed);

        
        if (jumping)
        {
            UpdateJumpMomentum();
        }


        SyncPlayerTransform_ServerRPC(transform.position, rotTransform.rotation);
    }



    [SerializeField] private bool jumping;
    [SerializeField] private bool hasExtraJump;

    [SerializeField] private float fallVel;
    [SerializeField] private float jumpStrength;

    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float overlapSphereSize;

    public bool OnGround => Physics.OverlapSphere(groundCheckTransform.position, overlapSphereSize, groundLayers).Length > 0;


    private void UpdateJumpMomentum()
    {
        if (_rb.velocity.y < 0.5)
        {
            _rb.velocity -= new Vector3(0, fallVel * Time.deltaTime, 0);

            if (OnGround)
            {
                jumping = false;
            }
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void SyncPlayerTransform_ServerRPC(Vector3 pos, Quaternion rot)
    {
        SyncPlayerTransform_ClientRPC(pos, rot);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SyncPlayerTransform_ClientRPC(Vector3 pos, Quaternion rot)
    {
        //transform gets updated through client that owns this player, updating these values on that client after sending them would flicker the player back and fourth.
        if (NetworkManager.LocalClientId == OwnerClientId) return;

        transform.position = pos;
        rotTransform.rotation = rot;
    }
}
