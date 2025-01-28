using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shield : MonoBehaviour
{
    [SerializeField] private GameObject shield;
    private GunHandler _gun;
    [SerializeField] private int shieldHealth;

    private void Start()
    {
        _gun = GetComponent<GunHandler>();
    }

    public void OnShield(InputAction.CallbackContext ctx)
    {
        if (shieldHealth <= 10) return;
        if (ctx.performed)
        {
            _gun.isBlocking = true;
            shield.SetActive(true);
            Debug.Log("Shield is active");
        }
        else if (ctx.canceled)
        {
            _gun.isBlocking = false;
            shield.SetActive(false);
            Debug.Log("Shield is canceled");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Bullet b))
        {
            shieldHealth -= b.damage;
            if (shieldHealth <= 10)
            {
                shield.SetActive(false);
            } 
            Destroy(other.gameObject);
        }
    }
}
