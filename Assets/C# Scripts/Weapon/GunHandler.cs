using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunHandler : MonoBehaviour
{
    private Gun _g;
    private bool _hasShot;
    public bool isBlocking;

    private void Start()
    {
        StartCoroutine(ShootDelay());
        _g = GetComponent<Gun>();
    }


    public void Shoot(InputAction.CallbackContext ctx)
    {
        if (isBlocking) return;
        if (!ctx.started) return;
        if (_g != null && !_hasShot)
        {
            _g.Shoot();
            _hasShot = true;
        }
    }

    public void Reload(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;
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
