using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunHandler : MonoBehaviour
{
    public GameObject gun;
    private Gun _g;
    private bool _hasShot;
    public bool isBlocking;

    private void Start()
    {
        StartCoroutine(ShootDelay());
        gun = GetComponentInChildren<Gun>().gameObject;
        _g = gun.GetComponent<Gun>();
    }

    private void OnGunChanged()
    {
        _g = gun.GetComponent<Gun>();
    }

    public void Shoot(InputAction.CallbackContext ctx)
    {
        if (isBlocking) return;
        print("shot");
        if (!ctx.started) return;
        if (_g != null && !_hasShot)
        {
            _g.Shoot();
            _hasShot = true;
        }
    }

    public void Reload(InputAction.CallbackContext ctx)
    {
        print("reload");
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
