using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunHandler : NetworkBehaviour
{
    private Gun _g;
    private bool _hasShot;
    public bool isBlocking;

    private void Start()
    {
        if (!IsOwner) return;

        StartCoroutine(ShootDelay());
        _g = GetComponent<Gun>();
    }

    private bool held;



    private void Update()
    {
        if (held && _g != null && !_hasShot)
        {
            _g.Shoot();
            _hasShot = true;
        }
    }


    public void Shoot(InputAction.CallbackContext ctx)
    {
        if (isBlocking || !IsOwner) return;

        held = ctx.performed;

        if (_g != null && !_hasShot)
        {
            _g.Shoot();
            _hasShot = true;
        }
    }

    public void Reload(InputAction.CallbackContext ctx)
    {
        if (!ctx.started || !IsOwner) return;
        if (_g != null)
        {
            _g.PrematureReload();
        }
    }

    private IEnumerator ShootDelay()
    {
        while (true)
        {
            yield return new WaitUntil(() => _hasShot);
            yield return new WaitForSeconds(_g.fireRate);
            _hasShot = false;
        }
    }
}
