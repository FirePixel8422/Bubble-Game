using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] protected GameObject bubble;
    [SerializeField] private GunSo so;
    [SerializeField] protected Transform shootPos;
    public int remainingAmmo { get; private set; }
    public int maxAmmo { get; private set; }
    private int _reloadTime;
    private Task _currentTask;
    [SerializeField] private Animator animator;
    [HideInInspector] public float fireRate;

    private void Start()
    {
        maxAmmo = so.maxAmmo;
        _reloadTime = so.reloadTime;
        remainingAmmo = maxAmmo;
        fireRate = so.fireRate;
    }

    public virtual void Shoot()
    {
        print("shooting");
        if (_currentTask != null) return;
        if (remainingAmmo <= 0)
        {
            Reload(_reloadTime);
            return;
        }
        GameObject b = Instantiate(bubble, shootPos.position, shootPos.rotation);
        b.GetComponent<Bullet>().SetVariables(transform.root.gameObject, so.damage, so.speed);
        remainingAmmo--;
    }

    public void PrematureReload()
    {
        if (_currentTask != null) return;
        int t = _reloadTime - (remainingAmmo * maxAmmo * 10);
        Reload(t);
    }
    private async void Reload(int time)
    {
        _currentTask = Task.Delay(_reloadTime);
        await _currentTask;
        remainingAmmo = maxAmmo;
        _currentTask = null;
        
    }
}
